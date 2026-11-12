using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Data;
using RecyclingApp.Models;
using RecyclingApp.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RecyclingApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var collectors = await _userManager.GetUsersInRoleAsync("Collector");
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            var businesses = await _userManager.GetUsersInRoleAsync("Business");
            var requests = await _context.PickupRequests.Include(r => r.RequestItems).ToListAsync();

            bool IsInProgress(PickupRequest r) => r.Status == "Approved" || r.Status == "Accepted" || r.Status == "Collected";
            var completed = requests.Where(r => r.Status == "Completed").ToList();
            var inProgress = requests.Where(IsInProgress).ToList();
            var pending = requests.Where(r => r.Status == "Pending").ToList();
            var rejected = requests.Where(r => r.Status == "Rejected").ToList();
            var failed = requests.Where(r => r.Status == "Failed").ToList();

            var model = new DashboardViewModel
            {
                TotalRequests = requests.Count,
                PendingRequests = pending.Count,
                CompletedRequests = completed.Count,
                RejectedRequests = rejected.Count,
                FailedRequests = failed.Count,
                InProgressRequests = inProgress.Count,

                // Headline numbers = completed work only
                CompletedValue = completed.Sum(r => r.TotalValue),
                CompletedWeightKg = completed.SelectMany(r => r.RequestItems).Sum(i => i.QuantityKg),
                TotalWeightKg = completed.SelectMany(r => r.RequestItems).Sum(i => i.QuantityKg),
                TotalRevenue = completed.Sum(r => r.TotalValue),

                // Per-status values (for the Order Statistics section)
                InProgressValue = inProgress.Sum(r => r.TotalValue),
                PendingValue = pending.Sum(r => r.TotalValue),
                RejectedValue = rejected.Sum(r => r.TotalValue),
                FailedValue = failed.Sum(r => r.TotalValue),

                UserCount = customers.Count + businesses.Count,
                CollectorCount = collectors.Count,
                RecentRequests = requests.OrderByDescending(r => r.Id).Take(10).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == id)
            {
                return BadRequest("Cannot delete yourself.");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction("ManageUsers");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1) If the user is a Collector: detach them from their assigned requests
                var collectedRequests = await _context.PickupRequests
                    .Where(p => p.CollectorId == id)
                    .ToListAsync();
                foreach (var r in collectedRequests)
                {
                    r.CollectorId = null;
                    if (r.Status == "Approved" || r.Status == "Accepted")
                        r.Status = "Pending"; // back to pending so admin can assign another collector
                }

                // 2) If the user is a Customer/Business: delete their requests (RequestItems cascade)
                var ownRequests = await _context.PickupRequests
                    .Where(p => p.UserId == id)
                    .ToListAsync();
                _context.PickupRequests.RemoveRange(ownRequests);

                await _context.SaveChangesAsync();

                // 3) Delete the user itself
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
                    return RedirectToAction("ManageUsers");
                }

                await transaction.CommitAsync();
                TempData["Success"] = $"User {user.Name} deleted.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Could not delete the user.";
            }

            return RedirectToAction("ManageUsers");
        }

        public async Task<IActionResult> ManageWasteTypes()
        {
            var wasteTypes = await _context.WasteTypes.ToListAsync();
            return View(wasteTypes);
        }

        [HttpGet]
        public IActionResult CreateWasteType()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateWasteType(WasteType wasteType)
        {
            if (ModelState.IsValid)
            {
                _context.WasteTypes.Add(wasteType);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageWasteTypes");
            }
            return View(wasteType);
        }

        [HttpGet]
        public async Task<IActionResult> EditWasteType(int id)
        {
            var wasteType = await _context.WasteTypes.FindAsync(id);
            if (wasteType == null)
            {
                return NotFound();
            }
            return View(wasteType);
        }

        [HttpPost]
        public async Task<IActionResult> EditWasteType(WasteType wasteType)
        {
            if (ModelState.IsValid)
            {
                _context.WasteTypes.Update(wasteType);
                await _context.SaveChangesAsync();
                return RedirectToAction("ManageWasteTypes");
            }
            return View(wasteType);
        }

        public async Task<IActionResult> AllRequests()
        {
            var requests = await _context.PickupRequests
                .Include(r => r.User)
                .Include(r => r.Collector)
                .Include(r => r.RequestItems)
                .ThenInclude(i => i.WasteType)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            ViewBag.Collectors = await _userManager.GetUsersInRoleAsync("Collector");
            return View(requests);
        }

        // Admin approves a pending request and assigns it to a collector
        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id, string collectorId)
        {
            var request = await _context.PickupRequests.FindAsync(id);
            if (request == null || request.Status != "Pending")
            {
                return NotFound();
            }

            var collector = string.IsNullOrEmpty(collectorId) ? null : await _userManager.FindByIdAsync(collectorId);
            if (collector == null || !await _userManager.IsInRoleAsync(collector, "Collector"))
            {
                TempData["Error"] = "Please choose a collector to assign this request to.";
                return RedirectToAction("AllRequests");
            }

            request.CollectorId = collector.Id;
            request.Status = "Approved";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Request #{id} approved and assigned to {collector.Name}.";
            return RedirectToAction("AllRequests");
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id)
        {
            var request = await _context.PickupRequests.FindAsync(id);
            if (request == null || request.Status != "Pending")
            {
                return NotFound();
            }

            request.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Request #{id} rejected.";
            return RedirectToAction("AllRequests");
        }
    }
}