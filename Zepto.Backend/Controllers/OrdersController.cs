using System.Security.Claims;
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
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private const decimal FreeDeliveryThreshold = 2000m;
        private const decimal DeliveryFee = 199m;
        private readonly ZeptoDbContext _context;

        public OrdersController(ZeptoDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        private bool IsAdmin()
        {
            return User.IsInRole(UserRoles.Admin);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto dto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            if (dto.Items == null || !dto.Items.Any())
            {
                return BadRequest(new { message = "Order must contain at least one product item." });
            }

            var requestedItems = dto.Items
                .GroupBy(item => item.ProductId)
                .Select(group => new { ProductId = group.Key, Quantity = group.Sum(item => item.Quantity) })
                .ToList();
            if (requestedItems.Any(item => item.Quantity > 100))
            {
                return BadRequest(new { message = "A product quantity cannot exceed 100." });
            }

            var productIds = requestedItems.Select(i => i.ProductId).ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            decimal totalAmount = 0m;
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in requestedItems)
            {
                var product = products.FirstOrDefault(p => p.Id == itemDto.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = $"Product with ID {itemDto.ProductId} was not found." });
                }

                if (!product.InStock || product.StockQuantity < itemDto.Quantity)
                {
                    return BadRequest(new { message = $"Insufficient stock for {product.Name}." });
                }

                decimal priceToUse = product.DiscountPrice ?? product.Price;
                decimal itemTotal = priceToUse * itemDto.Quantity;
                totalAmount += itemTotal;

                // Decrement stock
                product.StockQuantity -= itemDto.Quantity;
                if (product.StockQuantity <= 0)
                {
                    product.InStock = false;
                }

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = priceToUse,
                    TotalPrice = itemTotal
                });
            }

            var deliveryFee = totalAmount > 0 && totalAmount < FreeDeliveryThreshold ? DeliveryFee : 0m;
            totalAmount += deliveryFee;

            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "Placed",
                ShippingAddress = dto.ShippingAddress.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                PaymentMethod = dto.PaymentMethod.Trim(),
                PaymentStatus = "Pending",
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Load user and product relations for response
            await _context.Entry(order).Reference(o => o.User).LoadAsync();
            foreach (var oi in order.OrderItems)
            {
                await _context.Entry(oi).Reference(i => i.Product).LoadAsync();
            }

            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, MapToDto(order));
        }

        [HttpGet("my-orders")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetMyOrders()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders.Select(MapToDto));
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders([FromQuery] string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status.ToLower() == status.Trim().ToLower());
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrderById(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            if (order.UserId != userId.Value && !IsAdmin())
            {
                return Forbid();
            }

            return Ok(MapToDto(order));
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            order.Status = dto.Status.Trim();
            if (order.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase) &&
                order.PaymentMethod.Equals("Cash on Delivery", StringComparison.OrdinalIgnoreCase))
            {
                order.PaymentStatus = "Paid";
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerName = order.User?.FullName ?? "Unknown",
                CustomerEmail = order.User?.Email ?? string.Empty,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                DeliveryFee = Math.Max(0m, order.TotalAmount - order.OrderItems.Sum(item => item.TotalPrice)),
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PhoneNumber = order.PhoneNumber,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "Item",
                    ProductImageUrl = oi.Product?.ImageUrl,
                    Unit = oi.Product?.Unit ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };
        }
    }
}
