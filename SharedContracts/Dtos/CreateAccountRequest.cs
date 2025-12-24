namespace SharedContracts.Dtos
{
    /// <summary>
    /// Реквест на создание аккаунта
    /// </summary>
    /// <param name="UserId">Id пользователя</param>
    public sealed record CreateAccountRequest(string UserId);
}