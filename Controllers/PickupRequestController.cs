using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Data;
using RecyclingApp.Models;
using RecyclingApp.ViewModels;
using Microsoft.Extensions.Configuration;

namespace RecyclingApp.Controllers
{
    [Authorize]
    public class PickupRequestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        private decimal MisreportFee => _config.GetValue<decimal>("Policy:MisreportFeeEgp", 50m);

        public PickupRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _config = config;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (await _userManager.IsInRoleAsync(user!, "Collector"))
            {
                return RedirectToAction("AvailableRequests");
            }

            var requests = await _context.PickupRequests
                .Include(r => r.RequestItems)
                .ThenInclude(i => i.WasteType)
                .Where(r => r.UserId == user!.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var wasteTypes = await _context.WasteTypes.ToListAsync();
            ViewBag.Fee = MisreportFee;
            var model = new CreatePickupRequestViewModel
            {
                ScheduledAt = DateTime.Now.AddDays(1),
                Items = wasteTypes.Select(wt => new CreateRequestItemViewModel
                {
                    WasteTypeId = wt.Id,
                    WasteTypeName = wt.Name,
                    PricePerKg = wt.PricePerKg,
                    QuantityKg = 0,
                    IsSelected = false
                }).ToList(),
                AvailableWasteTypes = wasteTypes
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePickupRequestViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var wasteTypes = await _context.WasteTypes.ToListAsync();

            ViewBag.Fee = MisreportFee;

            var selectedItems = model.Items
                .Where(i => i.IsSelected && i.QuantityKg > 0)
                .ToList();

            if (!model.AcceptPolicy)
            {
                ModelState.AddModelError(nameof(model.AcceptPolicy), "You must agree to the Accuracy & Privacy Policy before submitting.");
            }

            if (!selectedItems.Any())
            {
                ModelState.AddModelError("", "Please select at least one waste type with a valid quantity.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableWasteTypes = wasteTypes;

                foreach (var item in model.Items)
                {
                    var wt = wasteTypes.FirstOrDefault(w => w.Id == item.WasteTypeId);
                    if (wt != null)
                    {
                        item.WasteTypeName = wt.Name;
                        item.PricePerKg = wt.PricePerKg;
                    }
                }

                return View(model);
            }

            decimal totalValue = 0;
            var requestItems = new List<RequestItem>();

            foreach (var item in selectedItems)
            {
                var wasteType = wasteTypes.FirstOrDefault(w => w.Id == item.WasteTypeId);
                if (wasteType != null)
                {
                    totalValue += item.QuantityKg * wasteType.PricePerKg;
                    requestItems.Add(new RequestItem
                    {
                        WasteTypeId = item.WasteTypeId,
                        QuantityKg = item.QuantityKg
                    });
                }
            }

            var request = new PickupRequest
            {
                UserId = user!.Id,
                Status = "Pending",
                ScheduledAt = model.ScheduledAt,
                TotalValue = totalValue,
                CreatedAt = DateTime.Now,
                PrivacyAccepted = true,
                PrivacyAcceptedAt = DateTime.Now,
                RequestItems = requestItems
            };

            _context.PickupRequests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Collector arrives and the waste is not what the customer described -> request fails and a fee is recorded
        [HttpPost]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> ReportMismatch(int id, string note)
        {
            var user = await _userManager.GetUserAsync(User);
            var request = await _context.PickupRequests.FirstOrDefaultAsync(r => r.Id == id && r.CollectorId == user!.Id);

            if (request == null || request.Status != "Accepted")
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(note) || note.Trim().Length < 5)
            {
                TempData["Error"] = "Please describe what you found (at least 5 characters).";
                return RedirectToAction("Details", new { id });
            }

            request.Status = "Failed";
            request.PenaltyAmount = MisreportFee;
            request.CollectorNote = note.Trim();
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reported. A service fee was recorded on this request.";
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.PickupRequests
                .Include(r => r.RequestItems)
                .ThenInclude(i => i.WasteType)
                .Include(r => r.User)
                .Include(r => r.Collector)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user!, "Admin");

            if (request.UserId != user!.Id && request.CollectorId != user.Id && !isAdmin)
            {
                return Forbid();
            }

            return View(request);
        }

        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> AvailableRequests()
        {
            var me = await _userManager.GetUserAsync(User);
            var requests = await _context.PickupRequests
                .Include(r => r.User)
                .Include(r => r.RequestItems)
                .ThenInclude(i => i.WasteType)
                .Where(r => r.Status == "Approved" && r.CollectorId == me!.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> AcceptRequest(int id)
        {
            var request = await _context.PickupRequests.FindAsync(id);
            var user = await _userManager.GetUserAsync(User);
            // Only the collector the admin assigned can accept an approved request
            if (request == null || request.Status != "Approved" || request.CollectorId != user!.Id)
            {
                return NotFound();
            }

            request.Status = "Accepted";

            await _context.SaveChangesAsync();
            return RedirectToAction("MyCollections");
        }

        [HttpPost]
        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var request = await _context.PickupRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (request.CollectorId != user!.Id)
            {
                return Forbid();
            }

            bool validStep =
                (status == "Accepted" && request.Status == "Approved") ||
                (status == "Collected" && request.Status == "Accepted") ||
                (status == "Completed" && request.Status == "Collected");

            if (validStep)
            {
                request.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyCollections");
        }

        [Authorize(Roles = "Collector")]
        public async Task<IActionResult> MyCollections()
        {
            var user = await _userManager.GetUserAsync(User);
            var requests = await _context.PickupRequests
                .Include(r => r.User)
                .Include(r => r.RequestItems)
                .ThenInclude(i => i.WasteType)
                .Where(r => r.CollectorId == user!.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }
    }
}