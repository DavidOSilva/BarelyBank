using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Domain.Entities;

namespace BarelyBank.Application.Interfaces
{
    public interface IClientService
    {

        Task<ClientViewModel> GetClientByIdAsync(Guid clientId);
        Task<List<AccountViewModel>> SearchAccountsAsync(Guid? clientId, string? document);
    }
}
