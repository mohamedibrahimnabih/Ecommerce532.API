using Ecommerce532.API.DTOs.Requests;
using Ecommerce532.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Ecommerce532.API.Areas.Customer;

[Route("api/[area]/[controller]")]
[ApiController]
[Area(AreaConstants.CUSTOMER_AREA)]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<UserProductReview> _userProductReviewRepository;
    private readonly IRepository<Product> _product;
    private readonly IStringLocalizer<Localization> _localizer;

    public ReviewsController(UserManager<ApplicationUser> userManager,
        IRepository<Order> orderRepository,
        IRepository<OrderItem> orderItemRepository,
        IRepository<UserProductReview> userProductReviewRepository,
        IRepository<Product> product,
        IStringLocalizer<Localization> localizer)
    {
        _userManager = userManager;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _userProductReviewRepository = userProductReviewRepository;
        _product = product;
        _localizer = localizer;
    }

    [HttpGet]
    public IActionResult Get(string? query, int page = 1, int size = 4)
    {
        var reviews = _userProductReviewRepository.Get();

        if (query is not null)
            reviews = reviews.Where(e => e.Comment.ToLower().Contains(query.ToLower()));

        var totalPages = Math.Ceiling(reviews.Count() / (double)size);
        reviews = reviews.Skip((page - 1) * size).Take(size);

        return Ok(new ReviewWithFilterResponse
        {
            Reviews = reviews,
            Query = query ?? "",
            TotalPages = totalPages,
            CurrentPage = page,
        });
    }

    [HttpGet("{id}")]
    public IActionResult GetOne(int id)
    {
        var review = _userProductReviewRepository.GetOne(e => e.Id == id, tracked: false);

        if (review is null)
            return NotFound();

        return Ok(review);
    }

    [HttpPost("Comment")]
    public async Task<IActionResult> Create(CreateNewReview createNewReview, CancellationToken ct = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return NotFound();

        var user = await _userManager.FindByIdAsync(userId);
        if(user is null) return NotFound();

        //e.UpdateAt.Value.AddDays(-7)

        //var diffDate = (DateTime.Now - e.UpdateAt.Value).TotalDays > 7

        var userOrders = _orderRepository.Get(e => e.ApplicationUserId == userId && e.OrderStatus == OrderStatus.Completed);

        foreach (var item in userOrders)
        {
            var userOrderItems = _orderItemRepository.Get(e => e.OrderId == item.Id && e.ProductId == createNewReview.productId);

            if(userOrderItems.Any())
            {
                await _userProductReviewRepository.CreateAsync(new()
                {
                    ApplicationUserId = userId,
                    Comment = createNewReview.comment,
                    Rate = createNewReview.rate,
                    ProductId = createNewReview.productId,
                }, ct);
                await _userProductReviewRepository.CommitAsync(ct);

                return Ok(new SuccessResponse()
                {
                    SuccessNotification = _localizer["CreateNewReview"].Value
                });
            }
        }

        return BadRequest(new ErrorResponse()
        {
            ErrorNotification = _localizer["FailedInCreateNewReview"].Value
        });
    }

    [HttpPost("Reply")]
    public async Task<IActionResult> Reply(CreateNewReply createNewReply)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return NotFound();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var product = _product.GetOne(e => e.Id == createNewReply.productId);
        if (product is null) return NotFound();

        if (product.CreatedById != userId)
            return BadRequest(new ErrorResponse()
            {
                ErrorNotification = "You can not reply this comment"
            });

        var review = _userProductReviewRepository.GetOne(e => e.Id == createNewReply.reviewId);
        if (review is null) return NotFound();

        review.Reply = createNewReply.reply;
        await _userProductReviewRepository.CommitAsync();

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Add Reply successfully"
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateReviewRequest updateReviewRequest)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return NotFound();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var review = _userProductReviewRepository.GetOne(e => e.Id == updateReviewRequest.reviewId);
        if (review is null) return NotFound();

        if(review.Reply is not null)
            return BadRequest(new ErrorResponse()
            {
                ErrorNotification = "You can not update this comment"
            });

        review.Rate = updateReviewRequest.newRate;
        review.Comment = updateReviewRequest.newComment;

        await _userProductReviewRepository.CommitAsync();

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Update review successfully"
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{RoleConstants.SUPER_ADMIN},{RoleConstants.ADMIN},{RoleConstants.EMPLOYEE}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var review = _userProductReviewRepository.GetOne(e => e.Id == id);

        if (review is null) return NotFound();

        _userProductReviewRepository.Delete(review);
        await _userProductReviewRepository.CommitAsync(ct);

        //Response.Cookies.Append(NotificationConstants.SUCCESS_NOTIFICATION, "Delete Category Successfully");
        //TempData[NotificationConstants.SUCCESS_NOTIFICATION] = "Delete Category Successfully";

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Delete review Successfully"
        });
    }
}
