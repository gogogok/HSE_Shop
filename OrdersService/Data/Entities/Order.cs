using SharedContracts.Enums;

namespace OrdersService.Data.Entities
{
    /// <summary>
    /// Сущность заказа
    /// </summary>
    public sealed class Order
    {
        /// <summary>
        /// Внутренний Id
        /// </summary>
        public long Id { get; set; }
        
        /// <summary>
        /// Id заказа
        /// </summary>
        public string OrderId { get; set; } = null!;
        
        /// <summary>
        /// Id пользователя
        /// </summary>
        public string UserId { get; set; } = null!;
        
        /// <summary>
        /// Сумма заказа
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Статус заказа
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Created;
        
        /// <summary>
        /// Ошибка оплаты
        /// </summary>
        public string? LastPaymentError { get; set; }
        
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }
    }
}