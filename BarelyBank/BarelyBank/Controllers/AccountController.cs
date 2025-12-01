using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Application.Interfaces;
using BarelyBank.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarelyBank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IAccountService service) : ControllerBase
    {

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountViewModel>> GetAccountById(Guid id)
        {
            var viewAccount = await service.GetAccountByIdAsync(id);
            return Ok(viewAccount);
        }

        [HttpPost]
        public async Task<ActionResult<AccountViewModel>> CreateAccount([FromBody] AccountInputModel input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var accountViewModel = await service.CreateAccountAsync(input);
            return CreatedAtAction(nameof(GetAccountById), new { id = accountViewModel.Id }, accountViewModel);
        }

        [HttpPost("{id}/deposit")]
        public async Task<ActionResult<AccountViewModel>> Deposit(Guid id, int amount)
        {
            var account = await service.DepositAsync(id, amount);
            return Ok(account);
        }

        [HttpPost("{id}/withdraw")]
        public async Task<ActionResult<AccountViewModel>> Withdraw(Guid id, int amount)
        {
            var account = await service.WithdrawAsync(id, amount);
            return Ok(account);
        }

        [HttpPost("transfer")]
        public async Task<ActionResult<TransferViewModel>> Transfer([FromBody] TransferInputModel input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var transaction = await service.TransferAsync(input);
            return Ok(transaction);
        }

        [HttpGet("{id}/statement")]
        public async Task<IActionResult> GetAccountStatement(Guid id, [FromQuery] StatementInputModel input)
        {
            var statement = await service.GetAccountStatementAsync(id, input);
            return Ok(statement);
        }
    }
}