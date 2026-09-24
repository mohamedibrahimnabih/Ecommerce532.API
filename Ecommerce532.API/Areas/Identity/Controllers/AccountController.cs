using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;
using LoginRequest = ECommerce532.API.DTOs.Requests.LoginRequest;
using RegisterRequest = ECommerce532.API.DTOs.Requests.RegisterRequest;

namespace Ecommerce532.API.Areas.Identity.Controllers;

[Route("api/[area]/[controller]")]
[Area(AreaConstants.IDENTITY_AREA)]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager; // Service layer => UserStore<ApplicationUser>
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;

    public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        IRepository<ApplicationUserOTP> applicationUserOTPRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _applicationUserOTPRepository = applicationUserOTPRepository;
    }

    [HttpPost] // Data: JSON => Binding: object
    [Route("Register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse()
            {
                Data = [registerRequest]
            });

        //ApplicationUser user = new()
        //{
        //    FirstName = registerRequest.FirstName,
        //    LastName = registerRequest.LastName,
        //    Email = registerRequest.Email,
        //    UserName = registerRequest.UserName,
        //    PasswordHash = registerRequest.Password,
        //    Address = registerRequest.Address,
        //};

        //TypeAdapterConfig config = new();
        //config.NewConfig<RegisterVM, ApplicationUser>()
        //    .Map("FirstName", "FName")
        //    .Map("LastName", "LName");

        ApplicationUser user = registerRequest.Adapt<ApplicationUser>(/*config*/);

        var result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (!result.Succeeded)
        {
            //foreach (var item in result.Errors)
            //{
            //    ModelState.AddModelError(string.Empty, item.Description);
            //}

            //return View(registerRequest);

            ModelStateDictionary modelState = new();

            foreach (var item in result.Errors)
            {
                modelState.AddModelError(string.Empty, item.Description);
            }

            return BadRequest(new ErrorResponse()
            {
                Data = [modelState]
            });
        }

        {
            // Send confirmation mail
            // generate unique token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("Confirm", ControllerConstants.ACCOUNT_CONTROLLER, new { area = AreaConstants.IDENTITY_AREA, user.Id, token }, Request.Scheme);
            string body = $"<h1>Please confirm your account by clicking <b><a href='{link}'>here</a></b></h1>";

            await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Account", body);
        }

        await _userManager.AddToRoleAsync(user, RoleConstants.CUSTOMER);

        var SUCCESS_NOTIFICATION = "Create Account Successfully, please verify your account";

        //return Ok(new SuccessResponse
        //{
        //    SuccessNotification = SUCCESS_NOTIFICATION
        //});

        return Created($"{Request.Scheme}://{Request.Host}/Identity/account/login", new SuccessResponse
        {
            SuccessNotification = SUCCESS_NOTIFICATION
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse()
            {
                Data = [loginRequest]
            });

        // 1. Check user name or email 
        var user = await _userManager.FindByEmailAsync(loginRequest.EmailOrUserName) ??
                                await _userManager.FindByNameAsync(loginRequest.EmailOrUserName);

        if (user is null)
        {
            ModelStateDictionary modelState = new();

            modelState.AddModelError(nameof(loginRequest.EmailOrUserName), "Invalid User Name or Email");
            modelState.AddModelError(nameof(loginRequest.Password), "Invalid Password");

            return BadRequest(new ErrorResponse()
            {
                Data = [modelState]
            });
        }

        #region Old way
        //// 2. Check password
        //bool result = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

        //if (!result)
        //{
        //    ModelStateDictionary modelState = new();

        //    modelState.AddModelError(nameof(loginRequest.EmailOrUserName), "Invalid User Name or Email");
        //    modelState.AddModelError(nameof(loginRequest.Password), "Invalid Password");

        //    return BadRequest(new ErrorResponse()
        //    {
        //        Data = [modelState]
        //    });
        //}

        //// 3. Login & Check remember me
        //await _signInManager.SignInAsync(user, loginRequest.Remember); 
        #endregion

        var signInResult = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.Remember, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return BadRequest(new ErrorResponse
            {
                ErrorNotification = "Too many attempts, please try again later"
            });
        }

        if (signInResult.IsNotAllowed)
        {
            return BadRequest(new ErrorResponse
            {
                ErrorNotification = "Please verify your account!"
            });
        }

        if (!signInResult.Succeeded)
        {
            ModelStateDictionary modelState = new();

            modelState.AddModelError(nameof(loginRequest.EmailOrUserName), "Invalid User Name or Email");
            modelState.AddModelError(nameof(loginRequest.Password), "Invalid Password");

            return BadRequest(new ErrorResponse()
            {
                Data = [modelState]
            });
        }

        var SUCCESS_NOTIFICATION = $"Welcome Back {user.FirstName} {user.LastName}";

        return Created($"{Request.Scheme}://{Request.Host}/customer/home", new SuccessResponse
        {
            SuccessNotification = SUCCESS_NOTIFICATION
        });
    }

    [HttpGet("Confirm")]
    public async Task<IActionResult> Confirm(string id, string token)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
            return NotFound(new ErrorResponse()
            {
                ErrorNotification = "user not found"
            });

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        //var ERROR_NOTIFICATION = String.Join(", ", result.Errors.Select(e => e.Description));
        {
            StringBuilder msg = new();

            foreach (var item in result.Errors)
            {
                msg.Append(item.Description);
            }

            var ERROR_NOTIFICATION = msg.ToString();

            return BadRequest(new ErrorResponse()
            {
                ErrorNotification = ERROR_NOTIFICATION
            });
        }

        var SUCCESS_NOTIFICATION = "Confirm Account successfully, please login";

        return Ok(new SuccessResponse()
        {
            SuccessNotification = SUCCESS_NOTIFICATION
        });
    }

    // ToDo:
    //[HttpPost]
    //public IActionResult ResendConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
    //{
    //    // 1. generate new token
    //    // 2. generate link
    //    // 3. generate new body
    //    // 4. send email
    //    // 5. redirect to login

    //    return View();
    //}

    [HttpPost("ForgetPassword")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse()
            {
                Data = [forgetPasswordRequest]
            });

        var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.EmailOrUserName) ??
                                await _userManager.FindByNameAsync(forgetPasswordRequest.EmailOrUserName);

        if (user is null)
        {
            ModelStateDictionary modelState = new();

            modelState.AddModelError(nameof(forgetPasswordRequest.EmailOrUserName), "Invalid User Name or Email");

            return BadRequest(new ErrorResponse()
            {
                Data = [modelState]
            });
        }

        string otp = new Random().Next(1000, 9999).ToString();

        await _applicationUserOTPRepository.CreateAsync(new()
        {
            ApplicationUserId = user.Id,
            OTP = otp,
        }, ct);
        await _applicationUserOTPRepository.CommitAsync(ct);


        string body = $"<h1>Your otp number is: {otp}. don't share it.</h1>";
        await _emailSender.SendEmailAsync(user.Email!, "Reset your account", body);

        Response.Cookies.Append("userId", user.Id);

        return Ok(new SuccessResponse()
        {
            Data = [new { RedirectToValidateOTP = Guid.NewGuid() }]
        });
    }

    [HttpPost("ValidateOTP")]
    public async Task<IActionResult> ValidateOTP(ValidateOTPRequest validateRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse()
            {
                Data = [validateRequest]
            });

        var userId = Request.Cookies["userId"];
        if (userId is null) return NotFound();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var otpInDB = _applicationUserOTPRepository
            .Get(e => e.ApplicationUserId == userId && !e.IsUsed && e.ValidTo >= DateTime.UtcNow)
            .OrderBy(e => e.CreateAt)
            .LastOrDefault();

        if (otpInDB is null || otpInDB.OTP is null || validateRequest.OTP != otpInDB.OTP)
        {
            return Ok(new ErrorResponse()
            {
                Data = [new { RedirectToValidateOTP = Guid.NewGuid() }],
                ErrorNotification = $"Invalid OTP"
            });
        }

        otpInDB.IsUsed = true;
        await _applicationUserOTPRepository.CommitAsync();

        return Ok(new SuccessResponse()
        {
            SuccessNotification = $"Valid OTP, you can now change your password"
        });
    }

    [HttpPost("NewPassword")]
    public async Task<IActionResult> NewPassword(NewPasswordRequest newPasswordRequest)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ErrorResponse()
            {
                Data = [newPasswordRequest]
            });

        var userId = Request.Cookies["userId"];
        if (userId is null) return NotFound();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return NotFound();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPasswordRequest.Password);

        if (!result.Succeeded)
        {
            ModelStateDictionary modelState = new();

            foreach (var item in result.Errors)
            {
                modelState.AddModelError(string.Empty, item.Description);
            }
            return BadRequest(new ErrorResponse()
            {
                Data = [modelState]
            });
        }

        Response.Cookies.Delete("userId");

        return Ok(new SuccessResponse()
        {
            SuccessNotification = $"Valid OTP, you can now change your password"
        });
    }

    //public IActionResult ExternalLogin()
    //{
    //    return View();
    //}
}

