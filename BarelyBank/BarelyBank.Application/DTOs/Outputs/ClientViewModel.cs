using BarelyBank.Domain.Entities;

namespace BarelyBank.Application.DTOs.Outputs
{
    public class ClientViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public static ClientViewModel ToViewModel(Client client)
        {
            return new ClientViewModel
            {
                Id = client.Id,
                Name = client.Name,
                DocumentNumber = client.DocumentNumber,
                Email = client.Email
            };
        }

    }


}
