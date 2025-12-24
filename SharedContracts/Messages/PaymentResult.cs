namespace SharedContracts.Messages
{
    /// <summary>
    /// Результат оплаты
    /// </summary>
    public sealed record PaymentResult(string OrderId, string UserId, decimal Amount, bool Success, string? Reason);
}