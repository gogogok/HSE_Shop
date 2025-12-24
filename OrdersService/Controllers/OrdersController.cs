using OrdersService.Services;
using SharedContracts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers
{
    /// <summary>
    /// Контроллер заказов
    /// </summary>
    [ApiController]
    [Route("orders")]
    public sealed class OrdersController : ControllerBase
    {
        /// <summary>
        /// Сервис заказов
        /// </summary>
        private readonly OrdersAppService _svc;

        /// <summary>
        /// Конструктор OrdersController
        /// </summary>
        /// <param name="svc">Сервис заказов</param>
        public OrdersController(OrdersAppService svc)
        {
            _svc = svc;
        }
        
        /// <summary>
        /// Создание нового заказа
        /// </summary>
        /// <param name="req">Данные заказа</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Созданный заказ</returns>
        [HttpPost]
        public async Task<ActionResult<CreateOrderResponse>> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
        {
            try
            {
                CreateOrderResponse res = await _svc.CreateOrderAsync(req, ct);
                return Ok(res);
            }
            catch (InvalidOperationException ex) when (ex.Message == "account_not_found")
            {
                return NotFound(new { error = "account_not_found", userId = req.UserId });
            }
        }

        /// <summary>
        /// Получение заказа по ID
        /// </summary>
        /// <param name="orderId">Id заказа</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Информация о заказе</returns>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDto>> Get(string orderId, CancellationToken ct)
        {
            OrderDto? dto = await _svc.GetOrderAsync(orderId, ct);
            if (dto is null)
            {
                return NotFound(new { error = "order_not_found", orderId });
            }

            return Ok(dto);
        }

        /// <summary>
        /// Список заказов пользователя
        /// </summary>
        /// <param name="userId">Шв пользователя</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Список заказов</returns>
        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> List([FromQuery] string userId, CancellationToken ct)
        {
            List<OrderDto> list = await _svc.ListOrdersAsync(userId, ct);
            return Ok(list);
        }
    }
}