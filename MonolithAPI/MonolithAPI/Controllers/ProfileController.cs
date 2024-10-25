using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using MonolithAPI.DTOs.Reponse;
using MonolithAPI.DTOs.Request;
using MonolithAPI.Models;

namespace MonolithAPI.Controllers;

[ApiController]
//[Authorize]
[Route("[controller]")]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private AppDbContext _appDbContext;
    public ProfileController(AppDbContext appDbContext)
    {
        this._appDbContext = appDbContext;
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var profile = await _appDbContext.Profiles.SingleOrDefaultAsync(s=>s.Id == id);
        if(profile == null){
            return NotFound(new { messages = "Profile not found.", status = false });
        }

        return Ok(new {data = profile, status  = true});
    }

    [HttpPost("CreateProfile")]
    public async Task<IActionResult> CreateProfile([FromBody] ProfileModel profile)
    {
        if(!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        await _appDbContext.Profiles.AddAsync(profile);
        await _appDbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProfile), new {id = profile.Id},  new { data = profile});
    }


    [HttpPut("UpdateProfile/{id}")]
    public async Task<IActionResult> UpdateProfile(Guid id, [FromBody] ProfileModel updateProfile)
    {
         if(!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var existingProfile = await _appDbContext.Profiles.SingleOrDefaultAsync(p => p.Id == id);
        if(existingProfile == null) {
            return NotFound (new { messages = "Profile not found."});
        }

        existingProfile.FullAddress = updateProfile.FullAddress;
        existingProfile.District = updateProfile.District;
        existingProfile.Amphoe = updateProfile.Amphoe;
        existingProfile.Province = updateProfile.Province;
        existingProfile.ZipCode = updateProfile.ZipCode;
        await _appDbContext.SaveChangesAsync();
        
        return Ok(new {data = existingProfile });
    }

}