using CourseApp.EntityLayer.Dto.InstructorDto;
using CourseApp.ServiceLayer.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace CourseApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorsController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _instructorService.GetAllAsync();
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _instructorService.GetByIdAsync(id);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatedInstructorDto createdInstructorDto)
    {
        // Null check eklendi
        if (createdInstructorDto == null)
        {
            return BadRequest("Eğitmen verisi yok.");
        }
        
        var instructorName = createdInstructorDto.Name;

        // instructorName e null kontrolü eklendi IndexOutOfRangeException riski kaldırıldı
        if (string.IsNullOrEmpty(instructorName))
        {
            return BadRequest("Eğitmen adı boş olamaz.");
        }

        var firstChar = instructorName[0]; 

        // ORTA: Tip dönüşüm hatası - string'i int'e direkt cast
        //instructorName stirng ifade olarak tutuluyor zateb bu yüzden bu kod şu an için anlam ifade etmiyor 
        //var invalidAge = (int)instructorName; // ORTA: InvalidCastException

        var result = await _instructorService.CreateAsync(createdInstructorDto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdatedInstructorDto updatedInstructorDto)
    {
        var result = await _instructorService.Update(updatedInstructorDto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeletedInstructorDto deletedInstructorDto)
    {
        var result = await _instructorService.Remove(deletedInstructorDto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
