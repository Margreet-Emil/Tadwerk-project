using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Data;
using RecyclingApp.ViewModels;
using RecyclingApp.Models;
using System.Diagnostics;

namespace RecyclingApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (User.IsInRole("Collector"))
                {
                    return RedirectToAction("AvailableRequests", "PickupRequest");
                }
                else
                {
                    return RedirectToAction("Index", "PickupRequest");
                }
            }

            var wasteTypes = await _context.WasteTypes.OrderBy(w => w.Name).ToListAsync();

            var model = new HomeViewModel
            {
                WasteTypes = wasteTypes,
                TotalKg = await _context.RequestItems
                    .Where(i => i.PickupRequest.Status == "Completed")
                    .SumAsync(i => (decimal?)i.QuantityKg) ?? 0,
                CompletedRequests = await _context.PickupRequests.CountAsync(r => r.Status == "Completed"),
                Collectors = await _context.Users.CountAsync(u => u.Role == "Collector"),
                Customers = await _context.Users.CountAsync(u => u.Role == "Customer" || u.Role == "Business")
            };

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
