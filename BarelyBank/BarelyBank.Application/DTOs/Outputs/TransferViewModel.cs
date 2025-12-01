using BarelyBank.Domain.Entities;

namespace BarelyBank.Application.DTOs.Outputs
{
    public class TransferViewModel
    {
        public decimal Amount { get; set; }
        public decimal FeeAmont { get; set; }
        public decimal Total => Amount + FeeAmont;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public Guid? SourceAccountId { get; set; }
        public string SourceAccountHolder { get; set; } = string.Empty;
        public int SourceAccountNumber { get; set; }
        public Guid? TargetAccountId { get; set; }
        public string TargetAccountHolder { get; set; } = string.Empty;
        public int TargetAccountNumber { get; set; }

        public static TransferViewModel ToViewModel ( Account SourceAccount, Account TargetAccount, decimal amount )
        {
            return new TransferViewModel
            {
                Amount = amount,
                FeeAmont = SourceAccount.Fee * amount,
                SourceAccountId = SourceAccount.Id,
                SourceAccountNumber = SourceAccount.Number,
                SourceAccountHolder = SourceAccount.Holder.Name,
                TargetAccountId = TargetAccount.Id,
                TargetAccountNumber = TargetAccount.Number,
                TargetAccountHolder = TargetAccount.Holder.Name,
            };
        }
    }

}
