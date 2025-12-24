namespace PaymentsService.Data.Entities
{
    /// <summary>
    /// Сущность Outbox сообщений Kafka
    /// </summary>
    public sealed class OutboxMessage
    {
        /// <summary>
        /// Id записи
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// Тип сообщения
        /// </summary>
        public string MessageType { get; set; } = null!;
        
        /// <summary>
        /// Kafka топик
        /// </summary>
        public string Topic { get; set; } = null!;
        
        /// <summary>
        /// Kafka ключ
        /// </summary>
        public string Key { get; set; } = null!;
        
        /// <summary>
        /// JSON payload
        /// </summary>
        public string PayloadJson { get; set; } = null!;
        
        /// <summary>
        /// Время создания
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Время публикации
        /// </summary>
        public DateTime? PublishedAtUtc { get; set; }
        
        
        /// <summary>
        /// Попытки публикации
        /// </summary>
        public int PublishAttempts { get; set; }
        
        /// <summary>
        /// Последняя ошибка
        /// </summary>
        public string? LastError { get; set; }
    }
}