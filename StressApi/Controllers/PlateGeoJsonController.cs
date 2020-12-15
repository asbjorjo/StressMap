using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using StressApi.Helpers;
using StressData.Database;
using StressData.Database.Constants;
using StressData.Model;

namespace StressApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class PlateGeoJsonController : ControllerBase
    {
        private readonly DbContext _context;
        private readonly ILogger<PlateGeoJsonController> _logger;
        private readonly NtsGeometryServices _geometryServices;
        private readonly IOptions<JsonOptions> _jsonOptions;

        public PlateGeoJsonController(ILogger<PlateGeoJsonController> logger, NtsGeometryServices geometryServices, IOptions<JsonOptions> jsonOptions, StressDbContext context)
        {
            _context = context;
            _logger = logger;
            _geometryServices = geometryServices;
            _jsonOptions = jsonOptions;
        }

        // GET: api/StressGeoJson
        [EnableCors("_allowSpecificOrigins")]
        [HttpGet]
        public async Task<ActionResult<FeatureCollection>> GetStressPlates(
            [FromQuery] Feature boundary)
        {
            var geometryFactory = _geometryServices.CreateGeometryFactory(GeometryConstants.SRID);
            var result = new FeatureCollection();
            
            var plates = _context.Set<StressPlate>().AsQueryable();
                       
            if (boundary.Geometry is not null)
            {
                plates = plates.Where(p => p.Outline.CoveredBy(boundary.Geometry));
            }

            plates = plates.OrderBy(p => p.Name).Where(p => p.Name != "AN");
            
            foreach (var plate in plates)
            {
                result.Add(FeatureFromPlate(plate));
            }

            return await Task.FromResult(result);
        }

        // GET: api/StressGeoJson/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Feature>> GetStressPlate(long id)
        {
            var stressPlate = await _context.Set<StressPlate>().FindAsync(id);

            if (stressPlate == null)
            {
                return NotFound();
            }

            return FeatureFromPlate(stressPlate);
        }

        // PUT: api/StressGeoJson/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStressPlate(long id, Feature stressFeature)
        {
            StressPlate stressPlate;

            try
            {
                stressPlate = PlateFromFeature(stressFeature);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }

            if (stressPlate.Id != id)
            {
                return BadRequest();
            }


            _context.Entry(stressPlate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StressPlateExists(id))
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
        public async Task<ActionResult<Feature>> PostStressPlate(Feature plateFeature)
        {
            var plate = PlateFromFeature(plateFeature);

            if (_context.Set<StressPlate>().Any(p => p.Name == plate.Name))
            {
                return Conflict("trying to add existing plate");
            }

            await _context.Set<StressPlate>().AddAsync(plate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStressPlate", new { id = plateFeature.GetOptionalId(GeoJsonConverterFactory.DefaultIdPropertyName) }, plateFeature);
        }

        // DELETE: api/StressGeoJson/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStressPlate(long id)
        {
            var stressPlate = await _context.Set<StressPlate>().FindAsync(id);
            if (stressPlate == null)
            {
                return NotFound();
            }

            _context.Set<StressPlate>().Remove(stressPlate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StressPlateExists(long id)
        {
            return _context.Set<StressPlate>().Any(e => e.Id == id);
        }

        private Feature FeatureFromPlate(StressPlate plate)
        {
            var feature = new Feature
            {
                Geometry = plate.Outline,
                Attributes = new AttributesTable
                    {
                        { GeoJsonConverterFactory.DefaultIdPropertyName, plate.Id },
                        { "name", plate.Name }
                     }
            };

            return feature;
        }

        private StressPlate PlateFromFeature(Feature feature)
        {
            feature.Attributes.TryDeserializeJsonObject(_jsonOptions.Value.JsonSerializerOptions, out StressPlate plate);

            var featureId = feature.GetOptionalId(GeoJsonConverterFactory.DefaultIdPropertyName);

            if (featureId is not null)
            {
                if (featureId is decimal)
                {
                    plate.Id = Convert.ToInt64(featureId);
                }
                else if (featureId is string stringId && long.TryParse(stringId, out long id))
                {
                    plate.Id = id;
                } 
                else
                {
                    throw new ArgumentException("id must be a numeric value");
                }
            }
            
            if (feature.Geometry is not Polygon outline)
            {
                throw new ArgumentException("missing 'geometry' or it is not a Polygon");
            }
            plate.Outline = outline;

            return plate;
        }
    }
}
