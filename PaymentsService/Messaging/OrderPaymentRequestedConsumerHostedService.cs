using Confluent.Kafka;
using PaymentsService.Messaging;
using PaymentsService.Options;
using PaymentsService.Services;
using SharedContracts.Messages;
using Microsoft.Extensions.Options;

namespace PaymentsService.Messaging
{
    /// <summary>
    /// Kafka consumer оплаты заказа
    /// </summary>
    public sealed class OrderPaymentRequestedConsumerHostedService : BackgroundService
    {
        /// <summary>
        /// Фабрика скоупов
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// JSON сериализация сообщений
        /// </summary>
        private readonly IJsonMessageSerializer _json;

        /// <summary>
        /// Kafka настройки
        /// </summary>
        private readonly KafkaOptions _opt;

        /// <summary>
        /// Логирование ошибок
        /// </summary>
        private readonly ILogger<OrderPaymentRequestedConsumerHostedService> _log;

        /// <summary>
        /// Конструктор OrderPaymentRequestedConsumerHostedService
        /// </summary>
        public OrderPaymentRequestedConsumerHostedService(
            IServiceScopeFactory scopeFactory,
            IJsonMessageSerializer json,
            IOptions<KafkaOptions> opt,
            ILogger<OrderPaymentRequestedConsumerHostedService> log)
        {
            _scopeFactory = scopeFactory;
            _json = json;
            _opt = opt.Value;
            _log = log;
        }

        /// <summary>
        /// Запуск consumer цикла
        /// </summary>
        /// <param name="stoppingToken">Токен остановки</param>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                //конфигурация Kafka consumer
                ConsumerConfig cfg = new ConsumerConfig
                {
                    BootstrapServers = _opt.BootstrapServers,
                    GroupId = _opt.ConsumerGroupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                using IConsumer<string, string>? consumer = new ConsumerBuilder<string, string>(cfg).Build();
                consumer.Subscribe(_opt.OrderPaymentRequestedTopic);

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

                            OrderPaymentRequested msg = _json.Deserialize<OrderPaymentRequested>(cr.Message.Value);
                            using IServiceScope scope = _scopeFactory.CreateScope();
                            PaymentProcessingService svc = scope.ServiceProvider.GetRequiredService<PaymentProcessingService>();
                            svc.HandleOrderPaymentRequestedAsync(msg, stoppingToken).GetAwaiter().GetResult();
                            consumer.Commit(cr);
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Consume failed (topic={Topic}, offset={Offset})", cr?.Topic, cr?.Offset.Value);
                            //небольшая пауза, чтобы не спамить в лог при постоянной ошибке
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
