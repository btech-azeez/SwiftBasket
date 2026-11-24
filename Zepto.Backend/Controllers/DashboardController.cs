using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Zepto.Backend.Data;
using Zepto.Backend.DTOs;
using Zepto.Backend.Models;

namespace Zepto.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class DashboardController : ControllerBase
    {
        private readonly ZeptoDbContext _context;

        public DashboardController(ZeptoDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            var totalRevenue = await _context.Orders
                .Where(o => o.Status != "Cancelled")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            var totalOrders = await _context.Orders.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Placed" || o.Status == "Processing");
            var deliveredOrders = await _context.Orders.CountAsync(o => o.Status == "Delivered");

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    CustomerName = o.User != null ? o.User.FullName : "Customer",
                    CustomerEmail = o.User != null ? o.User.Email : "",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    DeliveryFee = o.TotalAmount > o.OrderItems.Sum(oi => oi.TotalPrice)
                        ? o.TotalAmount - o.OrderItems.Sum(oi => oi.TotalPrice)
                        : 0m,
                    Status = o.Status,
                    ShippingAddress = o.ShippingAddress,
                    PhoneNumber = o.PhoneNumber,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        Id = oi.Id,
                        ProductId = oi.ProductId,
                        ProductName = oi.Product != null ? oi.Product.Name : "",
                        ProductImageUrl = oi.Product != null ? oi.Product.ImageUrl : null,
                        Unit = oi.Product != null ? oi.Product.Unit : "",
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new DashboardSummaryDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                PendingOrders = pendingOrders,
                DeliveredOrders = deliveredOrders,
                RecentOrders = recentOrders
            });
        }
    }
}
