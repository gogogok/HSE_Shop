using Confluent.Kafka;
using OrdersService.Options;
using OrdersService.Services;
using SharedContracts.Messages;
using Microsoft.Extensions.Options;

namespace OrdersService.Messaging
{
    public sealed class PaymentResultConsumerHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IJsonMessageSerializer _json;
        private readonly KafkaOptions _opt;
        private readonly ILogger<PaymentResultConsumerHostedService> _log;

        public PaymentResultConsumerHostedService(
            IServiceScopeFactory scopeFactory,
            IJsonMessageSerializer json,
            IOptions<KafkaOptions> opt,
            ILogger<PaymentResultConsumerHostedService> log)
        {
            _scopeFactory = scopeFactory;
            _json = json;
            _opt = opt.Value;
            _log = log;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                ConsumerConfig cfg = new ConsumerConfig
                {
                    BootstrapServers = _opt.BootstrapServers,
                    GroupId = _opt.ConsumerGroupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using IConsumer<string, string>? consumer = new ConsumerBuilder<string, string>(cfg).Build();
                consumer.Subscribe(_opt.PaymentResultTopic);

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        ConsumeResult<string, string>? cr = null;

                        try
                        {
                            cr = consumer.Consume(stoppingToken);
                            if (cr?.Message?.Value is null)
                            {
                                continue;
                            }

                            PaymentResult pr = _json.Deserialize<PaymentResult>(cr.Message.Value);

                            using IServiceScope scope = _scopeFactory.CreateScope();
                            OrdersAppService svc = scope.ServiceProvider.GetRequiredService<OrdersAppService>();

                            svc.ApplyPaymentResultAsync(pr, stoppingToken).GetAwaiter().GetResult();

                            consumer.Commit(cr);
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Consume PaymentResult failed topic={Topic} offset={Offset}",
                                cr?.Topic, cr?.Offset.Value);
                            Thread.Sleep(500);
                        }
                    }
                }
                finally
                {
                    consumer.Close();
                }
            }, stoppingToken);
        }
    }
}
