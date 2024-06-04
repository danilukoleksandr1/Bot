
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlantsController : ControllerBase
    {
        private readonly ILogger<PlantsController> _logger;
        private readonly Database _database = new Database();

        public PlantsController(ILogger<PlantsController> logger)
        {
            _logger = logger;
        }

        // GET: /plants/{chatId}
        [HttpGet("{chatId}")]
        public async Task<ActionResult<List<Plant>>> GetPlants(long chatId)
        {
            var plants = await _database.SelectPlants(chatId);
            if (plants == null || plants.Count == 0)
            {
                return NotFound();
            }
            return Ok(plants);
        }

        // GET: /plants/{chatId}/{plantName}
        [HttpGet("{chatId}/{plantName}")]
        public async Task<ActionResult<Plant>> GetPlant(long chatId, string plantName)
        {
            var plants = await _database.SelectPlants(chatId);
            var plant = plants.Find(p => p.PlantName == plantName);
            if (plant == null)
            {
                return NotFound();
            }
            return Ok(plant);
        }

        // PUT: /plants
        [HttpPut]
        public async Task<ActionResult> UpdatePlant([FromBody] Plant plant)
        {
            try
            {
                await _database.RemovePlantAsync(plant.ChatId, plant.PlantName);
                await _database.AddPlantAsync(plant.ChatId, plant.PlantName, plant.DateAdded, plant.Recommendation);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка оновлення рослини");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        // POST: /plants
        [HttpPost]
        public async Task<ActionResult> AddPlant([FromBody] Plant plant)
        {
            try
            {
                await _database.AddPlantAsync(plant.ChatId, plant.PlantName, plant.DateAdded, plant.Recommendation);
                return CreatedAtAction(nameof(GetPlant), new { chatId = plant.ChatId, plantName = plant.PlantName }, plant);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка додавання рослини");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }

        // DELETE: /plants/{chatId}/{plantName}
        [HttpDelete("{chatId}/{plantName}")]
        public async Task<ActionResult> DeletePlant(long chatId, string plantName)
        {
            try
            {
                await _database.RemovePlantAsync(chatId, plantName);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка видалення рослини");
                return StatusCode(500, "Внутрішня помилка сервера");
            }
        }
    }
}