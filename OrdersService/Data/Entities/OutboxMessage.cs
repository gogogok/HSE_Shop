namespace OrdersService.Data.Entities
{
    public sealed class OutboxMessage
    {
        public long Id { get; set; }
        public string MessageType { get; set; } = null!;
        public string Topic { get; set; } = null!;
        public string Key { get; set; } = null!;
        public string PayloadJson { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAtUtc { get; set; }
        public int PublishAttempts { get; set; }
        public string? LastError { get; set; }
    }
}