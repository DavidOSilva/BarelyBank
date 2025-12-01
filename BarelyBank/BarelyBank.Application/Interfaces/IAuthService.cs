using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Domain.Entities;

namespace BarelyBank.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ClientViewModel> RegisterClientAsync(ClientInputModel input);
        Task<Client> ValidateCredentialsAsync(LoginInputModel input);
        string GenerateToken(Client client);
    }
}