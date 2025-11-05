using CourseApp.EntityLayer.Dto.StudentDto;
using CourseApp.ServiceLayer.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
// KOLAY: Eksik using - System.Text.Json kullanılıyor ama using yok
//DataAccessLayer ulaşması katman ihlali yapıyor o yüzden sildim
//using CourseApp.DataAccessLayer.Context; 
namespace CourseApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    // direkt DataAccess katmanına erişimi engellemk için sildim
    //private readonly AppDbContext _dbContext;
    // ORTA: Değişken tanımlandı ama asla kullanılmadı ve null olabilir
    // değişkene başlangıç için new ile değer verildi
    private List<GetAllStudentDto> _cachedStudents = new();

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
        //direk DataAccessLayer ulaşmaması için AppDbContext dbContext kaldırıldı
        //_dbContext = dbContext; 

    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        //Null reference exception riski, ull kontrolü '?.Count > 0' gidelirdi
        if (_cachedStudents?.Count > 0)
        {
            return Ok(_cachedStudents); // Mantıksal hata: cache kontrolü yanlış
        }
        
        var result = await _studentService.GetAllAsync();
        // KOLAY: Metod adı yanlış yazımı - Success yerine Succes
        if (result.Success) // TYPO: Success yerine Succes
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        // ORTA: Null check eksik - id null/empty olabilir
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Geçersiz ID değeri yok.");
        }

        // ORTA: Index out of range riski - string.Length kullanımı yanlış olabilir
        //id length 10 dan kısa olması durumları
        if (id.Length <= 10)
        {
            return BadRequest("ID kısa.");
        }
        var studentIdPrefix = id[10];

        var result = await _studentService.GetByIdAsync(id);

        // Null check
        if (result.Data == null)
        {
            return NotFound("Öğrenci bulunamadı.");
        }

        // Null check , isim kontrolü
        if (string.IsNullOrWhiteSpace(result.Data.Name))
        {
            result.Data.Name = "Bilinmiyor";
        }
        var studentName = result.Data.Name; 
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStudentDto createStudentDto)
    {
        // Null check eklendi
        if (createStudentDto == null)
        {
            return BadRequest("Öğrenci bilgileri eksik veya geçersiz.");
        }
        // ORTA: Tip dönüşüm hatası - string'i int'e direkt atama
        //hatalı tip dönüşü çıkarıldı
        //var invalidAge = createStudentDto.Surname;  // ORTA: InvalidCastException - string int'e dönüştürülemez
        //CreateStudtenDTO da Age ile ilgi bir yer olmadığı için invildeAge anlamsız kalıyordu onu Surname ile değiştirdim.

        // ZOR: Katman ihlali - Controller'dan direkt DbContext'e erişim (Business Logic'i bypass ediyor)
        //var directDbAccess = _dbContext.Students.Add(new CourseApp.EntityLayer.Entity.Student 
        //{ 
        //    Name = createStudentDto.Name 
        //});
        
        var result = await _studentService.CreateAsync(createStudentDto);
        if (result.Success)
        {
            return Ok(result);
        }
        // KOLAY: Noktalı virgül eksikliği
        return BadRequest(result); // TYPO: ; eksik
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateStudentDto updateStudentDto)
    {
        // KOLAY: Değişken adı typo - updateStudentDto yerine updateStudntDto
        var name = updateStudentDto.Name; // TYPO

        var result = await _studentService.Update(updateStudentDto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteStudentDto deleteStudentDto)
    {
        // deleteStudentDto null  check
        if (deleteStudentDto == null)
        {
            return BadRequest("Silinecek öğrenci bilgisi bulunamadı.");
        }
        // stribg kontrolü
        if (string.IsNullOrWhiteSpace(deleteStudentDto.Id))
        {
            return BadRequest("Geçersiz ID.");
        }
        var id = deleteStudentDto.Id;

        // Memory leak ve katman ihlali oluşturan AppDbContext kaldırıldı
        //var tempContext = new AppDbContext(new Microsoft.EntityFrameworkCore.DbContextOptions<AppDbContext>());
        //tempContext.Students.ToList(); // Dispose edilmeden kullanılıyor

        var result = await _studentService.Remove(deleteStudentDto);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
