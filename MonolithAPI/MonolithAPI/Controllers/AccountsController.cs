using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MonolithAPI.DTOs.Reponse;
using MonolithAPI.DTOs.Request;
using MonolithAPI.Helpers;
using MonolithAPI.Models;

namespace MonolithAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly UserManager<UserModel> userManager;
    private readonly TokenHelper tokenHelper;
    private readonly IEmailSender<UserModel> emailSender;

    public AccountsController(UserManager<UserModel> userManager, TokenHelper tokenHelper, IEmailSender<UserModel> emailSender)
    {
        this.userManager = userManager;
        this.tokenHelper = tokenHelper;
        this.emailSender = emailSender;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterUser(RegisterUserDTO request)
    {
        // convert request into user model
        var newUser = new UserModel
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };

        // create new user
        var result = await userManager.CreateAsync(newUser, request.Password!);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(x => x.Description);
            return BadRequest(new { Errors = errors });
        }

        // assign user role
        try
        {
            await userManager.AddToRoleAsync(newUser, request.Role!);
        }
        catch (Exception ex)
        {
            await userManager.DeleteAsync(newUser);
            var errors = new[] { ex.Message };
            return BadRequest(new { Errors = errors });
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginUser(LoginUserDTO request)
    {
        var user = await userManager.FindByEmailAsync(request.Email!);

        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password!))
        {
            var errors = new[] { "Invalid email or password." };
            return Unauthorized(new { Errors = errors });
        }

        var token = await tokenHelper.CreateToken(user);

        return Ok(new TokenResultDTO { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
    }

    [HttpPost("Token/Refresh")]
    public async Task<IActionResult> RefreshToken(RefreshTokenDTO request)
    {
        string? username;

        try
        {
            var claimsPrincipal = tokenHelper.GetClaimsPrincipalFromExpiredToken(request.AccessToken!);
            username = claimsPrincipal.FindFirstValue("preferred_username");
        }
        catch (Exception ex)
        {
            var errors = new[] { ex.Message };
            return Unauthorized(new { Errors = errors });
        }

        var user = await userManager.FindByNameAsync(username!);

        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            var errors = new[] { "Invalid token." };
            return Unauthorized(new { Errors = errors });
        }

        var token = await tokenHelper.CreateToken(user, false);

        return Ok(new TokenResultDTO { AccessToken = token.AccessToken, RefreshToken = token.RefreshToken });
    }

    [HttpPost("Token/Revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] object request)
    {
        var username = User.FindFirstValue("preferred_username");

        var user = await userManager.FindByNameAsync(username!);

        if (user is null)
        {
            var errors = new[] { "Invalid revoke request." };
            return BadRequest(new { Errors = errors });
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await userManager.UpdateAsync(user);

        return NoContent();
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO request)
    {
        var user = await userManager.FindByEmailAsync(request.Email!);

        if (user is not null)
        {
            var resetCode = await userManager.GeneratePasswordResetTokenAsync(user);
            resetCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetCode));
            var queryParams = new Dictionary<string, string?>{
                { "token", resetCode },
                { "email", request.Email },
            };
            var resetLink = QueryHelpers.AddQueryString(request.ClientURI!, queryParams);
            await emailSender.SendPasswordResetLinkAsync(user, request.Email!, resetLink);
        }

        return NoContent();
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO request)
    {
        var user = await userManager.FindByEmailAsync(request.Email!);

        if (user is null)
        {
            var errors = new[] { "Invalid reset password request." };
            return BadRequest(new { Errors = errors });
        }

        var resetCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token!));
        var result = await userManager.ResetPasswordAsync(user, resetCode, request.Password!);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return NoContent();
    }
}
