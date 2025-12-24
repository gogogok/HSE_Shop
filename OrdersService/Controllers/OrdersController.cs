using OrdersService.Services;
using SharedContracts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace OrdersService.Controllers
{
    [ApiController]
    [Route("orders")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly OrdersAppService _svc;

        public OrdersController(OrdersAppService svc)
        {
            _svc = svc;
        }

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

        [HttpGet]
        public async Task<ActionResult<List<OrderDto>>> List([FromQuery] string userId, CancellationToken ct)
        {
            List<OrderDto> list = await _svc.ListOrdersAsync(userId, ct);
            return Ok(list);
        }
    }
}