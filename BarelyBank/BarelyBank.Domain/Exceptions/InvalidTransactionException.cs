using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarelyBank.Domain.Exceptions
{
    public class InvalidTransactionException : Exception
    {

        public InvalidTransactionException(string message) : base(message)
        {
        }
    }

}
