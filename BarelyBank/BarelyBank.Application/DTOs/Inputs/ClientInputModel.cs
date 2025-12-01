using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarelyBank.Application.DTOs.Inputs
{
    public class ClientInputModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [StringLength(14)]
        [RegularExpression(@"\d{3}\.\d{3}\.\d{3}-\d{2}")]
        public string DocumentNumber { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string ConfirmPassword { get; set; } = null!;
    }

}
