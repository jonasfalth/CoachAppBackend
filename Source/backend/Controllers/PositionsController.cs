using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoachBackend.Models;
using CoachBackend.Services;

namespace CoachBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PositionsController : ControllerBase
{
    private readonly PositionService _positionService;

    public PositionsController(PositionService positionService)
    {
        _positionService = positionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Position>>> GetAllPositions()
    {
        var positions = await _positionService.GetAllPositionsAsync();
        return Ok(positions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Position>> GetPositionById(int id)
    {
        var position = await _positionService.GetPositionByIdAsync(id);
        if (position == null)
        {
            return NotFound();
        }
        return Ok(position);
    }

    [HttpPost]
    public async Task<ActionResult<Position>> CreatePosition(Position position)
    {
        try
        {
            var createdPosition = await _positionService.CreatePositionAsync(position);
            return CreatedAtAction(nameof(GetPositionById), new { id = createdPosition.Id }, createdPosition);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Position>> UpdatePosition(int id, Position position)
    {
        if (id != position.Id)
        {
            return BadRequest("ID mismatch");
        }

        try
        {
            var updatedPosition = await _positionService.UpdatePositionAsync(position);
            return Ok(updatedPosition);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePosition(int id)
    {
        await _positionService.DeletePositionAsync(id);
        return NoContent();
    }
} 