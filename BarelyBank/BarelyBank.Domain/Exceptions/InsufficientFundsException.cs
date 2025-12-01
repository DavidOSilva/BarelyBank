using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarelyBank.Domain.Exceptions
{
    public class InsufficientFundsException : Exception
    {
        public InsufficientFundsException(string message) : base(message)
        {
        }
    }

}
