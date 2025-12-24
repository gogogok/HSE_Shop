namespace SharedContracts.Dtos
{
    /// <summary>
    /// Ответ от сервера о балансе аккаунта
    /// </summary>
    /// <param name="UserId">Id пользователя</param>
    /// <param name="Balance">Баланс</param>
    public sealed record BalanceResponse(string UserId, decimal Balance);
}