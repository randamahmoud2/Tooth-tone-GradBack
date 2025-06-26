using DentalManagementAPI.Data;
using DentalManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DentalManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DrugsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DrugsController> _logger;

        public DrugsController(AppDbContext context, ILogger<DrugsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Drug>>> GetDrugs()
        {
            try
            {
                var drugs = await _context.Drugs.ToListAsync();
                return Ok(drugs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching drugs");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}