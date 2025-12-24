namespace SharedContracts.Dtos
{
    /// <summary>
    /// Реквест на создание заказа
    /// </summary>
    /// <param name="UserId">Id пользователя</param>
    /// <param name="Amount">Количество заказанного</param>
    public sealed record CreateOrderRequest(string UserId, decimal Amount);
}