using BarelyBank.Application.DTOs.Inputs;
using BarelyBank.Application.DTOs.Outputs;
using BarelyBank.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BarelyBank.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<ClientViewModel>> RegisterClient([FromBody] ClientInputModel input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var clientViewModel = await authService.RegisterClientAsync(input);
            return CreatedAtAction(nameof(ClientController.GetClientById), "Client", new { id = clientViewModel.Id }, clientViewModel);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginInputModel input)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var client = await authService.ValidateCredentialsAsync(input);
            var token = authService.GenerateToken(client);
            
            return Ok(new AuthResponse
            {
                Token = token,
                Client = ClientViewModel.ToViewModel(client)
            });
        }
    }


}
