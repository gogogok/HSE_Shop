namespace SharedContracts.Dtos
{
    /// <summary>
    /// Ответ о статусе платежа заказа
    /// </summary>
    /// <param name="OrderId">Id заказа</param>
    /// <param name="Status">Статус оплаты</param>
    public sealed record CreateOrderResponse(string OrderId, string Status);
}