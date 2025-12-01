using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BarelyBank.Infra.Data
{
    public class BBContext : DbContext
    {
        public BBContext(DbContextOptions<BBContext> options)
            : base(options) { }

        // Tabelas do Banco
        public DbSet<Client> Clients { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.DocumentNumber).IsRequired().HasMaxLength(14);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.BirthDate).IsRequired();
                entity.Property(c => c.Email).IsRequired().HasMaxLength(100);
                entity.Property(c => c.PasswordHash).IsRequired();
                entity.HasIndex(c => c.DocumentNumber).IsUnique();
                entity.HasIndex(c => c.Email).IsUnique();

                // 1 Cliente tem N Contas
                entity.HasMany(c => c.Accounts)
                      .WithOne(a => a.Holder)
                      .HasForeignKey(a => a.ClientId);
            });

            // Configuração da Conta
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Number).IsRequired();
                entity.Property(a => a.Status).IsRequired();
                entity.Property(a => a.Type).IsRequired();
                entity.HasIndex(a => a.Number).IsUnique();
                entity.Property(a => a.Balance).HasPrecision(18, 2);
                entity.Property(a => a.Fee).HasPrecision(18, 4);
            });
            // Configuração da Herança de Conta
            modelBuilder.Entity<Account>()
                        .HasDiscriminator(a => a.Type)
                        .HasValue<CheckingAccount>(AccountType.Checking)
                        .HasValue<SavingsAccount>(AccountType.Savings);
            modelBuilder.Entity<CheckingAccount>();
            modelBuilder.Entity<SavingsAccount>();

            // Configuração da Transação
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Amount).HasPrecision(18, 2);
                entity.Property(t => t.Type).IsRequired().HasConversion<int>();
                entity.Property(t => t.Timestamp)
                      .IsRequired()
                      .HasColumnType("datetime");
                entity.Property(t => t.SourceAccountId).IsRequired(false);
                entity.Property(t => t.TargetAccountId).IsRequired(false);

                // 1 Transação pode ter 1 Conta de Origem e 1 Conta de Destino
                entity.HasOne(t => t.SourceAccount)
                      .WithMany()
                      .HasForeignKey(t => t.SourceAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.TargetAccount)
                      .WithMany()
                      .HasForeignKey(t => t.TargetAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

            });
        }
    }
}

