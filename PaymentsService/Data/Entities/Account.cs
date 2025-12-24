namespace PaymentsService.Data.Entities
{
    /// <summary>
    /// Сущность аккаунта
    /// </summary>
    public sealed class Account
    {
        /// <summary>
        /// Id аккаунта
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Id пользователя
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Баланс аккаунта
        /// </summary>
        public decimal Balance { get; set; }
        
        /// <summary>
        /// Дата создания аккаунта
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}