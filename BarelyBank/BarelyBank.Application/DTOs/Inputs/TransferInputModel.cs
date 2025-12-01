using System.ComponentModel.DataAnnotations;
namespace BarelyBank.Application.DTOs.Inputs
{
    public class TransferInputModel
    {
        [Required]
        public Guid SourceAccountId { get; set; }

        [Required]
        public Guid TargetAccountId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
    }

}
