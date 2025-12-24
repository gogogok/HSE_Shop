namespace OrdersService.Data.Entities
{
    /// <summary>
    /// Сообщение outbox для Kafka
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
        /// Ключ сообщения
        /// </summary>
        public string Key { get; set; } = null!;
        
        /// <summary>
        /// Тело сообщения
        /// </summary>
        public string PayloadJson { get; set; } = null!;
        
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Дата публикации
        /// </summary>
        public DateTime? PublishedAtUtc { get; set; }
        
        /// <summary>
        /// Количество попыток
        /// </summary>
        public int PublishAttempts { get; set; }
        
        /// <summary>
        /// Последняя ошибка
        /// </summary>
        public string? LastError { get; set; }
    }
}