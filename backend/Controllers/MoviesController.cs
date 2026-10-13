using Microsoft.AspNetCore.Mvc;
using mvc.BLL.DTOs;
using mvc.BLL.Interfaces;

namespace mvc.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetAll()
    {
        return Ok(await _movieService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDto>> GetById(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        return movie == null ? NotFound() : Ok(movie);
    }

    // Фільм з усіма перекладами — для форми редагування
    [HttpGet("{id:int}/translations")]
    public async Task<ActionResult<MovieEditDto>> GetTranslations(int id)
    {
        var movie = await _movieService.GetForEditAsync(id);
        return movie == null ? NotFound() : Ok(movie);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MovieDto>> Create([FromForm] MovieCreateDto model)
    {
        var posterError = _movieService.ValidatePosterFile(model.Poster);
        if (posterError != null)
            ModelState.AddModelError(nameof(model.Poster), posterError);

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var id = await _movieService.CreateAsync(model, model.Poster);
            var movie = await _movieService.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, movie);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<MovieDto>> Update(int id, [FromForm] MovieUpdateDto model)
    {
        var posterError = _movieService.ValidatePosterFile(model.Poster);
        if (posterError != null)
            ModelState.AddModelError(nameof(model.Poster), posterError);

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            if (!await _movieService.UpdateAsync(id, model, model.Poster))
                return NotFound();

            return Ok(await _movieService.GetByIdAsync(id));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _movieService.DeleteAsync(id))
            return NotFound();

        return NoContent();
    }
}
