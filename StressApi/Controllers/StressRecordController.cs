using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using StressApi.Database;
using StressData.Model;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StressApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StressRecordController : ControllerBase
    {
        private readonly ILogger<StressRecordController> _logger;
        private readonly DbContext _dbContext;
        private readonly NtsGeometryServices _geometryServices;

        public StressRecordController(ILogger<StressRecordController> logger, StressDbContext stressDbContext, NtsGeometryServices geometryServices)
        {
            _logger = logger;
            _dbContext = stressDbContext;
            _geometryServices = geometryServices;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StressRecord>> GetById(long id)
        {
            var record = await _dbContext.Set<StressRecord>().FindAsync(id);

            return record;
        }

        [HttpGet]
        public ActionResult<IEnumerable<StressRecord>> GetAll()
        {
            var records = _dbContext.Set<StressRecord>().ToList();

            return records;
        }

        [HttpPost]
        public async Task<ActionResult<StressRecord>> Add(StressRecord record)
        {
            if (_dbContext.Set<StressRecord>().SingleOrDefault(r => r.WsmId == record.WsmId) != null)
            {
                return BadRequest("Record already exists.");
            }

            await _dbContext.AddAsync(record);

            await _dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }

        [HttpPut]
        public async Task<ActionResult<StressRecord>> Update(StressRecord record)
        {
            if (!_dbContext.Entry(record).IsKeySet)
            {
                return BadRequest("Trying to update non-existent stress record.");
            }
            if (_dbContext.Entry(record).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
            {
                _dbContext.Update(record);
            }

            await _dbContext.SaveChangesAsync();

            return record;
        }
    }
}
