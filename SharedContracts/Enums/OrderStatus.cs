namespace SharedContracts.Enums
{
    /// <summary>
    /// Статусы заказов
    /// </summary>
    public enum OrderStatus
    {
        Created = 0,
        PaymentRequested = 1,
        Paid = 2,
        Rejected = 3
    }
}