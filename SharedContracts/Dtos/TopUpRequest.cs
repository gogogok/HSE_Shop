namespace SharedContracts.Dtos
{
    /// <summary>
    /// Запрос о пополнении счёта
    /// </summary>
    /// <param name="UserId">Id пользователя</param>
    /// <param name="Amount">Количество заказанного</param>
    public sealed record TopUpRequest(string UserId, decimal Amount);
}