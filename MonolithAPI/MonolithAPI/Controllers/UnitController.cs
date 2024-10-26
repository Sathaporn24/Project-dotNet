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
[Authorize]
[Route("[controller]")]
[Produces("application/json")]
public class UnitController : ControllerBase
{
    private AppDbContext _appDbContext;
    public UnitController(AppDbContext appDbContext)
    {
        this._appDbContext = appDbContext;
    }

    [HttpGet]    
    public async Task<IActionResult> AllUnits()
    {
        var units = await _appDbContext.Units.ToListAsync();
        return Ok(new { data = units });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUnits(int id)
    {
        var unit = await _appDbContext.Units.SingleOrDefaultAsync(s=>s.Id == id);
        if(unit == null){
            return NotFound(new { messages = "Unit not found." });
        }

        return Ok(new {data = unit});
    }

    [HttpPost("CreateUnits")]
    public async Task<IActionResult> CreateUnits([FromBody] UnitModel units)
    {
        if(!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var checkExisting = await _appDbContext.Units.SingleOrDefaultAsync(s => s.UnName == units.UnName);
        if (checkExisting != null)
        {
            return Conflict(new { messages = "it have units." });
        }
        else
        {
            await _appDbContext.Units.AddAsync(units);
            await _appDbContext.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetUnits), new {id = units.Id},  new { data = units});
    }

    [HttpDelete("DeleteUnits/{id}")]
    public async Task<IActionResult> DeleteUnits(int id)
    {
        var Units =  await _appDbContext.Units.SingleOrDefaultAsync(s => s.Id == id);
        if(Units == null) {
            return NotFound(new { messages = "Units not found." });
        }

        _appDbContext.Units.Remove(Units);
        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("UpdateUnits/{id}")]
    public async Task<IActionResult> UpdateUnits(int id , [FromBody] UnitModel units) 
    {
        if(!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var existingUnits = await _appDbContext.Units.SingleOrDefaultAsync(s => s.Id == id);
        if(existingUnits == null) {
            return NotFound (new { messages = "Units not found."});
        }
       
        existingUnits.UnName = units.UnName;
        await _appDbContext.SaveChangesAsync();
        
        return Ok(new {data = existingUnits });
    }
}
