namespace OrdersService.Options
{
    /// <summary>
    /// Настройки базы данных
    /// </summary>
    public sealed class DbOptions
    {
        /// <summary>
        /// Подключение к БД
        /// </summary>
        public string ConnectionString { get; set; } = "";
    }
}