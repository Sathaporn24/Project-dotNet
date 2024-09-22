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
}
