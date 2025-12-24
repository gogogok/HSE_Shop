using SharedContracts.Enums;

namespace SharedContracts.Dtos
{
    /// <summary>
    /// Dto заказа
    /// </summary>
    /// <param name="OrderId">Id заказа</param>
    /// <param name="UserId">Id пользователя</param>
    /// <param name="Amount">Количество заказанного</param>
    /// <param name="Status">Статус заказа</param>
    public sealed record OrderDto(string OrderId, string UserId, decimal Amount, OrderStatus Status);
}