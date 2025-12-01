using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarelyBank.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController(IClientService service) : ControllerBase
    {

        [HttpGet("{id}")]
        public async Task<ActionResult<ClientViewModel>> GetClientById(Guid id)
        {
            var viewClient = await service.GetClientByIdAsync(id);
            return Ok(viewClient);
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccounts([FromQuery] Guid? clientId, [FromQuery] string? document)
        {
            var accounts = await service.SearchAccountsAsync(clientId, document);
            return Ok(accounts);
        }


    }
}
