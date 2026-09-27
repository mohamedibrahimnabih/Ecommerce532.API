using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace Ecommerce532.API.Areas.Customer;

[Route("api/[area]/[controller]")]
[ApiController]
[Area(AreaConstants.CUSTOMER_AREA)]
[Authorize]
public class CheckoutsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<Cart> _cartRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly ILogger<CheckoutsController> _logger;

    public CheckoutsController(UserManager<ApplicationUser> userManager,
        IRepository<Cart> cartRepository,
        IRepository<Product> productRepository,
        IRepository<Order> orderRepository,
        IRepository<OrderItem> orderItemRepository,
        ApplicationDbContext applicationDbContext,
        ILogger<CheckoutsController> logger)
    {
        _userManager = userManager;
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _applicationDbContext = applicationDbContext;
        _logger = logger;
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> Success(int orderId)
    {
        //if (Request.Headers["fromPaymentAction"].Any()) return NotFound();

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return NotFound();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var transaction = _applicationDbContext.Database.BeginTransaction();

        try
        {
            // 1. Update order status
            var order = _orderRepository.GetOne(e => e.Id == orderId && e.ApplicationUserId == user.Id);
            if (order is null || order.OrderStatus >= OrderStatus.InProcessing) return NotFound();

            var service = new SessionService();
            var session = service.Get(order.SessionId);

            order.PaymentDate = DateTime.Now;
            order.PaymentStatus = PaymentStatus.Succussed;
            order.OrderStatus = OrderStatus.InProcessing;
            order.TransactionId = session.PaymentIntentId;

            await _orderRepository.CommitAsync();

            // 2. Move to order item
            var carts = _cartRepository.Get(e => e.ApplicationUserId == user.Id,
                includes: [e => e.Product]);

            foreach (var item in carts)
            {
                await _orderItemRepository.CreateAsync(new()
                {
                    OrderId = orderId,
                    Count = item.Count,
                    Price = item.CurrentPrice,
                    ProductId = item.ProductId
                });
            }
            await _orderItemRepository.CommitAsync();

            // 3. Decrease product quantity 
            foreach (var item in carts)
                item.Product.Quantity -= item.Count;

            // 4. remove cart from db
            foreach (var item in carts)
                _cartRepository.Delete(item);
            await _cartRepository.CommitAsync();

            transaction.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            transaction.Rollback();
        }

        return Ok();
    }
}
