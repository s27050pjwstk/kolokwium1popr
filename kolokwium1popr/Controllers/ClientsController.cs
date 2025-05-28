using kolokwium1popr.Models.DTO;
using kolokwium1popr.Services;
using Microsoft.AspNetCore.Mvc;

namespace kolokwium1popr.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public ClientsController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(int id)
        {
            try
            {
                var client = await _databaseService.GetClientById(id);
                return Ok(client);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddClientAndRental([FromBody] NewClientWithRentalRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _databaseService.AddClientWithRental(request);
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}