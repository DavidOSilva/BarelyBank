using Azure.Core;
using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Application.Interfaces;
using BarelyBank.Domain.Entities;
using BarelyBank.Domain.Repositories;
using BarelyBank.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace BarelyBank.Application.Services
{
    public class ClientService(
        IAccountRepository accountRepository,
        IClientRepository clientRepository) : IClientService
    {
        public async Task<ClientViewModel> GetClientByIdAsync(Guid clientId)
        {
            var client =  await clientRepository.GetAsync(clientId);
            if (client == null) throw new NotFoundException("Cliente não foi encontrado.");
            return ClientViewModel.ToViewModel(client);
        }

        public async Task<List<AccountViewModel>> SearchAccountsAsync(Guid? clientId, string? document)
        {
            if (!clientId.HasValue && string.IsNullOrWhiteSpace(document)) throw new ValidationException("É necessário informar o ID do cliente ou o CPF para realizar a busca.");

            Guid targetClientId;
            if (clientId.HasValue)
            {
                var client = await clientRepository.GetAsync(clientId.Value);
                if (client == null) throw new NotFoundException("Cliente não encontrado pelo ID informado.");
                targetClientId = client.Id;
            }
            else
            {
                var client = await clientRepository.GetClientByDocumentNumberAsync(document!);
                if (client == null) throw new NotFoundException("Cliente não encontrado pelo CPF informado.");
                targetClientId = client.Id;
            }
            var accounts = await accountRepository.GetAccountsByClientIdAsync(targetClientId);
            return accounts.Select(AccountViewModel.ToViewModel).ToList();

        }

    }

}
