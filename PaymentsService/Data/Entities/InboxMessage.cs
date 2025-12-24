namespace PaymentsService.Data.Entities
{
    /// <summary>
    /// Сущность Inbox сообщений Kafka
    /// </summary>
    public sealed class InboxMessage
    {
        /// <summary>
        /// Id записи
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// Ключ сообщения
        /// </summary>
        public string MessageKey { get; set; } = null!;

        /// <summary>
        /// Тип сообщения
        /// </summary>
        public string MessageType { get; set; } = null!;

        /// <summary>
        /// Время получения
        /// </summary>
        public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    }
}