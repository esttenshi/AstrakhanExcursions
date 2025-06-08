using AstrakhanExcursions.Data;
using AstrakhanExcursions.Models;

namespace AstrakhanExcursions.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [Route("api/[controller]")]
    [ApiController]
    public class TourTagController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TourTagController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TourTag
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TourTag>>> GetTourTags()
        {
            return await _context.TourTags.Include(tt => tt.Tour).Include(tt => tt.Tag).ToListAsync();
        }

        // GET: api/TourTag/5
        [HttpGet("{tourId}/{tagId}")]
        public async Task<ActionResult<TourTag>> GetTourTag(int tourId, int tagId)
        {
            var tourTag = await _context.TourTags
                .Include(tt => tt.Tour)
                .Include(tt => tt.Tag)
                .FirstOrDefaultAsync(tt => tt.TourId == tourId && tt.TagId == tagId);

            if (tourTag == null)
            {
                return NotFound();
            }

            return tourTag;
        }

        // POST: api/TourTag
        [HttpPost]
        public async Task<ActionResult<TourTag>> PostTourTag(TourTag tourTag)
        {
            _context.TourTags.Add(tourTag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTourTag), new { tourId = tourTag.TourId, tagId = tourTag.TagId }, tourTag);
        }

        // DELETE: api/TourTag/5/1
        [HttpDelete("{tourId}/{tagId}")]
        public async Task<IActionResult> DeleteTourTag(int tourId, int tagId)
        {
            var tourTag = await _context.TourTags.FindAsync(tourId, tagId);
            if (tourTag == null)
            {
                return NotFound();
            }

            _context.TourTags.Remove(tourTag);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TourTagExists(int tourId, int tagId)
        {
            return _context.TourTags.Any(e => e.TourId == tourId && e.TagId == tagId);
        }
    }

}
