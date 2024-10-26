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
public class CategoryController : ControllerBase
{
    private AppDbContext _appDbContext;
    public CategoryController(AppDbContext appDbContext)
    {
        this._appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> AllCategory()
    {
        var categories = await _appDbContext.Categories.OrderByDescending(d => d.Id).ToListAsync();
        return Ok(new { data = categories });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var categories = await _appDbContext.Categories.SingleOrDefaultAsync(s=>s.Id == id);
        if(categories == null){
            return NotFound(new { messages = "Categories not found." });
        }

        return Ok(new {data = categories});
    }

    [HttpPost("CreateCategory")]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryModel category)
    {
        if(!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var checkExisting = await _appDbContext.Categories.SingleOrDefaultAsync(s=>s.CateName == category.CateName);
        if(checkExisting != null){
            return Conflict(new { messages = "it have categories." });
        } else {
            await _appDbContext.Categories.AddAsync(category);
            await _appDbContext.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetCategory), new {id = category.Id},  new { data = category});
    }

    [HttpDelete("DeleteCategory/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var categories =  await _appDbContext.Categories.SingleOrDefaultAsync(s => s.Id == id);
        if(categories == null) {
            return NotFound(new { messages = "Categories not found." });
        }

        _appDbContext.Categories.Remove(categories);
        await _appDbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("UpdateCategory/{id}")]
    public async Task<IActionResult> UpdateCategory(int id , [FromBody] CategoryModel category) 
    {
        if(!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var existingCates = await _appDbContext.Categories.SingleOrDefaultAsync(s => s.Id == id);
        if(existingCates == null) {
            return NotFound (new { messages = "Categorise not found."});
        }
       
        existingCates.CateName = category.CateName;
        await _appDbContext.SaveChangesAsync();
        
        return Ok(new {data = existingCates });
    }


}
