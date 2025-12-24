using Confluent.Kafka;
using PaymentsService.Data;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Data.Entities;

namespace PaymentsService.Messaging
{
    /// <summary>
    /// Публикация сообщений Outbox
    /// </summary>
    public sealed class OutboxPublisherHostedService : BackgroundService
    {
        /// <summary>
        /// Фабрика скоупов
        /// </summary>
        private readonly IServiceScopeFactory _scopeFactory;

        /// <summary>
        /// Kafka producer factory
        /// </summary>
        private readonly IKafkaProducerFactory _producerFactory;

        /// <summary>
        /// Логирование сервиса
        /// </summary>
        private readonly ILogger<OutboxPublisherHostedService> _log;

        /// <summary>
        /// Размер сообщений
        /// </summary>
        private readonly int _batchSize;

        /// <summary>
        /// Интервал опроса Outbox
        /// </summary>
        private readonly int _pollMs;

        
        /// <summary>
        /// Конструктор OutboxPublisherHostedService
        /// </summary>
        public OutboxPublisherHostedService(
            IServiceScopeFactory scopeFactory,
            IKafkaProducerFactory producerFactory,
            IConfiguration cfg,
            ILogger<OutboxPublisherHostedService> log)
        {
            _scopeFactory = scopeFactory;
            _producerFactory = producerFactory;
            _log = log;
            _batchSize = cfg.GetValue("Outbox:BatchSize", 20);
            _pollMs = cfg.GetValue("Outbox:PollIntervalMs", 1000);
        }

        /// <summary>
        /// Основной цикл публикации
        /// </summary>
        /// <param name="stoppingToken">Токен остановки</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using IProducer<string, string> producer = _producerFactory.Create();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    PaymentsDbContext db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

                    List<OutboxMessage> batch = await db.OutboxMessages
                        .Where(x => x.PublishedAtUtc == null)
                        .OrderBy(x => x.Id)
                        .Take(_batchSize)
                        .ToListAsync(stoppingToken);

                    if (batch.Count == 0)
                    {
                        await Task.Delay(_pollMs, stoppingToken);
                        continue;
                    }

                    foreach (OutboxMessage m in batch)
                    {
                        try
                        {
                            DeliveryResult<string, string>? dr = await producer.ProduceAsync(
                                m.Topic,
                                new Message<string, string> { Key = m.Key, Value = m.PayloadJson },
                                stoppingToken);

                            m.PublishedAtUtc = DateTime.UtcNow;
                            m.PublishAttempts += 1;
                            m.LastError = null;

                            _log.LogInformation("Outbox published id={Id} topic={Topic} offset={Offset}",
                                m.Id, m.Topic, dr.Offset.Value);
                        }
                        catch (Exception ex)
                        {
                            m.PublishAttempts += 1;
                            m.LastError = ex.Message;
                            _log.LogWarning(ex, "Outbox publish failed id={Id} topic={Topic}", m.Id, m.Topic);
                        }
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Outbox publisher loop failed");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
    }
}
