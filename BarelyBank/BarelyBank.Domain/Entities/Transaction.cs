using BarelyBank.Domain.Enums;
using BarelyBank.Domain.Exceptions;

namespace BarelyBank.Domain.Entities
{
    public record Transaction
    (
        Guid Id,
        TransactionType Type,
        decimal Amount,
        DateTime Timestamp,
        Guid? SourceAccountId,
        Guid? TargetAccountId
    )
    {
        public Account? SourceAccount { get; init; }
        public Account? TargetAccount { get; init; }

        public Transaction(
            TransactionType type,
            decimal amount,
            Guid? sourceAccountId = null,
            Guid? targetAccountId = null
        ) : this(
            Guid.NewGuid(),
            type,
            amount,
            DateTime.UtcNow,
            sourceAccountId,
            targetAccountId
        )
        {
            switch (type)
            {
                case TransactionType.Deposit:
                    if (targetAccountId == null) throw new InvalidTransactionException("Depósitos exigem uma conta de destino.");
                    break;

                case TransactionType.Withdraw:
                    if (sourceAccountId == null) throw new InvalidTransactionException("Saques exigem uma conta de origem.");
                    break;

                case TransactionType.Fee:
                    if (sourceAccountId == null) throw new InvalidTransactionException("Taxas exigem uma conta de origem.");
                    break;

                default:
                    throw new InvalidTransactionException("Tipo de transação desconhecido.");
            }
        }
    }
}