using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using StressData.Database;
using StressData.Database.Constants;
using StressData.Model;

namespace StressApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StressGeoJsonController : ControllerBase
    {
        private readonly DbContext _context;
        private readonly ILogger<StressGeoJsonController> _logger;
        private readonly NtsGeometryServices _geometryServices;
        private readonly IOptions<JsonOptions> _jsonOptions;

        public StressGeoJsonController(ILogger<StressGeoJsonController> logger, NtsGeometryServices geometryServices, IOptions<JsonOptions> jsonOptions, StressDbContext context)
        {
            _context = context;
            _logger = logger;
            _geometryServices = geometryServices;
            _jsonOptions = jsonOptions;
        }

        // GET: api/StressGeoJson
        [HttpGet]
        public async Task<ActionResult<FeatureCollection>> GetStressRecords()
        {
            var records = _context.Set<StressRecord>().OrderBy(s => s.WsmId);

            var geometryFactory = _geometryServices.CreateGeometryFactory(GeometryConstants.SRID);
            var result = new FeatureCollection();

            foreach (var record in records)
            {
                result.Add(FeatureFromRecord(record));
            }

            return await Task.FromResult(result);
        }

        // GET: api/StressGeoJson/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IFeature>> GetStressRecord(long id)
        {
            var stressRecord = await _context.Set<StressRecord>().FindAsync(id);

            if (stressRecord == null)
            {
                return NotFound();
            }

            return FeatureFromRecord(stressRecord);
        }

        // PUT: api/StressGeoJson/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStressRecord(long id, IFeature stressFeature)
        {
            StressRecord stressRecord;

            try
            {
                stressRecord = RecordFromFeature(stressFeature);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }

            if (stressRecord.Id != id)
            {
                return BadRequest();
            }


            _context.Entry(stressRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StressRecordExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/StressGeoJson
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Feature>> PostStressRecord(IFeature stressFeature)
        {
            var record = RecordFromFeature(stressFeature);

            _context.Set<StressRecord>().Add(record);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStressRecord", new { id = stressFeature.GetOptionalId(GeoJsonConverterFactory.DefaultIdPropertyName) }, stressFeature);
        }

        // DELETE: api/StressGeoJson/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStressRecord(long id)
        {
            var stressRecord = await _context.Set<StressRecord>().FindAsync(id);
            if (stressRecord == null)
            {
                return NotFound();
            }

            _context.Set<StressRecord>().Remove(stressRecord);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StressRecordExists(long id)
        {
            return _context.Set<StressRecord>().Any(e => e.Id == id);
        }

        public static Feature FeatureFromRecord(StressRecord record)
        {
            var feature = new Feature
            {
                Geometry = record.Location,
                Attributes = new AttributesTable
                    {
                        { GeoJsonConverterFactory.DefaultIdPropertyName, record.Id },
                        { "wsmid", record.WsmId},
                        { "azimuth",  record.Azimuth},
                        { "ISO", record.ISO },
                        { "regime", record.Regime },
                        { "type", record.Type }
                     }
            };

            return feature;
        }

        private StressRecord RecordFromFeature(IFeature feature)
        {
            feature.Attributes.TryDeserializeJsonObject(_jsonOptions.Value.JsonSerializerOptions, out StressRecord record);

            var featureId = feature.GetOptionalId(GeoJsonConverterFactory.DefaultIdPropertyName);

            if (featureId is not null)
            {
                if (featureId is decimal)
                {
                    record.Id = Convert.ToInt64(featureId);
                }
                else if (featureId is string stringId && long.TryParse(stringId, out long id))
                {
                    record.Id = id;
                } 
                else
                {
                    throw new ArgumentException("id must be a numeric value");
                }
            }
            
            if (feature.Geometry is not Point location)
            {
                throw new ArgumentException("missing 'geometry' or it is not a Point");
            }
            record.Location = location;

            return record;
        }
    }
}
