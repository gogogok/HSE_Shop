using PaymentsService.Data;
using PaymentsService.Messaging;
using PaymentsService.Options;
using SharedContracts.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using PaymentsService.Data.Entities;

namespace PaymentsService.Services
{
    /// <summary>
    /// Сервис для работы с оплатой
    /// </summary>
    public sealed class PaymentProcessingService
    {
        /// <summary>
        /// База данных
        /// </summary>
        private readonly PaymentsDbContext _db;
        
        /// <summary>
        /// Сериализатор
        /// </summary>
        private readonly IJsonMessageSerializer _json;
        
        /// <summary>
        /// Кафка
        /// </summary>
        private readonly KafkaOptions _kafka;

        /// <summary>
        /// Конструктор PaymentProcessingService
        /// </summary>
        /// <param name="db">База данных</param>
        /// <param name="json">Сериализатор</param>
        /// <param name="kafka">Кафка</param>
        public PaymentProcessingService(PaymentsDbContext db, IJsonMessageSerializer json, IOptions<KafkaOptions> kafka)
        {
            _db = db;
            _json = json;
            _kafka = kafka.Value;
        }
        
        /// <summary>
        /// Обработка запроса оплаты
        /// </summary>
        /// <param name="msg">Сообщение об оплате</param>
        /// <param name="ct">Токен отмены</param>
        public async Task HandleOrderPaymentRequestedAsync(OrderPaymentRequested msg, CancellationToken ct)
        {
            string orderId = msg.OrderId;
            string userId = msg.UserId;
            decimal amount = msg.Amount;
            await using IDbContextTransaction tx = await _db.Database.BeginTransactionAsync(ct);

            bool already = await _db.InboxMessages.AnyAsync(x => x.MessageType == nameof(OrderPaymentRequested) && x.MessageKey == orderId, ct);
            if (already)
            {
                await tx.CommitAsync(ct);
                return;
            }
            _db.InboxMessages.Add(new InboxMessage
            {
                MessageType = nameof(OrderPaymentRequested),
                MessageKey = orderId
            });
            bool success;
            string? reason = null;

            Account? acc = await _db.Accounts.SingleOrDefaultAsync(a => a.UserId == userId, ct);
            if (acc is null)
            {
                success = false;
                reason = "account_not_found";
            }
            else if (acc.Balance >= amount)
            {
                acc.Balance -= amount;
                success = true;
            }
            else
            {
                success = false;
                reason = "insufficient_funds";
            }

            PaymentResult result = new PaymentResult (
                orderId,
                userId,
                amount,
                success,
                reason
            );

            _db.OutboxMessages.Add(new OutboxMessage
            {
                MessageType = nameof(PaymentResult),
                Topic = _kafka.PaymentResultTopic,
                Key = orderId,
                PayloadJson = _json.Serialize(result)
            });

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
    }
}
