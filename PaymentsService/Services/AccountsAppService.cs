using Microsoft.EntityFrameworkCore;
using PaymentsService.Data;
using PaymentsService.Data.Entities;

namespace PaymentsService.Services
{
    public sealed class AccountsAppService
    {
        /// <summary>
        /// База данных
        /// </summary>
        private readonly PaymentsDbContext _db;

        /// <summary>
        /// Конструктор AccountsAppService
        /// </summary>
        /// <param name="db">База данных</param>
        public AccountsAppService(PaymentsDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Проверка существования счёта
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Существует ли счёт</returns>
        public Task<bool> ExistsAsync(string userId, CancellationToken ct)
        {
            return _db.Accounts.AnyAsync(a => a.UserId == userId, ct);
        }

        /// <summary>
        /// Списание средств
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="amount">Сумма списания</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Итоговый баланс счёта</returns>
        public async Task<decimal> DebitAsync(string userId, decimal amount, CancellationToken ct)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
            Account? acc = await _db.Accounts.SingleOrDefaultAsync(a => a.UserId == userId, ct);
            if (acc is null)
            {
                throw new KeyNotFoundException("account_not_found");
            }
            if (acc.Balance < amount)
            {
                throw new InvalidOperationException("insufficient_funds");
            }
            acc.Balance -= amount;
            await _db.SaveChangesAsync(ct);
            return acc.Balance;
        }
        
        /// <summary>
        /// Создание счёта
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="ct">Токен отмены</param>
        public async Task CreateAccountAsync(string userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("userId is required", nameof(userId));
            }
            bool exists = await _db.Accounts.AnyAsync(a => a.UserId == userId, ct);
            if (exists)
            {
                throw new InvalidOperationException("Аккаунт уже существует");
            }
            _db.Accounts.Add(new Account { UserId = userId, Balance = 0m });
            await _db.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Пополнение баланса
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="amount">Сумма пополнения</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Итоговый баланс счёта</returns>
        public async Task<decimal> TopUpAsync(string userId, decimal amount, CancellationToken ct)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
            Account? acc = await _db.Accounts.SingleOrDefaultAsync(a => a.UserId == userId, ct);
            if (acc is null)
            {
                throw new KeyNotFoundException("account_not_found");
            }
            acc.Balance += amount;
            await _db.SaveChangesAsync(ct);
            return acc.Balance;
        }

        /// <summary>
        /// Получение баланса
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns>Баланс счёта</returns>
        public async Task<decimal> GetBalanceAsync(string userId, CancellationToken ct)
        {
            Account? acc = await _db.Accounts.SingleOrDefaultAsync(a => a.UserId == userId, ct);
            if (acc is null)
            {
                throw new KeyNotFoundException("account_not_found");
            }
            return acc.Balance;
        }
    }
}