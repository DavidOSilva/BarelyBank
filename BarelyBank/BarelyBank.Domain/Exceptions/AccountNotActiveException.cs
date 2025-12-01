using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarelyBank.Domain.Exceptions
{
    public class AccountNotActiveException : Exception
    {
        public AccountNotActiveException(string message) : base(message)
        {
        }
    }

}
