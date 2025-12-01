using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Exceptions;

namespace BarelyBank.Domain.Entities
{
    public abstract class Account
    {
        public Guid Id { get; private set; }
        public int Number { get; private set; }
        public decimal Balance { get; private set; }
        public decimal Fee { get; protected set; }
        public AccountStatus Status { get; private set; }
        public AccountType Type { get; protected set; }
        public List<Transaction> Transactions { get; private set; }
        public Guid ClientId { get; private set; }
        public Client Holder { get; private set; } = null!;

        protected Account(int number, Guid clientId, AccountStatus status, decimal fee)
        {
            Id = Guid.NewGuid();
            Number = number;
            ClientId = clientId;
            Balance = 0m;
            Status = status;
            Transactions = new List<Transaction>();
            Fee = fee;
        }

        protected void EnsureAccountIsActive()
        {
            if (Status != AccountStatus.Active)
                throw new AccountNotActiveException("A conta precisa está ativa.");
        }

        protected void EnsureSufficientFunds(decimal amount)
        {
            if (Balance < amount)
                throw new InsufficientFundsException("Saldo insuficiente.");
        }

        protected void EnsureAmountIsPositive(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("O valor da operação não pode ser negativo ou zero.");
        }

        public Transaction Deposit(decimal amount)
        {
            EnsureAccountIsActive();
            EnsureAmountIsPositive(amount);
            Balance += amount;
            var transaction = new Transaction(TransactionType.Deposit, amount, targetAccountId: Id);
            return transaction;
        }

        public virtual IEnumerable<Transaction> Withdraw(decimal amount)
        {
            EnsureAccountIsActive();
            EnsureAmountIsPositive(amount);
            EnsureSufficientFunds(amount + Fee * amount);

            Balance -= amount;
            var withdrawalTransaction = new Transaction(TransactionType.Withdraw, amount, sourceAccountId: Id);
            yield return withdrawalTransaction;

            if (Fee > 0)
            {
                var feeAmount = Fee * amount;
                Balance -= feeAmount;
                var feeTransaction = new Transaction(TransactionType.Fee, feeAmount, sourceAccountId: Id);
                yield return feeTransaction;
            }
        }
    }
}