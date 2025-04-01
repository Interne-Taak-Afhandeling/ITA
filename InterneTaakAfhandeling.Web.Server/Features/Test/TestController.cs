// InterneTaakAfhandeling.Web.Server/Features/Test/TestController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InterneTaakAfhandeling.Web.Server.Data;

namespace InterneTaakAfhandeling.Web.Server.Features.Test
{
    [Route("api/test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(ApplicationDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            try
            {
                var items = await _context.TestItems.ToListAsync();
                _logger.LogInformation("Retrieved {Count} test items", items.Count);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test items");
                return StatusCode(500, "An error occurred while retrieving data");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            try
            {
                var item = await _context.TestItems.FindAsync(id);

                if (item == null)
                {
                    _logger.LogWarning("Test item with ID {Id} not found", id);
                    return NotFound();
                }

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test item {Id}", id);
                return StatusCode(500, "An error occurred while retrieving data");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(TestItem item)
        {
            try
            {
                _context.TestItems.Add(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new test item with ID {Id}", item.Id);
                return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test item");
                return StatusCode(500, "An error occurred while creating item");
            }
        }
    }
}