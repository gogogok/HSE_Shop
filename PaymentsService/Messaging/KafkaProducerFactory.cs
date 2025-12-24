using Confluent.Kafka;
using Microsoft.Extensions.Options;
using PaymentsService.Options;

namespace PaymentsService.Messaging
{
    /// <summary>
    /// Фабрика Kafka producer
    /// </summary>
    public interface IKafkaProducerFactory
    {
        /// <summary>
        /// Создание Kafka producer
        /// </summary>
        IProducer<string, string> Create();
    }

    /// <summary>
    /// Реализация Kafka producer
    /// </summary>
    public sealed class KafkaProducerFactory : IKafkaProducerFactory
    {
        /// <summary>
        /// Настройки Kafka
        /// </summary>
        private readonly KafkaOptions _opt;

        /// <summary>
        /// Конструктор фабрики producer
        /// </summary>
        public KafkaProducerFactory(IOptions<KafkaOptions> opt)
        {
            _opt = opt.Value;
        }

        /// <summary>
        /// Создание producer
        /// </summary>
        public IProducer<string, string> Create()
        {
            //конфигурация producer
            ProducerConfig cfg = new ProducerConfig
            {
                BootstrapServers = _opt.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true
            };

            return new ProducerBuilder<string, string>(cfg).Build();
        }
    }

}