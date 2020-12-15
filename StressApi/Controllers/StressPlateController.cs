using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using StressData.Database;
using StressData.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StressApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StressPlateController : ControllerBase
    {
        private readonly ILogger<StressPlateController> _logger;
        private readonly DbContext _dbContext;
        private readonly NtsGeometryServices _geometryServices;

        public StressPlateController(ILogger<StressPlateController> logger, StressDbContext stressDbContext, NtsGeometryServices geometryServices)
        {
            _logger = logger;
            _dbContext = stressDbContext;
            _geometryServices = geometryServices;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StressPlate>> GetById(long id)
        {
            var plate = await _dbContext.Set<StressPlate>().FindAsync(id);

            return plate;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StressPlate>> GetAll()
        {
            var plates = _dbContext.Set<StressPlate>().ToList();

            return plates;
        }

        [HttpPost]
        public async Task<ActionResult<StressPlate>> Add(StressPlate plate)
        {
            if (_dbContext.Set<StressPlate>().SingleOrDefault(p => p.Name == plate.Name) != null)
            {
                return Conflict("Record already exists.");
            }
            if(!plate.Outline.Shell.IsCCW)
            {
                var shell = (LinearRing) plate.Outline.Shell.Reverse();
                plate.Outline = new Polygon(shell);
            }

            _dbContext.Add(plate);

            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = plate.Id }, plate);
        }

        [HttpPut]
        public async Task<ActionResult<StressPlate>> Update(StressPlate plate)
        {
            if (!_dbContext.Entry(plate).IsKeySet)
            {
                return BadRequest("Trying to update non-existent plate.");
            }
            if (_dbContext.Entry(plate).State == EntityState.Detached)
            {
                _dbContext.Update(plate);
            }

            await _dbContext.SaveChangesAsync();

            return plate;
        }
    }
}
