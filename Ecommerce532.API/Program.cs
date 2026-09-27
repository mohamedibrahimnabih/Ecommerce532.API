
using Ecommerce532.API.Models;
using ECommerce532.API.DataAccess;
using ECommerce532.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Stripe;
using System.Globalization;
using System.Text;
using Product = ECommerce532.API.Models.Product;

namespace Ecommerce532.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = $"{builder.Configuration["jwt:issuer"]}",
                    ValidAudience = $"{builder.Configuration["jwt:audience"]}",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes($"{builder.Configuration["jwt:signingCredentials"]}"))
                };
            });

        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        builder.Services.AddScoped<IRepository<Category>, Repository<Category>>();
        builder.Services.AddScoped<IRepository<Brand>, Repository<Brand>>();
        builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
        builder.Services.AddScoped<IBulkRepository<ProductSubImg>, BulkRepository<ProductSubImg>>();
        builder.Services.AddScoped<IBulkRepository<ProductColor>, BulkRepository<ProductColor>>();
        builder.Services.AddScoped<IRepository<ApplicationUserOTP>, Repository<ApplicationUserOTP>>();
        builder.Services.AddScoped<IRepository<Cart>, Repository<Cart>>();
        builder.Services.AddScoped<IRepository<Promotion>, Repository<Promotion>>();
        builder.Services.AddScoped<IRepository<Order>, Repository<Order>>();
        builder.Services.AddScoped<IRepository<OrderItem>, Repository<OrderItem>>();
        builder.Services.AddScoped<IRepository<UserProductReview>, Repository<UserProductReview>>();
        builder.Services.AddScoped<IRepository<ReviewImg>, Repository<ReviewImg>>();

        builder.Services.AddScoped<IDbInitializer, DbInitializer>();

        var connectionString =
                        builder.Configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string"
                            + "'DefaultConnection' not found.");

        builder.Services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseSqlServer(connectionString);
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 0;
            options.Lockout.MaxFailedAccessAttempts = 6;
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddTransient<IEmailSender, EmailSender>();

        const string defaultCulture = "en";

        var supportedCultures = new[]
        {
            new CultureInfo(defaultCulture),
            new CultureInfo("ar"),
            new CultureInfo("fr"),
        };

        builder.Services.Configure<RequestLocalizationOptions>(options => {
            options.DefaultRequestCulture = new RequestCulture(defaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
        });

        StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

        var app = builder.Build();

        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            dbInitializer.Initialize(); // Runs migrations and seeds data
        }

        app.Run();
    }
}
