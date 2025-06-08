using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AstrakhanExcursions.Data;
using AstrakhanExcursions.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AstrakhanExcursions.Controllers
{
    public class TourController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TourController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(int userId)
        {
            ViewData["UserId"] = userId;
            var places = _context.Places.ToList();
            ViewData["Places"] = places;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int userId, string title, string description, int[] selectedPlaceIds, string tagsInput)
        {
            if (selectedPlaceIds == null || selectedPlaceIds.Length == 0)
            {
                ViewData["Message"] = "Пожалуйста, выберите хотя бы одно место для посещения";
                ViewData["Places"] = _context.Places.ToList();
                ViewData["Description"] = description;
                ViewData["Title"] = title;
                ViewData["User Id"] = userId;
                ViewData["Tags"] = tagsInput;
                return View();
            }

            var tour = new Tour { Title = title, Description = description, UserId = userId };
            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();

            int orderInRoute = 1;
            foreach (var placeId in selectedPlaceIds)
            {
                var route = new Models.Route { TourId = tour.TourId, PlaceId = placeId, OrderInRoute = orderInRoute++ };
                _context.Routes.Add(route);
            }

            // Обработка тегов, если они указаны
            if (!string.IsNullOrWhiteSpace(tagsInput))
            {
                var tagNames = tagsInput.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    var tourTag = new TourTag { TourId = tour.TourId, TagId = tag.TagId };
                    _context.TourTags.Add(tourTag);
                }
            }

            await _context.SaveChangesAsync();

            ViewData["Message"] = "Экскурсия успешно добавлена!";
            ViewData["Places"] = _context.Places.ToList();
            ViewData["UserId"] = userId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Marshroutes(int userId)
        {
            var tours = await _context.Tours
                .Select(t => new TourViewModel
                {
                    Tour = t,
                    Tags = _context.TourTags
                        .Where(tt => tt.TourId == t.TourId)
                        .Select(tt => tt.Tag.Name)
                        .ToList(),
                    Places = _context.Routes
                        .Where(r => r.TourId == t.TourId)
                        .Select(r => r.Place.Name)
                        .ToList()
                })
                .ToListAsync();

            ViewData["Tours"] = tours;
            ViewData["UserId"] = userId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MyExcursions(int userId)
        {
            var tours = await _context.Tours
                .Where(t => t.UserId == userId)
                .Select(t => new TourViewModel
                {
                    Tour = t,
                    Tags = _context.TourTags
                        .Where(tt => tt.TourId == t.TourId)
                        .Select(tt => tt.Tag.Name)
                        .ToList(),
                    Places = _context.Routes
                        .Where(r => r.TourId == t.TourId)
                        .Select(r => r.Place.Name)
                        .ToList()
                })
                .ToListAsync();

            ViewData["Tours"] = tours;
            ViewData["UserId"] = userId;
            ViewData["Places"] = _context.Places.ToList();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTourDetails(int tourId)
        {
            var tour = await _context.Tours.FindAsync(tourId);

            var tags = await _context.TourTags
                .Where(tt => tt.TourId == tourId)
                .Select(tt => tt.Tag.Name)
                .ToListAsync();

            var selectedPlaceIds = await _context.Routes
                .Where(r => r.TourId == tourId)
                .Select(r => r.PlaceId)
                .ToListAsync();

            var tourDetails = new
            {
                tourId = tour.TourId,
                title = tour.Title,
                description = tour.Description,
                tags = tags,
                selectedPlaceIds = selectedPlaceIds 
            };

            return Json(tourDetails);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int tourId, string title, string description, int[] selectedPlaceIds, string tagsInput, int userId)
        {
            if (selectedPlaceIds == null || selectedPlaceIds.Length == 0)
            {
                ViewData["Message"] = "Пожалуйста, выберите хотя бы одно место для экскурсии.";
                ViewData["User Id"] = userId;

                var tour = await _context.Tours.FindAsync(tourId);
                if (tour == null)
                {
                    return NotFound();
                }

                var tags = await _context.TourTags
                    .Where(tt => tt.TourId == tourId)
                    .Select(tt => tt.Tag.Name)
                    .ToListAsync();

                var selectedPlaceIdsList = await _context.Routes
                    .Where(r => r.TourId == tourId)
                    .Select(r => r.PlaceId)
                    .ToListAsync();

                ViewData["Tours"] = new List<TourViewModel>
        {
            new TourViewModel
            {
                Tour = tour,
                Tags = tags,
                Places = await _context.Routes
                    .Where(r => r.TourId == tourId)
                    .Select(r => r.Place.Name)
                    .ToListAsync()
            }
        };

                ViewData["Places"] = _context.Places.ToList();
                return View("MyExcursions");
            }

            var tourToUpdate = await _context.Tours.FindAsync(tourId);
            if (tourToUpdate == null)
            {
                return NotFound();
            }

            // Обновление свойств экскурсии
            tourToUpdate.Title = title;
            tourToUpdate.Description = description;
            _context.Update(tourToUpdate);
            await _context.SaveChangesAsync();

            // Обновление маршрутов
            var existingRoutes = _context.Routes.Where(r => r.TourId == tourId).ToList();
            _context.Routes.RemoveRange(existingRoutes);
            await _context.SaveChangesAsync();

            int orderInRoute = 1;
            foreach (var placeId in selectedPlaceIds)
            {
                var route = new Models.Route { TourId = tourToUpdate.TourId, PlaceId = placeId, OrderInRoute = orderInRoute++ };
                _context.Routes.Add(route);
            }

            // Удаление существующих тегов
            var existingTourTags = _context.TourTags.Where(tt => tt.TourId == tourId).ToList();
            _context.TourTags.RemoveRange(existingTourTags);
            await _context.SaveChangesAsync();

            // Обработка тегов, если они указаны
            if (!string.IsNullOrWhiteSpace(tagsInput))
            {
                var tagNames = tagsInput.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags
                        .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                        await _context.SaveChangesAsync();
                    }

                    var tourTag = new TourTag { TourId = tourToUpdate.TourId, TagId = tag.TagId };
                    _context.TourTags.Add(tourTag);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Экскурсия успешно изменена!";
            return RedirectToAction("MyExcursions", new { userId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int userId)
        {
            var tour = await _context.Tours.FindAsync(id);

            var routes = _context.Routes.Where(r => r.TourId == id);
            _context.Routes.RemoveRange(routes);

            var tourTags = _context.TourTags.Where(tt => tt.TourId == id);
            _context.TourTags.RemoveRange(tourTags);

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Экскурсия успешно удалена!";
            return RedirectToAction("MyExcursions", new { userId = userId });
        }
    }
}
