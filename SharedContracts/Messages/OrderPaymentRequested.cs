namespace SharedContracts.Messages
{
    /// <summary>
    /// Запрос на оплату
    /// </summary>
    public sealed record OrderPaymentRequested(string OrderId, string UserId, decimal Amount);
}