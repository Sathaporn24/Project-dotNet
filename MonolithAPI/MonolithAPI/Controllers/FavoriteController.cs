using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using MonolithAPI.DTOs.Reponse;
using MonolithAPI.DTOs.Request;
using MonolithAPI.Models;

namespace MonolithAPI.Controllers;

[ApiController]
//[Authorize]
[Route("[controller]")]
[Produces("application/json")]
public class FavoriteController : ControllerBase
{
    private AppDbContext _appDbContext;
    public FavoriteController(AppDbContext appDbContext)
    {
        this._appDbContext = appDbContext;
    }

    [HttpGet]    
    public async Task<IActionResult> AllFavorite()
    {
        var favorites = await _appDbContext.Favorites.ToListAsync();
        return Ok(new { data = favorites });
    }

    [HttpPost("AddFavorite")]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteModel favorite)
    {
        if(!ModelState.IsValid) {
            return BadRequest(ModelState);
        }
        var checkFav = _appDbContext.Favorites.Where(w=>w.ProductId == favorite.ProductId);
        if(checkFav.Any()){
            return Ok(new { messages = "Product is Already." }); 
        }else{
            await _appDbContext.Favorites.AddAsync(favorite);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { message = "Sucessful!" });
        }
    }

    [HttpDelete("RemoveFavorite/{id}")]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        var Favorite =  await _appDbContext.Favorites.SingleOrDefaultAsync(s => s.Id == id);
        if(Favorite == null) {
            return NotFound(new { messages = "Favorite not found." });
        }

        _appDbContext.Favorites.Remove(Favorite);
        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }
}