using OrdersService.Data;
using OrdersService.Data.Entities;
using OrdersService.Messaging;
using OrdersService.Options;
using SharedContracts.Dtos;
using SharedContracts.Enums;
using SharedContracts.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;

namespace OrdersService.Services
{
    /// <summary>
    /// Бизнес-логика заказов
    /// </summary>
    public sealed class OrdersAppService
    {
        /// <summary>
        /// База данных заказов
        /// </summary>
        private readonly OrdersDbContext _db;
        
        /// <summary>
        /// JSON сериализатор сообщений
        /// </summary>
        private readonly IJsonMessageSerializer _json;
        
        /// <summary>
        /// Kafka настройки
        /// </summary>
        private readonly KafkaOptions _kafka;
        
        /// <summary>
        /// HTTP клиенты сервисов
        /// </summary>
        private readonly IHttpClientFactory _httpFactory;


        /// <summary>
        /// Конструктор OrdersAppService
        /// </summary>
        public OrdersAppService(
            OrdersDbContext db,
            IJsonMessageSerializer json,
            IOptions<KafkaOptions> kafka,
            IHttpClientFactory httpFactory)
        {
            _db = db;
            _json = json;
            _kafka = kafka.Value;
            _httpFactory = httpFactory;
        }

        /// <summary>
        /// Создание нового заказа
        /// </summary>
        /// <param name="req">Данные нового заказа</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>ID и статус заказа</returns>
        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.UserId))
            {
                throw new ArgumentException("UserId is required");
            }
            
            HttpClient payments = _httpFactory.CreateClient("payments");

            HttpResponseMessage resp = await payments.GetAsync(
                $"/accounts/exists?userId={Uri.EscapeDataString(req.UserId)}",
                ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException("account_not_found");
            }

            resp.EnsureSuccessStatusCode();

            if (req.Amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(req.Amount), "Заказ должен содержать больше 0 предметов");
            }
            string orderId = Guid.NewGuid().ToString("N");
            await using IDbContextTransaction tx = await _db.Database.BeginTransactionAsync(ct);

            Order order = new Order
            {
                OrderId = orderId,
                UserId = req.UserId,
                Amount = req.Amount,
                Status = OrderStatus.PaymentRequested,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _db.Orders.Add(order);

            //создаём сообщение в outbox
            OrderPaymentRequested msg = new OrderPaymentRequested(orderId, req.UserId, req.Amount);

            _db.OutboxMessages.Add(new OutboxMessage
            {
                MessageType = nameof(OrderPaymentRequested),
                Topic = _kafka.OrderPaymentRequestedTopic,
                Key = orderId,
                PayloadJson = _json.Serialize(msg)
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new CreateOrderResponse(orderId, order.Status.ToString());
        }

       
        /// <summary>
        /// Получить заказ по ID
        /// </summary>
        /// <param name="orderId">Id заказа</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Заказ или null</returns>
        public async Task<OrderDto?> GetOrderAsync(string orderId, CancellationToken ct)
        {
            Order? o = await _db.Orders.SingleOrDefaultAsync(x => x.OrderId == orderId, ct);
            if (o is null)
            {
                return null;
            }

            return new OrderDto(o.OrderId, o.UserId, o.Amount, o.Status);
        }

        /// <summary>
        /// Список заказов пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Список заказов</returns>
        public async Task<List<OrderDto>> ListOrdersAsync(string userId, CancellationToken ct)
        {
            return await _db.Orders
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => new OrderDto(x.OrderId, x.UserId, x.Amount, x.Status))
                .ToListAsync(ct);
        }

        /// <summary>
        /// Применить результат оплаты
        /// </summary>
        /// <param name="pr">Результат платежа</param>
        /// <param name="ct">Токен отмены</param>
        public async Task ApplyPaymentResultAsync(PaymentResult pr, CancellationToken ct)
        {
            Order? order = await _db.Orders.SingleOrDefaultAsync(x => x.OrderId == pr.OrderId, ct);
            if (order is null)
            {
                return;
            }
            //защита от повторов
            if (order.Status is OrderStatus.Paid or OrderStatus.Rejected)
            {
                return;
            }
            order.Status = pr.Success ? OrderStatus.Paid : OrderStatus.Rejected;
            order.LastPaymentError = pr.Success ? null : pr.Reason;
            order.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
