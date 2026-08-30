using Imnandi_NovaCare_Hospital_System.Data;
using Imnandi_NovaCare_Hospital_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Security.Claims;

namespace Imnandi_NovaCare_Hospital_System.Controllers
{
    [Authorize(Roles = "StockManager")]
    public class StockManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public StockManagerController(ApplicationDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> StockManagerDashboard(string? search)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
                return Unauthorized();

            var ordersQuery = _context.Order
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Consumable)
                .Where(o => o.StockManagerId == stockManager.StockManagerId)
                .AsQueryable();

            var consumablesQuery = _context.Consumable
                .Include(c => c.StockTake)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();

                ordersQuery = ordersQuery.Where(o =>
                    o.OrderName.ToLower().Contains(search) ||
                    o.Description.ToLower().Contains(search)
                );

                consumablesQuery = consumablesQuery.Where(c =>
                    c.ConsumableName.ToLower().Contains(search)
                );
            }

            var pendingOrders = await ordersQuery
                .Where(o => !o.IsReceived)
                .OrderByDescending(o => o.Date)
                .Take(3)
                .ToListAsync();

            var completedOrders = await ordersQuery
                .Where(o => o.IsReceived)
                .OrderByDescending(o => o.Date)
                .Take(3)
                .ToListAsync();

            var totalOrders = await ordersQuery.CountAsync();

            var totalConsumables = await consumablesQuery.CountAsync();

            var recentStockTakes = await _context.StockTake
                .Include(st => st.StockManager)
                .Include(st => st.Consumables)
                .OrderByDescending(st => st.Date)
                .Take(3)
                .ToListAsync();

            var lowStockConsumables = await consumablesQuery
                .Where(c => c.QuantityOnHand <= c.MinimumConsumables)
                .OrderBy(c => c.QuantityOnHand)
                .ToListAsync();

            var model = new StockManagerDashboardViewModel
            {
                StockManagerId = stockManager.StockManagerId,
                FirstName = stockManager.Employee.FirstName,
                LastName = stockManager.Employee.LastName,
                Email = user.Email ?? string.Empty,
                JobTitle = stockManager.Employee.JobTitle ?? "Stock Manager",
                PhoneNumber = stockManager.Employee.PhoneNumber,
                Department = stockManager.Department,

                TotalOrders = totalOrders,
                PendingOrderCount = pendingOrders.Count,
                CompletedOrderCount = completedOrders.Count,
                TotalConsumables = totalConsumables,

                PendingOrders = pendingOrders,
                CompletedOrders = completedOrders,
                LowStockConsumables = lowStockConsumables,
                RecentStockTakes = recentStockTakes
            };

            ViewData["Search"] = search;

            if (stockManager.Department == "Supply Chain")
            {
                return View("SupplyChainDashboard", model);
            }

            if (stockManager.Department == "Medical Stores")
            {
                return View("MedicalStoresDashboard", model);
            }

            return View("StockManagerDashboard", model);
        }




        private async Task LogAuditAsync(
           string actionTaken,
           User? user = null,
           string? entity = null,
           string? recordId = null,
           string? oldValue = null,
           string? newValue = null,
           string? failureReason = null,
           string? details = null)
        {
            var audit = new AuditLog
            {
                ActionTaken = actionTaken,
                User = user,
                UserId = user?.Id,
                Username = user?.UserName ?? User.Identity?.Name ?? "Unknown",
                Timestamp = DateTime.Now,
                Entity = entity,
                RecordId = recordId,
                OldValue = oldValue,
                NewValue = newValue,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Details = details,
                SessionId = HttpContext.Session.Id,
                FailureReason = failureReason
            };

            _context.AuditLogs.Add(audit);
            await _context.SaveChangesAsync();
        }






        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");
            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == user.Id);
            var model = new StockManagerDashboardViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.Employee.PhoneNumber,
                Department = user.Employee.Department,
                JobTitle = user.Employee.JobTitle,
                UserName = user.UserName
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(StockManagerDashboardViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var oldUserData = new
            {
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber
            };

            var employee = await _context.Employee
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employee != null)
            {
                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;
                employee.PhoneNumber = model.PhoneNumber;
                employee.Department = model.Department;
                employee.JobTitle = model.JobTitle;

                _context.Employee.Update(employee);
            }
          
            var stockmanager = await _context.StockManager
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id);

            if (stockmanager != null)
            {
                stockmanager.FirstName = model.FirstName;
                stockmanager.LastName = model.LastName;

                _context.StockManager.Update(stockmanager);
            }

            await _context.SaveChangesAsync();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                var newUserData = new
                {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.PhoneNumber
                };

                await LogAuditAsync(
                    actionTaken: "Profile Updated",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    oldValue: System.Text.Json.JsonSerializer.Serialize(oldUserData),
                    newValue: System.Text.Json.JsonSerializer.Serialize(newUserData),
                    details: $"Stock Manager {user.FirstName} {user.LastName} updated their profile."
                );

                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);

                await LogAuditAsync(
                    actionTaken: "Profile Update Failed",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    failureReason: error.Description,
                    details: $"Stock Manager {user.FirstName} {user.LastName} attempted to update profile but failed."
                );
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var oldUserData = new { user.UserName, user.Email };
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                await LogAuditAsync(
                    actionTaken: "Password Changed",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    oldValue: System.Text.Json.JsonSerializer.Serialize(oldUserData),
                    newValue: null,
                    details: $"Stock Manager {user.FirstName} {user.LastName} successfully changed their password."
                );

                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("StockManagerDashboard");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
                await LogAuditAsync(
                    actionTaken: "Password Change Failed",
                    user: user,
                    entity: "User",
                    recordId: user.Id.ToString(),
                    failureReason: error.Description,
                    details: $"Stock Manager {user.FirstName} {user.LastName} attempted to change password but failed."
                );
            }
            return View(model);
        }








        [HttpGet]
        public async Task<IActionResult> ViewAlerts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var alerts = await _context.Alerts
                .Where(a =>
                    a.IsActive &&
                    (
                        a.UserId == user.Id ||
                        (a.UserId == null && a.TargetRole == user.Role) ||
                        (a.UserId == null && a.TargetRole == null)
                    )
                    &&
                    !_context.AlertReads.Any(r =>
                        r.AlertId == a.AlertId &&
                        r.UserId == user.Id)
                )
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var alertData = alerts.Select(a => new
            {
                a.AlertId,
                a.Message
            }).ToList();

            await LogAuditAsync(
                actionTaken: "Viewed Alerts",
                user: user,
                entity: "Alert",
                recordId: "N/A",
                oldValue: "{}",
                newValue: System.Text.Json.JsonSerializer.Serialize(alertData),
                details: $"User {user.FirstName} {user.LastName} viewed {alerts.Count} active alerts."
            );

            return View(alerts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAlertRead(int alertId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var alert = await _context.Alerts.FindAsync(alertId);
            if (alert == null)
                return NotFound();

            bool canAccess =
                alert.UserId == user.Id ||
                (alert.UserId == null && alert.TargetRole == user.Role) ||
                (alert.UserId == null && alert.TargetRole == null);

            if (!canAccess)
                return Forbid();

            var alreadyRead = await _context.AlertReads
                .FirstOrDefaultAsync(r =>
                    r.AlertId == alertId &&
                    r.UserId == user.Id);

            if (alreadyRead == null)
            {
                var alertRead = new AlertRead
                {
                    AlertId = alertId,
                    UserId = user.Id,
                    IsRead = true,
                    ReadAt = DateTime.Now
                };

                _context.AlertReads.Add(alertRead);
                await _context.SaveChangesAsync();

                await LogAuditAsync(
                    actionTaken: "Marked Alert Read",
                    user: user,
                    entity: "Alert",
                    recordId: alert.AlertId.ToString(),
                    oldValue: "Unread",
                    newValue: "Read",
                    details: $"User {user.FirstName} {user.LastName} marked alert '{alert.Message}' as read."
                );

                TempData["Success"] = "Alert marked as read.";
            }
            else
            {
                TempData["Info"] = "This alert has already been marked as read.";
            }

            return RedirectToAction(nameof(ViewAlerts));
        }















        public async Task<IActionResult> ManageSuppliers()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                return Unauthorized();
            }

            var suppliers = await _context.Supplier
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            return View(suppliers);
        }

        [HttpGet]
        public IActionResult CreateSupplier()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSupplier(Supplier model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(ManageSuppliers));
            }

            var existingSupplier = await _context.Supplier
                .FirstOrDefaultAsync(s =>
                    s.SupplierName.ToLower() == model.SupplierName.ToLower() &&
                    !s.IsDeleted);

            if (existingSupplier != null)
            {
                ModelState.AddModelError(
                    "SupplierName",
                    "A supplier with this name already exists."
                );

                return View(model);
            }

            model.IsActive = true;
            model.IsDeleted = false;

            _context.Supplier.Add(model);
            await _context.SaveChangesAsync();

            var supplierInfo = new
            {
                model.SupplierId,
                model.SupplierName,
                model.ContactPerson,
                model.PhoneNumber,
                model.Email,
                model.Address,
                model.IsActive,
                model.IsDeleted
            };

            await LogAuditAsync(
                actionTaken: "Supplier Created",
                user: user,
                entity: "Supplier",
                recordId: model.SupplierId.ToString(),
                newValue: System.Text.Json.JsonSerializer.Serialize(supplierInfo),
                details: $"Stock Manager {user.FirstName} {user.LastName} created supplier {model.SupplierName}."
            );

            TempData["Success"] = "Supplier created successfully.";

            return RedirectToAction(nameof(ManageSuppliers));
        }

        [HttpGet]
        public async Task<IActionResult> EditSupplier(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(ManageSuppliers));
            }

            var supplier = await _context.Supplier
                .FirstOrDefaultAsync(s =>
                    s.SupplierId == id &&
                    !s.IsDeleted);

            if (supplier == null)
            {
                TempData["Error"] = "Supplier not found.";
                return RedirectToAction(nameof(ManageSuppliers));
            }

            return View(supplier);
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSupplier(int id, Supplier model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(ManageSuppliers));
            }
            if (id != model.SupplierId)
            {
                TempData["Error"] = "Invalid supplier information.";
                return RedirectToAction(nameof(ManageSuppliers));
            }
            var existingSupplier = await _context.Supplier
                .FirstOrDefaultAsync(s =>
                    s.SupplierId == id &&
                    !s.IsDeleted);

            if (existingSupplier == null)
            {
                TempData["Error"] = "Supplier not found.";
                return RedirectToAction(nameof(ManageSuppliers));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var duplicateSupplier = await _context.Supplier
                .FirstOrDefaultAsync(s =>
                    s.SupplierId != id &&
                    s.SupplierName.ToLower() == model.SupplierName.ToLower() &&
                    !s.IsDeleted);

            if (duplicateSupplier != null)
            {
                ModelState.AddModelError(
                    "SupplierName",
                    "A supplier with this name already exists."
                );

                return View(model);
            }

            var oldSupplierInfo = new
            {
                existingSupplier.SupplierId,
                existingSupplier.SupplierName,
                existingSupplier.ContactPerson,
                existingSupplier.PhoneNumber,
                existingSupplier.Email,
                existingSupplier.Address,
                existingSupplier.IsActive,
                existingSupplier.IsDeleted
            };

            existingSupplier.SupplierName = model.SupplierName;
            existingSupplier.ContactPerson = model.ContactPerson;
            existingSupplier.PhoneNumber = model.PhoneNumber;
            existingSupplier.Email = model.Email;
            existingSupplier.Address = model.Address;

            await _context.SaveChangesAsync();

            var newSupplierInfo = new
            {
                existingSupplier.SupplierId,
                existingSupplier.SupplierName,
                existingSupplier.ContactPerson,
                existingSupplier.PhoneNumber,
                existingSupplier.Email,
                existingSupplier.Address,
                existingSupplier.IsActive,
                existingSupplier.IsDeleted
            };

            await LogAuditAsync(
                actionTaken: "Supplier Updated",
                user: user,
                entity: "Supplier",
                recordId: existingSupplier.SupplierId.ToString(),
                oldValue: System.Text.Json.JsonSerializer.Serialize(oldSupplierInfo),
                newValue: System.Text.Json.JsonSerializer.Serialize(newSupplierInfo),
                details: $"Stock Manager {user.FirstName} {user.LastName} updated supplier {existingSupplier.SupplierName}."
            );

            TempData["Success"] = "Supplier updated successfully.";

            return RedirectToAction(nameof(ManageSuppliers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(ManageSuppliers));
            }

            var supplier = await _context.Supplier
                .FirstOrDefaultAsync(s =>
                    s.SupplierId == id &&
                    !s.IsDeleted);

            if (supplier == null)
            {
                TempData["Error"] = "Supplier not found.";
                return RedirectToAction(nameof(ManageSuppliers));
            }

            var supplierInfo = new
            {
                supplier.SupplierId,
                supplier.SupplierName,
                supplier.ContactPerson,
                supplier.PhoneNumber,
                supplier.Email,
                supplier.Address,
                supplier.IsActive,
                supplier.IsDeleted
            };

            supplier.IsDeleted = true;
            supplier.IsActive = false;

            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Supplier Deleted",
                user: user,
                entity: "Supplier",
                recordId: supplier.SupplierId.ToString(),
                newValue: System.Text.Json.JsonSerializer.Serialize(new
                {
                    supplier.SupplierId,
                    supplier.SupplierName,
                    supplier.ContactPerson,
                    supplier.PhoneNumber,
                    supplier.Email,
                    supplier.Address,
                    supplier.IsActive,
                    supplier.IsDeleted
                }),
                details: $"Stock Manager {user.FirstName} {user.LastName} soft deleted supplier {supplier.SupplierName}."
            );

            TempData["Success"] = "Supplier deleted successfully.";

            return RedirectToAction(nameof(ManageSuppliers));
        }























        public async Task<IActionResult> ManageOrders(string search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);

            if (stockManager == null)
                return NotFound();

            var orders = _context.Order
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Consumable)
                .Include(o => o.Consumables)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o =>
                    (o.OrderName ?? "").Contains(search) ||
                    (o.Description ?? "").Contains(search)
                );
            }

            var orderList = await orders.ToListAsync();
            return View(orderList);
        }

        [HttpGet]
        public IActionResult CreateOrder()
        {
            ViewBag.Consumables = _context.Consumable.Where(c => !c.IsDeleted).ToList();
            ViewBag.Wards = _context.Ward.ToList();
            ViewBag.HospitalStores = _context.HospitalStore.Where(h => !h.IsDeleted).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(Order model, List<int> consumableIds, List<int> quantities)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Consumables = _context.Consumable.Where(c => !c.IsDeleted).ToList();
            ViewBag.Wards = _context.Ward.ToList();
            ViewBag.HospitalStores = _context.HospitalStore.Where(h => !h.IsDeleted).ToList();

            if (!ModelState.IsValid)
                return View(model);

            var stockManager = await _context.StockManager
                .FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(ManageOrders));
            }

            model.StockManagerId = stockManager.StockManagerId;

            var hospitalStore = await _context.HospitalStore
                .FirstOrDefaultAsync(h => h.HospitalStoreId == model.HospitalStoreId && !h.IsDeleted);

            if (hospitalStore == null)
            {
                ModelState.AddModelError("HospitalStoreId", "Please select a valid Hospital Store.");
                return View(model);
            }

            model.Date = DateOnly.FromDateTime(DateTime.Now);
            model.IsReceived = false;

            _context.Order.Add(model);
            await _context.SaveChangesAsync();

            await AddOrderItemsAsync(model.OrderId, consumableIds, quantities);

            var orderInfo = new
            {
                model.OrderId,
                model.OrderNumber,
                model.OrderName,
                model.Date,
                model.IsReceived,
                model.StockManagerId,
                model.HospitalStoreId,
                model.WardId
            };

            await LogAuditAsync(
                actionTaken: "Order Created",
                user: user,
                entity: "Order",
                recordId: model.OrderId.ToString(),
                newValue: System.Text.Json.JsonSerializer.Serialize(orderInfo),
                details: $"Stock Manager {user.FirstName} {user.LastName} created order {model.OrderName}."
            );

            TempData["Success"] = "Order created successfully.";
            return RedirectToAction(nameof(ManageOrders));
        }

        private async Task AddOrderItemsAsync(int orderId, List<int> consumableIds, List<int> quantities)
        {
            if (consumableIds == null || quantities == null || consumableIds.Count != quantities.Count)
                return;

            for (int i = 0; i < consumableIds.Count; i++)
            {
                if (quantities[i] > 0)
                {
                    var item = new OrderItem
                    {
                        OrderId = orderId,
                        ConsumableId = consumableIds[i],
                        QuantityRequested = quantities[i]
                    };
                    _context.OrderItem.Add(item);
                }
            }

            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _context.Order
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id && !o.IsDeleted);

            if (order == null) return NotFound();

            var consumables = await _context.Consumable
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            var consumableQuantities = order.OrderItems
                .ToDictionary(oi => oi.ConsumableId, oi => oi.QuantityRequested);

            ViewBag.Consumables = consumables;
            ViewBag.ConsumableQuantities = consumableQuantities;
            ViewBag.Wards = await _context.Ward.ToListAsync();
            ViewBag.HospitalStores = await _context.HospitalStore.Where(h => !h.IsDeleted).ToListAsync();

            return View(order);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(Order model,List<int> consumableIds,List<int> quantities)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Consumables = await _context.Consumable
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            ViewBag.Wards = await _context.Ward
                .ToListAsync();

            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var order = await _context.Order
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == model.OrderId);

            if (order == null)
            {
                return NotFound();
            }
            var oldOrderInfo = new
            {
                order.OrderId,
                order.OrderNumber,
                order.OrderName,
                order.Description,
                order.Date,
                order.IsReceived,
                order.IsDeleted,
                order.StockManagerId,
                order.SupplierId,
                order.HospitalStoreId,
                order.WardId,

                OrderItems = order.OrderItems
                    .Select(oi => new
                    {
                        oi.Id,
                        oi.OrderId,
                        oi.ConsumableId,
                        oi.QuantityRequested,
                        oi.QuantityReceived
                    })
                    .ToList()
            };

            var oldValue = System.Text.Json.JsonSerializer.Serialize(oldOrderInfo);

            order.OrderName = model.OrderName;
            order.Description = model.Description;
            order.WardId = model.WardId;

            _context.Order.Update(order);

            _context.OrderItem.RemoveRange(order.OrderItems);

            if (consumableIds != null &&
                quantities != null &&
                consumableIds.Count == quantities.Count)
            {
                for (int i = 0; i < consumableIds.Count; i++)
                {
                    var item = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ConsumableId = consumableIds[i],
                        QuantityRequested = quantities[i]
                    };

                    _context.OrderItem.Add(item);
                }
            }


            await _context.SaveChangesAsync();

            var updatedOrderItems = await _context.OrderItem
                .Where(oi => oi.OrderId == order.OrderId)
                .Select(oi => new
                {
                    oi.Id,
                    oi.OrderId,
                    oi.ConsumableId,
                    oi.QuantityRequested,
                    oi.QuantityReceived
                })
                .ToListAsync();

            var newOrderInfo = new
            {
                order.OrderId,
                order.OrderNumber,
                order.OrderName,
                order.Description,
                order.Date,
                order.IsReceived,
                order.IsDeleted,
                order.StockManagerId,
                order.SupplierId,
                order.HospitalStoreId,
                order.WardId,

                OrderItems = updatedOrderItems
            };

            var newValue = System.Text.Json.JsonSerializer.Serialize(newOrderInfo);

            await LogAuditAsync(
                actionTaken: "Order Edited",
                user: user,
                entity: "Order",
                recordId: order.OrderId.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details: $"Stock Manager {user.FirstName} {user.LastName} edited order {order.OrderName}."
            );

            TempData["Success"] = "Order updated successfully.";

            return RedirectToAction(nameof(ManageOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Order
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == id &&
                    !o.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }
            var oldOrderInfo = new
            {
                order.OrderId,
                order.OrderNumber,
                order.OrderName,
                order.Description,
                order.Date,
                order.IsReceived,
                order.IsDeleted,
                order.StockManagerId,
                order.SupplierId,
                order.HospitalStoreId,
                order.WardId,

                OrderItems = order.OrderItems?
                    .Select(item => new
                    {
                        item.Id,
                        item.OrderId,
                        item.ConsumableId,
                        item.QuantityRequested,
                        item.QuantityReceived,
                        item.IsDeleted
                    })
                    .ToList()
            };

            var oldValue = System.Text.Json.JsonSerializer.Serialize(oldOrderInfo);
            order.IsDeleted = true;

            _context.Order.Update(order);
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    item.IsDeleted = true;

                    _context.OrderItem.Update(item);
                }
            }
            await _context.SaveChangesAsync();

            var newOrderInfo = new
            {
                order.OrderId,
                order.OrderNumber,
                order.OrderName,
                order.Description,
                order.Date,
                order.IsReceived,
                order.IsDeleted,
                order.StockManagerId,
                order.SupplierId,
                order.HospitalStoreId,
                order.WardId,

                OrderItems = order.OrderItems?
                    .Select(item => new
                    {
                        item.Id,
                        item.OrderId,
                        item.ConsumableId,
                        item.QuantityRequested,
                        item.QuantityReceived,
                        item.IsDeleted
                    })
                    .ToList()
            };

            var newValue = System.Text.Json.JsonSerializer.Serialize(newOrderInfo);

            await LogAuditAsync(
                actionTaken: "Order Deleted",
                user: user,
                entity: "Order",
                recordId: order.OrderId.ToString(),
                oldValue: oldValue,
                newValue: newValue,
                details:
                    $"Stock Manager {user.FirstName} {user.LastName} " +
                    $"deleted order {order.OrderName}."
            );
            TempData["Success"] = "Order deleted successfully.";

            return RedirectToAction(nameof(ManageOrders));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var order = await _context.Order
                .Include(o => o.OrderItems) 
                .FirstOrDefaultAsync(o => o.OrderId == id && o.IsDeleted);

            if (order == null) return NotFound();

            order.IsDeleted = false;
            _context.Order.Update(order);

            if (order.OrderItems != null && order.OrderItems.Any())
            {
                foreach (var item in order.OrderItems)
                {
                    item.IsDeleted = false;
                    _context.OrderItem.Update(item);
                }
            }

            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Order Restored",
                user: user,
                entity: "Order",
                recordId: order.OrderId.ToString(),
                details: $"Stock Manager {user.FirstName} {user.LastName} restored order {order.OrderName}."
            );

            TempData["Success"] = "Order restored successfully.";
            return RedirectToAction(nameof(ManageOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceiveOrder( int orderId, List<int> requestedQuantities, List<int> receivedQuantities)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                return NotFound("Stock Manager not found.");
            }
            var order = await _context.Order
                .Include(o => o.OrderItems)
                .Include(o => o.Ward)
                .FirstOrDefaultAsync(o =>
                    o.OrderId == orderId &&
                    !o.IsDeleted);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            if (order.IsReceived)
            {
                return BadRequest("Order has already been received.");
            }

            if (requestedQuantities == null ||
                receivedQuantities == null ||
                requestedQuantities.Count != order.OrderItems.Count ||
                receivedQuantities.Count != order.OrderItems.Count)
            {
                TempData["Error"] = "Quantities do not match order items.";
                return RedirectToAction(nameof(ManageOrders));
            }


            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                for (int i = 0; i < order.OrderItems.Count; i++)
                {
                    var item = order.OrderItems.ElementAt(i);

                    int requestedQty = requestedQuantities[i];
                    int receivedQty = receivedQuantities[i];

                    if (requestedQty < 0 || receivedQty < 0)
                    {
                        TempData["Error"] = "Quantities cannot be negative.";
                        await transaction.RollbackAsync();

                        return RedirectToAction(nameof(ManageOrders));
                    }

                    item.QuantityRequested = requestedQty;
                    item.QuantityReceived = receivedQty;

                    _context.OrderItem.Update(item);
                    var consumable = await _context.Consumable
                        .FirstOrDefaultAsync(c =>
                            c.ConsumableId == item.ConsumableId &&
                            !c.IsDeleted);

                    if (consumable == null)
                    {
                        throw new Exception(
                            $"Consumable with ID {item.ConsumableId} was not found."
                        );
                    }
                    consumable.OrderId = order.OrderId;
                    int oldQuantity = consumable.QuantityOnHand;
                    consumable.QuantityOnHand =
                        oldQuantity + receivedQty;

                    _context.Consumable.Update(consumable);

                    if (order.WardId.HasValue)
                    {
                        var wardStock = await _context.WardStocks
                            .FirstOrDefaultAsync(ws =>
                                ws.WardId == order.WardId.Value &&
                                ws.ConsumableId == consumable.ConsumableId);

                        if (wardStock != null)
                        {
                            wardStock.QuantityInWard += receivedQty;

                            _context.WardStocks.Update(wardStock);
                        }
                        else
                        {
                            await _context.WardStocks.AddAsync(
                                new WardStock
                                {
                                    WardId = order.WardId.Value,
                                    ConsumableId = consumable.ConsumableId,
                                    QuantityInWard = receivedQty
                                });
                        }
                    }

                    var stockTake = new StockTake
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now),

                        QuantityCountered = receivedQty,

                        StockManagerId = stockManager.StockManagerId,

                        Consumables = new Collection<Consumable>
                {
                    consumable
                },

                        WardId = order.WardId
                    };

                    await _context.StockTake.AddAsync(stockTake);

                    await LogAuditAsync(
                        actionTaken: "Consumable Stock Updated",
                        user: user,
                        entity: "Consumable",
                        recordId: consumable.ConsumableId.ToString(),
                        oldValue: oldQuantity.ToString(),
                        newValue: consumable.QuantityOnHand.ToString(),
                        details:
                            $"Stock Manager {user.FirstName} {user.LastName} " +
                            $"updated {consumable.ConsumableName}. " +
                            $"Requested quantity: {requestedQty}. " +
                            $"Received quantity: {receivedQty}. " +
                            $"Previous stock: {oldQuantity}. " +
                            $"New stock: {consumable.QuantityOnHand}."
                    );
                }

                order.IsReceived = true;

                _context.Order.Update(order);

                await LogAuditAsync(
                    actionTaken: "Order Received",
                    user: user,
                    entity: "Order",
                    recordId: order.OrderId.ToString(),
                    details:
                        $"Stock Manager {user.FirstName} {user.LastName} " +
                        $"received order {order.OrderName}."
                );

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                TempData["Success"] =
                    "Order received and stock updated successfully.";

                return RedirectToAction(nameof(ManageOrders));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "An error occurred while receiving the order. " +
                    "No changes were saved.";

                return RedirectToAction(nameof(ManageOrders));
            }
        }


        [HttpGet]
        public async Task<IActionResult> TrackDeliveries()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "You are not authorised to access delivery tracking.";
                return RedirectToAction("Index", "Home");
            }

            var orders = await _context.Order
                .Include(o => o.Supplier)
                .Include(o => o.HospitalStore)
                .Include(o => o.Ward)
                .Where(o =>
                    o.StockManagerId == stockManager.StockManagerId &&
                    !o.IsDeleted)
                .OrderByDescending(o => o.Date)
                .ToListAsync();

            return View(orders);
        }


        public async Task<IActionResult> ManageNotification(string search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var notifications = _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead);

            if (!string.IsNullOrEmpty(search))
            {
                notifications = notifications.Where(n => n.Message.Contains(search));
            }

            return View(await notifications.OrderByDescending(n => n.CreatedAt).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;

                try
                {
                    await _context.SaveChangesAsync();

                    await LogAuditAsync(
                        actionTaken: "Notification Marked as Read",
                        user: user,
                        entity: "Notification",
                        recordId: notification.NotificationId.ToString(),
                        oldValue: "{\"IsRead\": false}",
                        newValue: "{\"IsRead\": true}",
                        details: $"User {user.FirstName} {user.LastName} marked notification '{notification.Message}' as read."
                    );
                }
                catch (Exception ex)
                {
                    await LogAuditAsync(
                        actionTaken: "Notification Marked as Read Failed",
                        user: user,
                        entity: "Notification",
                        recordId: notification.NotificationId.ToString(),
                        failureReason: ex.Message,
                        details: $"User {user.FirstName} {user.LastName} attempted to mark notification '{notification.Message}' as read but failed."
                    );
                }
            }

            return RedirectToAction(nameof(ManageNotification));
        }










        public async Task<IActionResult> ManageConsumables(string search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);

            if (stockManager == null)
                return NotFound();

            var consumables = _context.Consumable
                .Include(c => c.Order)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                consumables = consumables.Where(c =>
                    (c.ConsumableName ?? "").Contains(search) ||
                    (c.Description ?? "").Contains(search)
                );
            }

            var consumableList = await consumables.ToListAsync();
            return View(consumableList);
        }

        [HttpGet]
        public async Task<IActionResult> CreateConsumable()
        {
            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateConsumable(Consumable model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                model.QuantityOnHand = model.QuantityOnHand >= 0 ? model.QuantityOnHand : 0;
                model.IsDeleted = false;

                _context.Consumable.Add(model);
                await _context.SaveChangesAsync();
                var logModel = new
                {
                    model.ConsumableId,
                    model.ConsumableName,
                    model.QuantityOnHand,
                    model.Unit,
                    model.HospitalStoreId
                };

                await LogAuditAsync(
                    actionTaken: "Created Consumable",
                    user: user,
                    entity: "Consumable",
                    recordId: model.ConsumableId.ToString(),
                    newValue: System.Text.Json.JsonSerializer.Serialize(logModel),
                    details: $"Stock Manager {user.FirstName} {user.LastName} created consumable {model.ConsumableName}."
                );

                TempData["Success"] = "Consumable created successfully.";
                return RedirectToAction(nameof(ManageConsumables));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while creating the consumable.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditConsumable(int id)
        {
            var consumable = await _context.Consumable
                .Include(c => c.HospitalStore)
                .FirstOrDefaultAsync(c => c.ConsumableId == id && !c.IsDeleted);

            if (consumable == null)
                return NotFound("Consumable not found.");

            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .ToListAsync();

            return View(consumable);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConsumable(Consumable model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(model);

            var consumable = await _context.Consumable
                .FirstOrDefaultAsync(c => c.ConsumableId == model.ConsumableId && !c.IsDeleted);

            if (consumable == null)
                return NotFound("Consumable not found.");

            var oldValue = new
            {
                consumable.ConsumableId,
                consumable.ConsumableName,
                consumable.Description,
                consumable.QuantityOnHand,
                consumable.Unit,
                consumable.ExpiryDate,
                consumable.MinimumConsumables,
                consumable.HospitalStoreId,
                consumable.OrderId
            };

            try
            {
                consumable.ConsumableName = model.ConsumableName;
                consumable.Description = model.Description;
                consumable.QuantityOnHand = model.QuantityOnHand >= 0 ? model.QuantityOnHand : 0;
                consumable.Unit = model.Unit;
                consumable.ExpiryDate = model.ExpiryDate;
                consumable.MinimumConsumables = model.MinimumConsumables;
                consumable.HospitalStoreId = model.HospitalStoreId;

                _context.Consumable.Update(consumable);
                await _context.SaveChangesAsync();

                var newValue = new
                {
                    consumable.ConsumableId,
                    consumable.ConsumableName,
                    consumable.Description,
                    consumable.QuantityOnHand,
                    consumable.Unit,
                    consumable.ExpiryDate,
                    consumable.MinimumConsumables,
                    consumable.HospitalStoreId,
                    consumable.OrderId
                };

                await LogAuditAsync(
                    actionTaken: "Edited Consumable",
                    user: user,
                    entity: "Consumable",
                    recordId: consumable.ConsumableId.ToString(),
                    oldValue: System.Text.Json.JsonSerializer.Serialize(oldValue),
                    newValue: System.Text.Json.JsonSerializer.Serialize(newValue),
                    details: $"Stock Manager {user.FirstName} {user.LastName} edited consumable {consumable.ConsumableName}."
                );

                TempData["Success"] = "Consumable updated successfully.";
                return RedirectToAction(nameof(ManageConsumables));
            }
            catch
            {
                TempData["Error"] = "An error occurred while updating the consumable.";
                return View(model);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConsumable(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var consumable = await _context.Consumable
                .FirstOrDefaultAsync(c => c.ConsumableId == id && !c.IsDeleted);

            if (consumable == null) return NotFound();

            var oldValue = System.Text.Json.JsonSerializer.Serialize(consumable);

            consumable.IsDeleted = true;
            _context.Consumable.Update(consumable);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Deleted Consumable",
                user: user,
                entity: "Consumable",
                recordId: consumable.ConsumableId.ToString(),
                oldValue: oldValue,
                newValue: null,
                details: $"Stock Manager {user.FirstName} {user.LastName} deleted consumable {consumable.ConsumableName}."
            );

            TempData["Success"] = "Consumable deleted successfully.";
            return RedirectToAction(nameof(ManageConsumables));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConsumable(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var consumable = await _context.Consumable
                .FirstOrDefaultAsync(c => c.ConsumableId == id && c.IsDeleted);

            if (consumable == null) return NotFound();

            var oldValue = System.Text.Json.JsonSerializer.Serialize(consumable);
            consumable.IsDeleted = false;
            _context.Consumable.Update(consumable);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Restored Consumable",
                user: user,
                entity: "Consumable",
                recordId: consumable.ConsumableId.ToString(),
                oldValue: oldValue,
                newValue: System.Text.Json.JsonSerializer.Serialize(consumable),
                details: $"Stock Manager {user.FirstName} {user.LastName} restored consumable {consumable.ConsumableName}."
            );

            TempData["Success"] = "Consumable restored successfully.";
            return RedirectToAction(nameof(ManageConsumables));
        }











        public async Task<IActionResult> ManageStock(string search)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);

            if (stockManager == null)
                return NotFound();

            var stockTakes = _context.StockTake
                .Include(s => s.Consumables)
                .Include(s => s.Ward)
                .Where(s => s.StockManagerId == stockManager.StockManagerId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                stockTakes = stockTakes.Where(s => (s.Ward.WardName ?? "").Contains(search));
            }

            return View(await stockTakes.OrderByDescending(s => s.Date).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> CreateStockTake()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Wards = await _context.Ward
                .Where(w => !w.IsDeleted)
                .ToListAsync();

            ViewBag.Consumables = await _context.Consumable
                .Where(c => !c.IsDeleted)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStockTake(StockTake model, int[] selectedConsumableIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewBag.Wards = await _context.Ward.Where(w => !w.IsDeleted).ToListAsync();
                ViewBag.Consumables = await _context.Consumable.Where(c => !c.IsDeleted).ToListAsync();
                return View(model);
            }

            try
            {
                var stockManager = await _context.StockManager.FirstOrDefaultAsync(sm => sm.Employee.UserId == user.Id);
                if (stockManager == null) return NotFound();

                model.StockManagerId = stockManager.StockManagerId;
                model.Consumables = new Collection<Consumable>();

                if (selectedConsumableIds != null)
                {
                    var consumables = await _context.Consumable
                        .Where(c => selectedConsumableIds.Contains(c.ConsumableId))
                        .ToListAsync();

                    foreach (var c in consumables)
                    {
                        model.Consumables.Add(c);
                    }
                }

                _context.StockTake.Add(model);
                await _context.SaveChangesAsync();

                await LogAuditAsync(
                    actionTaken: "Created StockTake",
                    user: user,
                    entity: "StockTake",
                    recordId: model.StockTakeId.ToString(),
                    newValue: System.Text.Json.JsonSerializer.Serialize(model, new System.Text.Json.JsonSerializerOptions
                    {
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                    }),
                    details: $"Stock Manager {user.FirstName} {user.LastName} created stock take for ward {(model.Ward != null ? model.Ward.WardName : "N/A")}."
                );

                TempData["Success"] = "Stock take created successfully.";
                return RedirectToAction(nameof(ManageStock));
            }
            catch
            {
                TempData["Error"] = "An error occurred while creating the stock take.";
                ViewBag.Wards = await _context.Ward.Where(w => !w.IsDeleted).ToListAsync();
                ViewBag.Consumables = await _context.Consumable.Where(c => !c.IsDeleted).ToListAsync();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditStockTake(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var stockTake = await _context.StockTake
                .Include(s => s.Consumables)
                .Include(s => s.Ward)
                .FirstOrDefaultAsync(s => s.StockTakeId == id);

            if (stockTake == null) return NotFound();

            ViewBag.WardId = (await _context.Ward
                    .Where(w => !w.IsDeleted)
                    .ToListAsync())
                .Select(w => new SelectListItem
                {
                    Value = w.WardId.ToString(),
                    Text = w.WardName,
                    Selected = stockTake.WardId == w.WardId
                })
                .ToList();

            ViewBag.Consumables = (await _context.Consumable
                    .Where(c => !c.IsDeleted)
                    .ToListAsync())
                .Select(c => new SelectListItem
                {
                    Value = c.ConsumableId.ToString(),
                    Text = c.ConsumableName,
                    Selected = stockTake.Consumables.Any(sc => sc.ConsumableId == c.ConsumableId)
                })
                .ToList();

            return View(stockTake);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStockTake(StockTake model, int[] selectedConsumableIds)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var stockTake = await _context.StockTake
                .Include(s => s.Consumables)
                .FirstOrDefaultAsync(s => s.StockTakeId == model.StockTakeId);

            if (stockTake == null) return NotFound();

            var oldValue = System.Text.Json.JsonSerializer.Serialize(new
            {
                stockTake.StockTakeId,
                stockTake.Date,
                stockTake.QuantityCountered,
                stockTake.WardId,
                ConsumableIds = stockTake.Consumables.Select(c => c.ConsumableId).ToList()
            });

            try
            {
                stockTake.Date = model.Date;
                stockTake.QuantityCountered = model.QuantityCountered;
                stockTake.WardId = model.WardId;

                stockTake.Consumables.Clear();

                if (selectedConsumableIds != null)
                {
                    var selected = await _context.Consumable
                        .Where(c => selectedConsumableIds.Contains(c.ConsumableId))
                        .ToListAsync();

                    foreach (var item in selected)
                        stockTake.Consumables.Add(item);
                }

                _context.StockTake.Update(stockTake);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Stock take updated successfully.";
                return RedirectToAction(nameof(ManageStock));
            }
            catch
            {
                TempData["Error"] = "An error occurred while updating the stock take.";

                ViewBag.WardId = (await _context.Ward.Where(w => !w.IsDeleted).ToListAsync())
                    .Select(w => new SelectListItem
                    {
                        Value = w.WardId.ToString(),
                        Text = w.WardName,
                        Selected = model.WardId == w.WardId
                    })
                    .ToList();

                ViewBag.Consumables = (await _context.Consumable.Where(c => !c.IsDeleted).ToListAsync())
                    .Select(c => new SelectListItem
                    {
                        Value = c.ConsumableId.ToString(),
                        Text = c.ConsumableName,
                        Selected = selectedConsumableIds != null && selectedConsumableIds.Contains(c.ConsumableId)
                    })
                    .ToList();

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStockTake(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var stockTake = await _context.StockTake
                .Include(s => s.Consumables)
                .Include(s => s.Ward)
                .FirstOrDefaultAsync(s => s.StockTakeId == id);

            if (stockTake == null) return NotFound();

            var oldValue = System.Text.Json.JsonSerializer.Serialize(stockTake, new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });

            stockTake.IsDeleted = true;
            _context.StockTake.Update(stockTake);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: " Deleted StockTake",
                user: user,
                entity: "StockTake",
                recordId: stockTake.StockTakeId.ToString(),
                oldValue: oldValue,
                newValue: System.Text.Json.JsonSerializer.Serialize(stockTake, new System.Text.Json.JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                }),
                details: $"Stock Manager {user.FirstName} {user.LastName} deleted stock take for ward {(stockTake.Ward != null ? stockTake.Ward.WardName : "N/A")}."
            );

            TempData["Success"] = "Stock take deleted successfully.";
            return RedirectToAction(nameof(ManageStock));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreStockTake(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var stockTake = await _context.StockTake
                .Include(s => s.Consumables)
                .Include(s => s.Ward)
                .FirstOrDefaultAsync(s => s.StockTakeId == id);

            if (stockTake == null) return NotFound();

            var oldValue = System.Text.Json.JsonSerializer.Serialize(stockTake, new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
            stockTake.IsDeleted = false;
            _context.StockTake.Update(stockTake);
            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Restored StockTake",
                user: user,
                entity: "StockTake",
                recordId: stockTake.StockTakeId.ToString(),
                oldValue: oldValue,
                newValue: System.Text.Json.JsonSerializer.Serialize(stockTake, new System.Text.Json.JsonSerializerOptions
                {
                    ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                }),
                details: $"Stock Manager {user.FirstName} {user.LastName} restored stock take for ward {(stockTake.Ward != null ? stockTake.Ward.WardName : "N/A")}."
            );

            TempData["Success"] = "Stock take restored successfully.";
            return RedirectToAction(nameof(ManageStock));
        }

        [HttpGet]
        public async Task<IActionResult> ViewStockTake(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var stockTake = await _context.StockTake
                .Include(s => s.Ward)
                .Include(s => s.Consumables)
                .FirstOrDefaultAsync(s => s.StockTakeId == id);

            if (stockTake == null)
                return NotFound();

            return View(stockTake);
        }




        [HttpGet]
        public async Task<IActionResult> ManageMedicineOrders(string search)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            var medicineOrders = _context.MedicineOrder
                .Include(o => o.StockManager)
                    .ThenInclude(sm => sm.Employee)
                .Include(o => o.Supplier)
                .Include(o => o.HospitalStore)
                .Include(o => o.MedicineOrderItems)
                    .ThenInclude(oi => oi.Medication)
                .Where(o =>
                    o.StockManagerId == stockManager.StockManagerId &&
                    !o.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                medicineOrders = medicineOrders.Where(o =>
                    (o.OrderNumber ?? "").Contains(search) ||
                    (o.OrderName ?? "").Contains(search));
            }

            var orders = await medicineOrders
                .OrderByDescending(o => o.Date)
                .ThenByDescending(o => o.MedicineOrderId)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> CreateMedicineOrder()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            ViewBag.Medications = await _context.Medication
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.MedicationName)
                .ToListAsync();

            ViewBag.Suppliers = await _context.Supplier
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.HospitalStoreName)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMedicineOrder(MedicineOrder model,List<int> medicationIds,List<int> quantities)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            string orderNumber;

            do
            {
                orderNumber =
                    $"MED-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            }
            while (await _context.MedicineOrder
                .AnyAsync(o => o.OrderNumber == orderNumber));

            ViewBag.Medications = await _context.Medication
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.MedicationName)
                .ToListAsync();

            ViewBag.Suppliers = await _context.Supplier
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.HospitalStoreName)
                .ToListAsync();

            model.OrderNumber = orderNumber;

            ModelState.Remove(nameof(MedicineOrder.OrderNumber));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (medicationIds == null ||
                quantities == null ||
                medicationIds.Count == 0 ||
                medicationIds.Count != quantities.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Please select at least one medicine and enter a quantity.");

                return View(model);
            }

            if (quantities.Any(q => q < 0))
            {
                ModelState.AddModelError(
                    "",
                    "Medicine quantities cannot be negative.");

                return View(model);
            }

            var selectedMedicines =
                new List<(int MedicationId, int Quantity)>();

            for (int i = 0; i < medicationIds.Count; i++)
            {
                if (quantities[i] > 0)
                {
                    selectedMedicines.Add(
                        (medicationIds[i], quantities[i]));
                }
            }

            if (!selectedMedicines.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Please enter a quantity greater than 0 for at least one medicine.");

                return View(model);
            }

            if (selectedMedicines
                .Select(x => x.MedicationId)
                .Distinct()
                .Count() != selectedMedicines.Count)
            {
                ModelState.AddModelError(
                    "",
                    "The same medicine cannot be selected more than once.");

                return View(model);
            }

            var selectedMedicationIds = selectedMedicines
                .Select(x => x.MedicationId)
                .ToList();

            var medications = await _context.Medication
                .Where(m =>
                    selectedMedicationIds.Contains(m.MedicationId) &&
                    !m.IsDeleted)
                .ToListAsync();

            if (medications.Count != selectedMedicationIds.Count)
            {
                ModelState.AddModelError(
                    "",
                    "One or more selected medicines could not be found.");

                return View(model);
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var medicineOrder = new MedicineOrder
                {
                    OrderNumber = orderNumber,
                    OrderName = model.OrderName,
                    Description = model.Description,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    IsReceived = false,
                    IsDeleted = false,
                    StockManagerId = stockManager.StockManagerId,
                    SupplierId = model.SupplierId,
                    HospitalStoreId = model.HospitalStoreId
                };

                _context.MedicineOrder.Add(medicineOrder);

                await _context.SaveChangesAsync();

                foreach (var selectedMedicine in selectedMedicines)
                {
                    var medicineOrderItem = new MedicineOrderItem
                    {
                        MedicineOrderId = medicineOrder.MedicineOrderId,
                        MedicationId = selectedMedicine.MedicationId,
                        QuantityRequested = selectedMedicine.Quantity,
                        QuantityReceived = 0,
                        IsDeleted = false
                    };

                    _context.MedicineOrderItem.Add(medicineOrderItem);
                }

                await _context.SaveChangesAsync();

                await LogAuditAsync(
                    actionTaken: "Medicine Order Created",
                    user: user,
                    entity: "MedicineOrder",
                    recordId: medicineOrder.MedicineOrderId.ToString(),
                    details:
                        $"Stock Manager {user.FirstName} {user.LastName} " +
                        $"created medicine order {medicineOrder.OrderNumber} " +
                        $"containing {selectedMedicines.Count} medicine item(s)."
                );

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Medicine order {medicineOrder.OrderNumber} created successfully.";

                return RedirectToAction(nameof(ManageMedicineOrders));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "An error occurred while creating the medicine order. No changes were saved.";

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditMedicineOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            var order = await _context.MedicineOrder
                .Include(o => o.MedicineOrderItems)
                    .ThenInclude(oi => oi.Medication)
                .FirstOrDefaultAsync(o =>
                    o.MedicineOrderId == id &&
                    !o.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            if (order.IsReceived)
            {
                TempData["Error"] = "A received medicine order cannot be edited.";
                return RedirectToAction(nameof(ManageMedicineOrders));
            }

            var medications = await _context.Medication
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.MedicationName)
                .ToListAsync();

            var medicationQuantities = order.MedicineOrderItems
                .ToDictionary(
                    oi => oi.MedicationId,
                    oi => oi.QuantityRequested);

            ViewBag.Medications = medications;
            ViewBag.MedicationQuantities = medicationQuantities;

            ViewBag.Suppliers = await _context.Supplier
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.HospitalStoreName)
                .ToListAsync();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedicineOrder(MedicineOrder model, List<int> medicationIds,List<int> quantities)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            ViewBag.Medications = await _context.Medication
                .Where(m => !m.IsDeleted)
                .OrderBy(m => m.MedicationName)
                .ToListAsync();

            ViewBag.Suppliers = await _context.Supplier
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.SupplierName)
                .ToListAsync();

            ViewBag.HospitalStores = await _context.HospitalStore
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.HospitalStoreName)
                .ToListAsync();

            var order = await _context.MedicineOrder
                .Include(o => o.MedicineOrderItems)
                    .ThenInclude(oi => oi.Medication)
                .FirstOrDefaultAsync(o =>
                    o.MedicineOrderId == model.MedicineOrderId &&
                    !o.IsDeleted);

            if (order == null)
            {
                return NotFound();
            }

            if (order.IsReceived)
            {
                TempData["Error"] = "A received medicine order cannot be edited.";
                return RedirectToAction(nameof(ManageMedicineOrders));
            }

            ModelState.Remove(nameof(MedicineOrder.OrderNumber));

            if (!ModelState.IsValid)
            {
                ViewBag.MedicationQuantities = medicationIds != null &&
                                               quantities != null &&
                                               medicationIds.Count == quantities.Count
                    ? medicationIds
                        .Select((id, index) => new
                        {
                            Id = id,
                            Quantity = quantities[index]
                        })
                        .ToDictionary(x => x.Id, x => x.Quantity)
                    : new Dictionary<int, int>();

                return View(model);
            }

            if (medicationIds == null ||
                quantities == null ||
                medicationIds.Count == 0 ||
                medicationIds.Count != quantities.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Please select at least one medicine and enter a quantity.");

                return View(model);
            }

            if (quantities.Any(q => q < 0))
            {
                ModelState.AddModelError(
                    "",
                    "Medicine quantities cannot be negative.");

                return View(model);
            }

            var selectedMedicines =
                new List<(int MedicationId, int Quantity)>();

            for (int i = 0; i < medicationIds.Count; i++)
            {
                if (quantities[i] > 0)
                {
                    selectedMedicines.Add(
                        (medicationIds[i], quantities[i]));
                }
            }

            if (!selectedMedicines.Any())
            {
                ModelState.AddModelError(
                    "",
                    "Please enter a quantity greater than 0 for at least one medicine.");

                return View(model);
            }

            if (selectedMedicines
                .Select(x => x.MedicationId)
                .Distinct()
                .Count() != selectedMedicines.Count)
            {
                ModelState.AddModelError(
                    "",
                    "The same medicine cannot be selected more than once.");

                return View(model);
            }

            var selectedMedicationIds = selectedMedicines
                .Select(x => x.MedicationId)
                .ToList();

            var medications = await _context.Medication
                .Where(m =>
                    selectedMedicationIds.Contains(m.MedicationId) &&
                    !m.IsDeleted)
                .ToListAsync();

            if (medications.Count != selectedMedicationIds.Count)
            {
                ModelState.AddModelError(
                    "",
                    "One or more selected medicines could not be found.");

                return View(model);
            }

            var oldValue = System.Text.Json.JsonSerializer.Serialize(
                order,
                new System.Text.Json.JsonSerializerOptions
                {
                    ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                });

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                order.OrderName = model.OrderName;
                order.Description = model.Description;
                order.SupplierId = model.SupplierId;
                order.HospitalStoreId = model.HospitalStoreId;

                _context.MedicineOrder.Update(order);

                if (order.MedicineOrderItems != null &&
                    order.MedicineOrderItems.Any())
                {
                    _context.MedicineOrderItem.RemoveRange(
                        order.MedicineOrderItems);
                }

                foreach (var selectedMedicine in selectedMedicines)
                {
                    var medicineOrderItem = new MedicineOrderItem
                    {
                        MedicineOrderId = order.MedicineOrderId,
                        MedicationId = selectedMedicine.MedicationId,
                        QuantityRequested = selectedMedicine.Quantity,
                        QuantityReceived = 0,
                        IsDeleted = false
                    };

                    _context.MedicineOrderItem.Add(medicineOrderItem);
                }

                await _context.SaveChangesAsync();

                var newValue = System.Text.Json.JsonSerializer.Serialize(
                    order,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        ReferenceHandler =
                            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                    });

                await LogAuditAsync(
                    actionTaken: "Medicine Order Edited",
                    user: user,
                    entity: "MedicineOrder",
                    recordId: order.MedicineOrderId.ToString(),
                    oldValue: oldValue,
                    newValue: newValue,
                    details:
                        $"Stock Manager {user.FirstName} {user.LastName} " +
                        $"edited medicine order {order.OrderNumber}."
                );

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Medicine order {order.OrderNumber} updated successfully.";

                return RedirectToAction(nameof(ManageMedicineOrders));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "An error occurred while editing the medicine order. No changes were saved.";

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicineOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            var medicineOrder = await _context.MedicineOrder
                .Include(o => o.MedicineOrderItems)
                .FirstOrDefaultAsync(o =>
                    o.MedicineOrderId == id &&
                    !o.IsDeleted);

            if (medicineOrder == null)
            {
                TempData["Error"] = "Medicine order not found.";
                return RedirectToAction(nameof(ManageMedicineOrders));
            }

            medicineOrder.IsDeleted = true;
            _context.MedicineOrder.Update(medicineOrder);

            if (medicineOrder.MedicineOrderItems != null &&
                medicineOrder.MedicineOrderItems.Any())
            {
                foreach (var item in medicineOrder.MedicineOrderItems)
                {
                    item.IsDeleted = true;
                    _context.MedicineOrderItem.Update(item);
                }
            }

            await _context.SaveChangesAsync();

            await LogAuditAsync(
                actionTaken: "Medicine Order Deleted",
                user: user,
                entity: "MedicineOrder",
                recordId: medicineOrder.MedicineOrderId.ToString(),
                details:
                    $"Stock Manager {user.FirstName} {user.LastName} " +
                    $"deleted medicine order {medicineOrder.OrderNumber}."
            );

            TempData["Success"] =
                $"Medicine order {medicineOrder.OrderNumber} deleted successfully.";

            return RedirectToAction(nameof(ManageMedicineOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceiveMedicineOrder(int medicineOrderId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            var medicineOrder = await _context.MedicineOrder
                .Include(o => o.MedicineOrderItems)
                    .ThenInclude(oi => oi.Medication)
                .Include(o => o.Supplier)
                .Include(o => o.HospitalStore)
                .FirstOrDefaultAsync(o =>
                    o.MedicineOrderId == medicineOrderId &&
                    !o.IsDeleted);

            if (medicineOrder == null)
            {
                TempData["Error"] = "Medicine order not found.";
                return RedirectToAction(nameof(ManageMedicineOrders));
            }

            if (medicineOrder.IsReceived)
            {
                TempData["Error"] = "This medicine order has already been received.";
                return RedirectToAction(nameof(ManageMedicineOrders));
            }

            var orderItems = medicineOrder.MedicineOrderItems
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.MedicineOrderItemId)
                .ToList();

            if (!orderItems.Any())
            {
                TempData["Error"] =
                    "This medicine order does not contain any medicine items.";

                return RedirectToAction(nameof(ManageMedicineOrders));
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in orderItems)
                {
                    if (item.Medication == null)
                    {
                        await transaction.RollbackAsync();

                        TempData["Error"] =
                            "One or more medicines in this order could not be found.";

                        return RedirectToAction(nameof(ManageMedicineOrders));
                    }

                    int requestedQty = item.QuantityRequested;
                    int receivedQty = requestedQty;

                    item.QuantityReceived = receivedQty;

                    _context.MedicineOrderItem.Update(item);

                    int oldQuantity =
                        item.Medication.QuantityOnHand ?? 0;

                    int newQuantity =
                        oldQuantity + receivedQty;

                    item.Medication.QuantityOnHand = newQuantity;

                    _context.Medication.Update(item.Medication);

                    await LogAuditAsync(
                        actionTaken: "Medicine Stock Updated",
                        user: user,
                        entity: "Medication",
                        recordId: item.Medication.MedicationId.ToString(),
                        oldValue: oldQuantity.ToString(),
                        newValue: newQuantity.ToString(),
                        details:
                            $"Stock Manager {user.FirstName} {user.LastName} " +
                            $"received {receivedQty} of " +
                            $"{item.Medication.MedicationName}. " +
                            $"Requested quantity: {requestedQty}. " +
                            $"Medicine order: {medicineOrder.OrderNumber}."
                    );
                }

                medicineOrder.IsReceived = true;

                _context.MedicineOrder.Update(medicineOrder);

                await LogAuditAsync(
                    actionTaken: "Medicine Order Received",
                    user: user,
                    entity: "MedicineOrder",
                    recordId: medicineOrder.MedicineOrderId.ToString(),
                    details:
                        $"Stock Manager {user.FirstName} {user.LastName} " +
                        $"received medicine order {medicineOrder.OrderNumber}."
                );

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"Medicine order {medicineOrder.OrderNumber} received successfully. " +
                    "Medicine stock has been updated.";

                return RedirectToAction(nameof(ManageMedicineOrders));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "An error occurred while receiving the medicine order. " +
                    "No changes were saved.";

                return RedirectToAction(nameof(ManageMedicineOrders));
            }
        }






        [HttpGet]
        public async Task<IActionResult> ManageWardStock(int? wardId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            var wards = await _context.Ward
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.WardName)
                .ToListAsync();

            if (!wardId.HasValue)
            {
                ViewBag.Wards = wards;
                ViewBag.SelectedWardId = null;
                ViewBag.SelectedWardName = "";

                return View(new List<WardStock>());
            }
            var selectedWard = await _context.Ward
                .FirstOrDefaultAsync(w =>
                    w.WardId == wardId.Value &&
                    !w.IsDeleted);

            if (selectedWard == null)
            {
                TempData["Error"] = "The selected ward could not be found.";

                ViewBag.Wards = wards;
                ViewBag.SelectedWardId = null;
                ViewBag.SelectedWardName = "";

                return View(new List<WardStock>());
            }

            var wardStocks = await _context.WardStocks
                .Include(ws => ws.Ward)
                .Include(ws => ws.Consumable)
                .Where(ws =>
                    ws.WardId == wardId.Value &&
                    ws.Consumable != null &&
                    !ws.Consumable.IsDeleted)
                .OrderBy(ws => ws.Consumable.ConsumableName)
                .ToListAsync();

            ViewBag.Wards = wards;
            ViewBag.SelectedWardId = selectedWard.WardId;
            ViewBag.SelectedWardName = selectedWard.WardName;

            return View(wardStocks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NotifyStockShortage(
            int wardId,
            int consumableId,
            string message)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ward = await _context.Ward
                .FirstOrDefaultAsync(w =>
                    w.WardId == wardId &&
                    !w.IsDeleted);

            if (ward == null)
            {
                TempData["Error"] = "The selected ward could not be found.";
                return RedirectToAction(nameof(ManageWardStock),
                    new { wardId = wardId });
            }

            var consumable = await _context.Consumable
                .FirstOrDefaultAsync(c =>
                    c.ConsumableId == consumableId &&
                    !c.IsDeleted);

            if (consumable == null)
            {
                TempData["Error"] = "The selected consumable could not be found.";
                return RedirectToAction(nameof(ManageWardStock),
                    new { wardId = wardId });
            }

            var wardStock = await _context.WardStocks
                .FirstOrDefaultAsync(ws =>
                    ws.WardId == wardId &&
                    ws.ConsumableId == consumableId);

            if (wardStock == null)
            {
                TempData["Error"] =
                    "This consumable is not currently recorded for the selected ward.";

                return RedirectToAction(nameof(ManageWardStock),
                    new { wardId = wardId });
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    !sm.IsDeleted &&
                    sm.Employee != null &&
                    sm.Employee.UserId != null);

            if (stockManager == null || stockManager.Employee == null)
            {
                TempData["Error"] =
                    "No Stock Manager could be found to receive the notification.";

                return RedirectToAction(nameof(ManageWardStock),
                    new { wardId = wardId });
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] =
                    "Please enter a message explaining the stock shortage.";

                return RedirectToAction(nameof(ManageWardStock),
                    new { wardId = wardId });
            }

            message = message.Trim();

            if (message.Length > 500)
            {
                message = message.Substring(0, 500);
            }

            var notificationMessage =
                $"Stock shortage alert: {consumable.ConsumableName} " +
                $"in {ward.WardName}. " +
                $"Current quantity: {wardStock.QuantityInWard:N0}. " +
                $"Minimum required: {consumable.MinimumConsumables:N0}. " +
                $"Message: {message}";

            var notification = new Notification
            {
                UserId = stockManager.Employee.UserId,
                Message = notificationMessage,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Stock shortage notification sent for {consumable.ConsumableName}.";

            return RedirectToAction(
                nameof(ManageWardStock),
                new { wardId = wardId });
        }










        [HttpGet]
        public async Task<IActionResult> ManageWardMedicationStock(int? wardId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            var wards = await _context.Ward
                .Where(w => !w.IsDeleted)
                .OrderBy(w => w.WardName)
                .ToListAsync();

            var medications = await _context.Medication
                .Where(m =>
                    !m.IsDeleted &&
                    (m.QuantityOnHand ?? 0) > 0)
                .OrderBy(m => m.MedicationName)
                .ToListAsync();

            ViewBag.Wards = wards;
            ViewBag.Medications = medications;

            if (!wardId.HasValue)
            {
                ViewBag.SelectedWardId = null;
                ViewBag.SelectedWardName = "";

                return View(new List<WardMedicationStock>());
            }

            var selectedWard = await _context.Ward
                .FirstOrDefaultAsync(w =>
                    w.WardId == wardId.Value &&
                    !w.IsDeleted);

            if (selectedWard == null)
            {
                TempData["Error"] = "The selected ward could not be found.";

                ViewBag.SelectedWardId = null;
                ViewBag.SelectedWardName = "";

                return View(new List<WardMedicationStock>());
            }

            var wardMedicationStocks = await _context.WardMedicationStocks
                .Include(wms => wms.Ward)
                .Include(wms => wms.Medication)
                .Where(wms =>
                    wms.WardId == wardId.Value &&
                    wms.Medication != null &&
                    !wms.Medication.IsDeleted)
                .OrderBy(wms => wms.Medication.MedicationName)
                .ToListAsync();

            ViewBag.SelectedWardId = selectedWard.WardId;
            ViewBag.SelectedWardName = selectedWard.WardName;

            return View(wardMedicationStocks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMedicationToWard(int wardId, int medicationId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockManager = await _context.StockManager
                .Include(sm => sm.Employee)
                .FirstOrDefaultAsync(sm =>
                    sm.Employee.UserId == user.Id &&
                    !sm.IsDeleted);

            if (stockManager == null)
            {
                TempData["Error"] = "Stock Manager not found.";
                return RedirectToAction(nameof(StockManagerDashboard));
            }

            if (quantity <= 0)
            {
                TempData["Error"] = "Quantity must be greater than 0.";

                return RedirectToAction(
                    nameof(ManageWardMedicationStock),
                    new { wardId });
            }

            var ward = await _context.Ward
                .FirstOrDefaultAsync(w =>
                    w.WardId == wardId &&
                    !w.IsDeleted);

            if (ward == null)
            {
                TempData["Error"] = "The selected ward could not be found.";

                return RedirectToAction(
                    nameof(ManageWardMedicationStock),
                    new { wardId });
            }

            var medication = await _context.Medication
                .FirstOrDefaultAsync(m =>
                    m.MedicationId == medicationId &&
                    !m.IsDeleted);

            if (medication == null)
            {
                TempData["Error"] = "The selected medicine could not be found.";

                return RedirectToAction(
                    nameof(ManageWardMedicationStock),
                    new { wardId });
            }

            var existingWardStock = await _context.WardMedicationStocks
                .FirstOrDefaultAsync(wms =>
                    wms.WardId == wardId &&
                    wms.MedicationId == medicationId);

            var currentWardQuantity = existingWardStock?.QuantityInWard ?? 0;

            if (medication.QuantityOnHand == null ||
                medication.QuantityOnHand < quantity)
            {
                TempData["Error"] =
                    $"There is not enough {medication.MedicationName} available in medical store stock. " +
                    $"Available: {medication.QuantityOnHand ?? 0:N0}.";

                return RedirectToAction(
                    nameof(ManageWardMedicationStock),
                    new { wardId });
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var oldMedicalStoreQuantity =
                    medication.QuantityOnHand ?? 0;

                medication.QuantityOnHand =
                    oldMedicalStoreQuantity - quantity;

                _context.Medication.Update(medication);

                if (existingWardStock != null)
                {
                    existingWardStock.QuantityInWard += quantity;

                    _context.WardMedicationStocks.Update(existingWardStock);
                }
                else
                {
                    var wardMedicationStock = new WardMedicationStock
                    {
                        WardId = wardId,
                        MedicationId = medicationId,
                        QuantityInWard = quantity
                    };

                    await _context.WardMedicationStocks.AddAsync(
                        wardMedicationStock);
                }

                var wardMedicationTransaction = new WardMedicationTransaction
                {
                    WardId = wardId,
                    MedicationId = medicationId,
                    Quantity = quantity,
                    DateReceived = DateTime.Now,
                    TransactionType = "Received",
                    IsDeleted = false
                };

                await _context.WardMedicationTransactions.AddAsync(
                    wardMedicationTransaction);

                await LogAuditAsync(
                    actionTaken: "Medicine Assigned To Ward",
                    user: user,
                    entity: "WardMedicationStock",
                    recordId: medicationId.ToString(),
                    oldValue: currentWardQuantity.ToString(),
                    newValue: (currentWardQuantity + quantity).ToString(),
                    details:
                        $"Stock Manager {user.FirstName} {user.LastName} " +
                        $"assigned {quantity:N0} of {medication.MedicationName} " +
                        $"to {ward.WardName}. " +
                        $"Medical store stock changed from " +
                        $"{oldMedicalStoreQuantity:N0} to " +
                        $"{medication.QuantityOnHand:N0}."
                );

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["Success"] =
                    $"{quantity:N0} of {medication.MedicationName} " +
                    $"was assigned to {ward.WardName} successfully.";

                return RedirectToAction(
                    nameof(ManageWardMedicationStock),
                    new { wardId });
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Error"] =
                    "An error occurred while assigning the medicine to the ward. No changes were saved.";

                return RedirectToAction(
                    nameof(ManageWardMedicationStock),
                    new { wardId });
            }
        }
    }
}