using SharedContracts.Enums;

namespace OrdersService.Data.Entities
{
    public sealed class Order
    {
        public long Id { get; set; }
        public string OrderId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public decimal Amount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        public string? LastPaymentError { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}