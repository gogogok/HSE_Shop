using Microsoft.EntityFrameworkCore;
using PaymentsService.Data.Entities;

namespace PaymentsService.Data
{
    /// <summary>
    /// Контекст платежной БД
    /// </summary>
    public sealed class PaymentsDbContext : DbContext
    {
        /// <summary>
        /// Конструктор PaymentsDbContext
        /// </summary>
        /// <param name="options">Параметры подключения БД</param>
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

        /// <summary>
        /// Таблица аккаунтов
        /// </summary>
        public DbSet<Account> Accounts => Set<Account>();
        
        /// <summary>
        /// Таблица inbox сообщений
        /// </summary>
        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
        
        /// <summary>
        /// Таблица outbox сообщений
        /// </summary>
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        /// <summary>
        /// Конфигурация схемы БД
        /// </summary>
        /// <param name="modelBuilder">Строитель модели</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<InboxMessage>()
                .HasIndex(x => new { x.MessageType, x.MessageKey })
                .IsUnique();

            modelBuilder.Entity<OutboxMessage>()
                .HasIndex(x => x.PublishedAtUtc);

            base.OnModelCreating(modelBuilder);
        }
    }
}