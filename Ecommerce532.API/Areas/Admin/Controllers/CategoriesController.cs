using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

/*
 *  MVC:
    Index (filter/search/pagination)
    Get Create (X)
    Post Create
    Get Update
    Post Update
    Delete

 *  API:
    GET Get (filter/search/pagination)
    GET GetOne
    POST Create
    PUT Update
    DELTE Delete
 */

namespace Ecommerce532.API.Areas.Admin.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area($"{AreaConstants.ADMIN_AREA}")]
public class CategoriesController : ControllerBase
{
    private readonly IRepository<Category> _repository;

    public CategoriesController(IRepository<Category> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [Authorize(Roles = $"{RoleConstants.SUPER_ADMIN},{RoleConstants.ADMIN},{RoleConstants.EMPLOYEE}")]
    public IActionResult Get(string? query, int page = 1, int size = 4)
    {
        var categories = _repository.Get();

        if (query is not null)
            categories = categories.Where(e => e.Name.ToLower().Contains(query.ToLower()));

        var totalPages = Math.Ceiling(categories.Count() / (double)size);
        categories = categories.Skip((page - 1) * size).Take(size);

        return Ok(new CategoryWithFilterResponse
        {
            Categories = categories,
            Query = query ?? "",
            TotalPages = totalPages,
            CurrentPage = page,
        });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{RoleConstants.SUPER_ADMIN},{RoleConstants.ADMIN},{RoleConstants.EMPLOYEE}")]
    public IActionResult GetOne(int id)
    {
        var category = _repository.GetOne(e => e.Id == id, tracked: false);

        if (category is null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleConstants.SUPER_ADMIN},{RoleConstants.ADMIN}")]
    public async Task<IActionResult> Create(Category category, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(category);

        //_db.Categories.Add(new Category()
        //{
        //    Name = name,
        //    Description = Description,
        //    Status = status
        //});
        //_db.Categories.Add(category);
        //_db.SaveChanges();

        await _repository.CreateAsync(category, ct);
        await _repository.CommitAsync(ct);

        //Response.Cookies.Append(NotificationConstants.SUCCESS_NOTIFICATION, "Create Category Successfully");
        //TempData[NotificationConstants.SUCCESS_NOTIFICATION] = "Create Category Successfully";

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Create Category Successfully"
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{RoleConstants.SUPER_ADMIN},{RoleConstants.ADMIN}")]
    public async Task<IActionResult> Update(int id, Category category, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(category);

        //_db.Categories.Add(new Category()
        //{
        //    Name = name,
        //    Description = Description,
        //    Status = status
        //});

        //_repository.Update(category);

        var categoryInDb = _repository.GetOne(e => e.Id == id, tracked: false);
        if (categoryInDb is null) return NotFound();

        categoryInDb.Name = categoryInDb.Name;
        categoryInDb.Description = categoryInDb.Description;
        categoryInDb.Status = categoryInDb.Status;

        await _repository.CommitAsync(ct);

        //Response.Cookies.Append(NotificationConstants.SUCCESS_NOTIFICATION, "Update Category Successfully");
        //TempData[NotificationConstants.SUCCESS_NOTIFICATION] = "Update Category Successfully";

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Update Category Successfully"
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{RoleConstants.SUPER_ADMIN},{RoleConstants.ADMIN}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var category = _repository.GetOne(e => e.Id == id);

        if (category is null)
            return NotFound();

        _repository.Delete(category);
        await _repository.CommitAsync(ct);

        //Response.Cookies.Append(NotificationConstants.SUCCESS_NOTIFICATION, "Delete Category Successfully");
        //TempData[NotificationConstants.SUCCESS_NOTIFICATION] = "Delete Category Successfully";

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Delete Category Successfully"
        });
    }
}
