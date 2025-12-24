using PaymentsService.Services;
using SharedContracts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace PaymentsService.Controllers
{
    /// <summary>
    /// Контроллер PaymentsService
    /// </summary>
    [ApiController]
    [Route("accounts")]
    public sealed class AccountsController : ControllerBase
    {
        /// <summary>
        /// Сервис аккаунтов
        /// </summary>
        private readonly AccountsAppService _svc;

        /// <summary>
        /// Конструктор AccountsController
        /// </summary>
        /// <param name="svc">Сервис аккаунтов</param>
        public AccountsController(AccountsAppService svc)
        {
            _svc = svc;
        }
        
        /// <summary>
        /// Создать новый аккаунт
        /// </summary>
        /// <param name="req">Данные создания аккаунта</param>
        /// <param name="ct">Токен отмены запроса</param>
        /// <returns>200 создан, 409 уже есть</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequest req, CancellationToken ct)
        {
            try
            {
                await _svc.CreateAccountAsync(req.UserId, ct);
                return Ok(new { status = "created", userId = req.UserId });
            }  catch (InvalidOperationException ex) when (ex.Message == "Аккаунт уже существует")
            {
                return Conflict(new
                {
                    error = "Аккаунт уже существует",
                    userId = req.UserId
                });
            }
        }

        /// <summary>
        /// Проверить наличие аккаунта
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="ct">Токен отмены запроса</param>
        /// <returns>200 если есть, 404 нет</returns>
        [HttpGet("exists")]
        public async Task<IActionResult> Exists([FromQuery] string userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest(new { error = "userId_required" });
            }

            bool exists = await _svc.ExistsAsync(userId, ct);

            return exists
                ? Ok(new { userId, exists = true })
                : NotFound(new { error = "account_not_found", userId });
        }


        /// <summary>
        /// Пополнить баланс аккаунта
        /// </summary>
        /// <param name="req">Данные пополнения баланса</param>
        /// <param name="ct">Токен отмены запроса</param>
        /// <returns>200 баланс, 404 нет</returns>
        [HttpPost("topup")]
        public async Task<IActionResult> TopUp([FromBody] TopUpRequest req, CancellationToken ct)
        {
            try
            {
                decimal bal = await _svc.TopUpAsync(req.UserId, req.Amount, ct);
                return Ok(new BalanceResponse(req.UserId, bal));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "account_not_found", userId = req.UserId });
            }
        }

        /// <summary>
        /// Получить текущий баланс
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="ct">Токен отмены запроса</param>
        /// <returns>200 баланс, 404 нет</returns>
        [HttpGet("balance")]
        public async Task<IActionResult> Balance([FromQuery] string userId, CancellationToken ct)
        {
            try
            {
                decimal bal = await _svc.GetBalanceAsync(userId, ct);
                return Ok(new BalanceResponse(userId, bal));
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { error = "account_not_found", userId });
            }
        }
    }
}