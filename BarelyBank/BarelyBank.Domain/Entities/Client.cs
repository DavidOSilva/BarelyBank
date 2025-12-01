using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarelyBank.Domain.Entities
{
    public class Client
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime BirthDate { get; private set; }
        public string DocumentNumber { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public List<Account> Accounts { get; private set; }


        public Client(string name, string documentNumber, DateTime birthDate, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            Name = name;
            DocumentNumber = documentNumber;
            BirthDate = birthDate;
            Email = email;
            PasswordHash = passwordHash;
            Accounts = new List<Account>();
        }

    }

}
