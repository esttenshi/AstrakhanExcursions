using Microsoft.AspNetCore.Mvc;
using AstrakhanExcursions.Data;
using AstrakhanExcursions.Models;
using Microsoft.EntityFrameworkCore;


namespace AstrakhanExcursions.Controllers
{
    public class PlaceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PlaceController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Create(int userId)
        {
            ViewData["UserId"] = userId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Place place, IFormFile Image, int userId)
        {
            try
            {
                if (Image != null && Image.Length > 0)
                {
                    var imagesPath = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }

                    var fileName = Path.GetFileNameWithoutExtension(Image.FileName);
                    var extension = Path.GetExtension(Image.FileName);
                    var newFileName = $"{fileName}_{Guid.NewGuid()}{extension}";

                    var path = Path.Combine(imagesPath, newFileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }
                    place.ImageUrl = $"~/images/{newFileName}";
                }

                _context.Places.Add(place);
                await _context.SaveChangesAsync();
                ViewData["Message"] = "Место успешно добавлено!";
                ViewData["UserId"] = userId;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                ViewData["Message"] = "Возникла ошибка при сохранении данных: " + ex.Message;
                ViewData["UserId"] = userId;
                return View();
            }
        }

        public IActionResult Places(int userId)
        {
            var places = _context.Places.ToList();
            ViewData["UserId"] = userId;
            return View(places);
        }

        [HttpGet]
        public IActionResult GetPlaceDetails(int placeId)
        {
            var place = _context.Places.Find(placeId);
            if (place == null)
            {
                return NotFound();
            }

            var placeDetails = new
            {
                placeId = place.PlaceId,
                name = place.Name,
                description = place.Description,
                latitude = place.Latitude,
                longitude = place.Longitude,
                imageUrl = place.ImageUrl
            };

            return Json(placeDetails);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Place place, IFormFile Image, int userId)
        {
            try
            {
                var existingPlace = await _context.Places.FindAsync(place.PlaceId);
                if (existingPlace == null)
                {
                    return NotFound();
                }

                existingPlace.Name = place.Name;
                existingPlace.Description = place.Description;
                existingPlace.Latitude = place.Latitude;
                existingPlace.Longitude = place.Longitude;

                if (Image != null && Image.Length > 0)
                {
                    var imagesPath = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(imagesPath))
                    {
                        Directory.CreateDirectory(imagesPath);
                    }
                    if (!string.IsNullOrEmpty(existingPlace.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_environment.WebRootPath, existingPlace.ImageUrl.TrimStart('~', '/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var fileName = Path.GetFileNameWithoutExtension(Image.FileName);
                    var extension = Path.GetExtension(Image.FileName);
                    var newFileName = $"{fileName}_{Guid.NewGuid()}{extension}";

                    var path = Path.Combine(imagesPath, newFileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }
                    existingPlace.ImageUrl = $"~/images/{newFileName}";
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = "Место успешно обновлено!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                TempData["Message"] = "Возникла ошибка при обновлении места: " + ex.Message;
            }

            var places = await _context.Places.ToListAsync();
            ViewData["UserId"] = userId;
            return View("Places", places);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, int userId)
        {
            try
            {
                var place = await _context.Places.FindAsync(id);
                if (place == null)
                {
                    return NotFound();
                }

                if (!string.IsNullOrEmpty(place.ImageUrl))
                {
                    var imagePath = Path.Combine(_environment.WebRootPath, place.ImageUrl.TrimStart('~', '/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Places.Remove(place);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Место успешно удалено!";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                TempData["Message"] = "Возникла ошибка при удалении места: " + ex.Message;
            }

            var places = await _context.Places.ToListAsync();
            ViewData["UserId"] = userId;
            return View("Places", places);
        }
    }
}
