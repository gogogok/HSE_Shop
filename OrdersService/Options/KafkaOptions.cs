namespace OrdersService.Options
{
    /// <summary>
    /// Настройки Kafka
    /// </summary>
    public sealed class KafkaOptions
    {
        /// <summary>
        /// Адреса брокеров
        /// </summary>
        public string BootstrapServers { get; set; } = "";
        
        /// <summary>
        /// Топик запроса оплаты
        /// </summary>
        public string OrderPaymentRequestedTopic { get; set; } = "order.payment.requested";
        
        /// <summary>
        /// Топик результата оплаты
        /// </summary>
        public string PaymentResultTopic { get; set; } = "payment.result";
        
        /// <summary>
        /// Consumer группа
        /// </summary>
        public string ConsumerGroupId { get; set; } = "orders-service";
    }
}