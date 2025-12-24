using System.Text.Json;

namespace PaymentsService.Messaging
{
    /// <summary>
    /// JSON сериализация сообщений
    /// </summary>
    public interface IJsonMessageSerializer
    {
        /// <summary>
        /// Объект в JSON
        /// </summary>
        string Serialize<T>(T value);
        
        /// <summary>
        /// JSON в объект
        /// </summary>
        T Deserialize<T>(string json);
    }

    /// <summary>
    /// Реализация JSON сериализатора
    /// </summary>
    public sealed class JsonMessageSerializer : IJsonMessageSerializer
    {
        /// <summary>
        /// Параметры сериализации JSON
        /// </summary>
        private static readonly JsonSerializerOptions Opt = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Сериализация в JSON
        /// </summary>
        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, Opt);
        }

        /// <summary>
        /// Десериализация из JSON
        /// </summary>
        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, Opt)!;
        }
    }
}