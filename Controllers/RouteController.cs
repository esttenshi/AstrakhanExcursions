using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AstrakhanExcursions.Data;
using AstrakhanExcursions.Models;

namespace AstrakhanExcursions.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RouteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Route
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Route>>> GetRoutes()
        {
            return await _context.Routes.Include(r => r.Tour).Include(r => r.Place).ToListAsync();
        }

        // GET: api/Route/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Route>> GetRoute(int id)
        {
            var route = await _context.Routes.Include(r => r.Tour).Include(r => r.Place).FirstOrDefaultAsync(r => r.RouteId == id);

            if (route == null)
            {
                return NotFound();
            }

            return route;
        }

        // POST: api/Route
        [HttpPost]
        public async Task<ActionResult<Models.Route>> PostRoute(Models.Route route)
        {
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoute), new { id = route.RouteId }, route);
        }

        // PUT: api/Route/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoute(int id, Models.Route route)
        {
            if (id != route.RouteId)
            {
                return BadRequest();
            }

            _context.Entry(route).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RouteExists(id))
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

        // DELETE: api/Route/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return NotFound();
            }

            _context.Routes.Remove(route);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RouteExists(int id)
        {
            return _context.Routes.Any(e => e.RouteId == id);
        }

        [HttpGet("places/{tourId}")]
        public async Task<ActionResult<IEnumerable<Place>>> GetPlacesByTourId(int tourId)
        {
            var places = await _context.Routes
                .Where(r => r.TourId == tourId)
                .Include(r => r.Place)
                .Select(r => new {
                    r.Place.Name,
                    r.Place.Latitude,
                    r.Place.Longitude,
                    r.Place.Description
                })
                .ToListAsync();

            return Ok(places);
        }
    }
}
