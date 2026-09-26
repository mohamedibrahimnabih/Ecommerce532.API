using Ecommerce532.API.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce532.API.Areas.Admin.Controllers;

[Route("api/[area]/[controller]")]
[ApiController]
[Area(AreaConstants.ADMIN_AREA)]
[Authorize(Roles = $"{RoleConstants.SUPER_ADMIN},{RoleConstants.ADMIN},{RoleConstants.EMPLOYEE}")]
public class BrandsController : ControllerBase
{
    //private readonly ApplicationDbContext _db = new();
    private readonly IRepository<Brand> _repository;// = new Repository<Brand>();

    public BrandsController(IRepository<Brand> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Get(string? query, int page = 1, int size = 4)
    {
        var brands = _repository.Get();

        if (query is not null)
            brands = brands.Where(e => e.Name.ToLower().Contains(query.ToLower()));

        var totalPages = Math.Ceiling(brands.Count() / (double)size);
        brands = brands.Skip((page - 1) * size).Take(size);

        return Ok(new BrandWithFilterResponse
        {
            Brands = brands,
            Query = query ?? "",
            TotalPages = totalPages,
            CurrentPage = page,
        });
    }

    [HttpGet("{id}")]
    public IActionResult GetOne(int id)
    {
        var brand = _repository.GetOne(e => e.Id == id, tracked: false);

        if (brand is null)
            return NotFound();

        return Ok(brand);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] BrandCreateRequest brandCreateRequest, CancellationToken ct = default) // photo.png
    {
        Brand brand = new()
        {
            Name = brandCreateRequest.Name,
            Status = brandCreateRequest.Status,
        };

        if (brandCreateRequest.Img is not null && brandCreateRequest.Img.Length > 0)
        {
            //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Img.FileName);
            //var fileName = Img.FileName + DateTime.Now.ToString("dd-MM-yyyy") + Path.GetExtension(Img.FileName); 
            var fileName = $"{Guid.NewGuid().ToString()}-{DateTime.Now.ToString("dd-MM-yyyy")}{Path.GetExtension(brandCreateRequest.Img.FileName)}"; 

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs", "brands", fileName);

            using(var stream = System.IO.File.Create(filePath))
            {
                brandCreateRequest.Img.CopyTo(stream);
            }

            brand.Logo = fileName;
        }

        //_db.Brands.Add(new Brand()
        //{
        //    Name = name,
        //    Description = Description,
        //    Status = status
        //});

        await _repository.CreateAsync(brand, ct);
        await _repository.CommitAsync(ct);

        //TempData[NotificationConstants.SUCCESS_NOTIFICATION] = "Create Brand Successfully";

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Create Brand Successfully"
        });
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromForm] BrandUpdateRequest brandUpdateRequest, CancellationToken ct = default)
    {
        var brandInDB = _repository.GetOne(e => e.Id == brandUpdateRequest.Id);

        if (brandInDB is null) return NotFound();

        if (brandUpdateRequest.Img is not null && brandUpdateRequest.Img.Length > 0)
        {
            // Save New Img in wwwroot

            //var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Img.FileName);
            //var fileName = Img.FileName + DateTime.Now.ToString("dd-MM-yyyy") + Path.GetExtension(Img.FileName); 
            var fileName = $"{Guid.NewGuid().ToString()}-{DateTime.Now.ToString("dd-MM-yyyy")}{Path.GetExtension(brandUpdateRequest.Img.FileName)}";

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs", "brands", fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                brandUpdateRequest.Img.CopyTo(stream);
            }

            // Delete Old Img from wwwroot

            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs", "brands", brandInDB.Logo);

            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            // Replace img in DB

            brandInDB.Logo = fileName;
        }
        else
            brandInDB.Logo = brandInDB.Logo;

        //_db.Brands.Add(new Brand()
        //{
        //    Name = name,
        //    Description = Description,
        //    Status = status
        //});
        _repository.Update(brandInDB);
        await _repository.CommitAsync(ct);

        //TempData[NotificationConstants.SUCCESS_NOTIFICATION] = "Update Brand Successfully";

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Update Brand Successfully"
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        var brand = _repository.GetOne(e => e.Id == id);

        if (brand is null)
            return NotFound();

        _repository.Update(brand);
        await _repository.CommitAsync(ct);

        //TempData[NotificationConstants.SUCCESS_NOTIFICATION] = "Delete Brand Successfully";

        return Ok(new SuccessResponse()
        {
            SuccessNotification = "Delete Brand Successfully"
        });
    }
}
