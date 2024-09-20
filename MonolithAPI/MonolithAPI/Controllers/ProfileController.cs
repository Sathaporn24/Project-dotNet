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

    [HttpGet()]
    public async Task<IActionResult> AllProfile()
    {
        var profile = await _appDbContext.Profiles.ToListAsync();
        return Ok(new {data = profile});
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProfile(Guid id)
    {
        var profile = await _appDbContext.Profiles.SingleOrDefaultAsync(s=>s.Id == id);
        if(profile == null){
            return NotFound(new { messages = "Profile not found." });
        }

        return Ok(new {data = profile});
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

    [HttpDelete("DeleteProfile/{id}")]
    public async Task<IActionResult> DeleteProfile(Guid id)
    {
        var profile =  await _appDbContext.Profiles.SingleOrDefaultAsync(p => p.Id == id);
        if(profile == null) {
            return NotFound(new { messages = "Profile not found." });
        }

        _appDbContext.Profiles.Remove(profile);
        await _appDbContext.SaveChangesAsync();

        return NoContent();
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

        _appDbContext.Profiles.Update(updateProfile);
        await _appDbContext.SaveChangesAsync();
        
        return Ok(new {data = existingProfile });
    }

}