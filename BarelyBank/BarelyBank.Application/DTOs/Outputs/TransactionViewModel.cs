using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Enums;

namespace BarelyBank.Application.DTOs.Outputs
{
    public class TransactionViewModel
    {
        public Guid Id { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? SourceAccountId { get; set; }
        public Guid? TargetAccountId { get; set; }

        public static TransactionViewModel ToViewModel(Transaction transaction)
        {
            return new TransactionViewModel
            {
                Id = transaction.Id,
                Type = transaction.Type,
                Amount = transaction.Amount,
                Timestamp = transaction.Timestamp,
                SourceAccountId = transaction.SourceAccountId,
                TargetAccountId = transaction.TargetAccountId
            };
        }
    }
}