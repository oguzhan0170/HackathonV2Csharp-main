using AutoMapper;
//ekleme 
using CourseApp.BusinessLayer.Utilities.Helpers;
using CourseApp.DataAccessLayer.UnitOfWork;
using CourseApp.EntityLayer.Dto.RegistrationDto;
using CourseApp.EntityLayer.Dto.StudentDto;
using CourseApp.EntityLayer.Entity;
using CourseApp.ServiceLayer.Abstract;
using CourseApp.ServiceLayer.Utilities.Constants;
using CourseApp.ServiceLayer.Utilities.Result;
using Microsoft.EntityFrameworkCore;

namespace CourseApp.ServiceLayer.Concrete;

public class StudentManager : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public StudentManager(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IDataResult<IEnumerable<GetAllStudentDto>>> GetAllAsync(bool track = true)
    {
        var studentList = await _unitOfWork.Students.GetAll(track).ToListAsync();
        var studentListMapping = _mapper.Map<IEnumerable<GetAllStudentDto>>(studentList);
        if (!studentList.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllStudentDto>>(null, ConstantsMessages.StudentListFailedMessage);
        }
        return new SuccessDataResult<IEnumerable<GetAllStudentDto>>(studentListMapping, ConstantsMessages.StudentListSuccessMessage);
    }

    public async Task<IDataResult<GetByIdStudentDto>> GetByIdAsync(string id, bool track = true)
    {
        //  id null check
        if (string.IsNullOrWhiteSpace(id))
        {
            return new ErrorDataResult<GetByIdStudentDto>(null, "Geçersiz ID.");
        }
        // hasStudent null check
        var hasStudent = await _unitOfWork.Students.GetByIdAsync(id, false);
        if (hasStudent == null)
        {
            return new ErrorDataResult<GetByIdStudentDto>(null, "Öğrenci bulunamadı.");
        }
        //hasStudentMapping null check
        var hasStudentMapping = _mapper.Map<GetByIdStudentDto>(hasStudent);
        if (hasStudentMapping == null)
        {
            return new ErrorDataResult<GetByIdStudentDto>(null, "Öğrenci verisi dönüştürülemedi.");
        }
      
        var name = hasStudentMapping.Name; // Null reference riski
        return new SuccessDataResult<GetByIdStudentDto>(hasStudentMapping, ConstantsMessages.StudentGetByIdSuccessMessage);
    }

    public async Task<IResult> CreateAsync(CreateStudentDto entity)
    {
        if(entity == null) return new ErrorResult("Null");

        // ORTA: Tip dönüşüm hatası - string'i int'e direkt cast
        //dto da strişng olarak tutulduğu için gerek yok dönüştürmeye
        //var invalidConversion = Convert.ToInt32(entity.TC); // ORTA: InvalidCastException - string int'e dönüştürülemez

        var createdStudent = _mapper.Map<Student>(entity);
        // Null check eklendi
        if (createdStudent == null)
        {
            return new ErrorResult("Öğrenci oluşturulamadı.");
        }

        var studentName = createdStudent.Name; 
        
        await _unitOfWork.Students.CreateAsync(createdStudent);
        // ZOR: Async/await anti-pattern - .Result kullanımı deadlock'a sebep olabilir
        var result = _unitOfWork.CommitAsync().Result; // ZOR: Anti-pattern
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.StudentCreateSuccessMessage);
        }

        return new ErrorResult(ConstantsMessages.StudentCreateFailedMessage);
    }

    public async Task<IResult> Remove(DeleteStudentDto entity)
    {
        var deletedStudent = _mapper.Map<Student>(entity);
        _unitOfWork.Students.Remove(deletedStudent);
        var result = _unitOfWork.CommitAsync().GetAwaiter().GetResult();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.StudentDeleteSuccessMessage);
        }
        return new ErrorResult(ConstantsMessages.StudentDeleteFailedMessage);
    }

    public async Task<IResult> Update(UpdateStudentDto entity)
    {
        // Null check eklendi
        if (entity == null)
        {
            return new ErrorResult("Güncelleme işlemi için öğrenci bilgisi boş olamaz.");
        }

        var updatedStudent = _mapper.Map<Student>(entity);

        // null çhek ile  IndexOutOfRangeException riski kaldırıldı
        if (string.IsNullOrEmpty(entity.TC))
        {
            return new ErrorResult("Güncelleme işlemi başarısız: TC kimlik numarası boş olamaz.");
        }
        var tcFirstDigit = entity.TC[0]; 
        
        _unitOfWork.Students.Update(updatedStudent);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            // UpdateSuccessMessage ile düzeltidi
            return new SuccessResult(ConstantsMessages.StudentUpdateSuccessMessage); // HATA: UpdateSuccessMessage olmalıydı
        }
        // SuccessResult, ErrorResult ile değiştirildi 
        return new ErrorResult(ConstantsMessages.StudentUpdateFailedMessage); // HATA: ErrorResult olmalıydı
    }

    public void MissingImplementation()
    {
        var x = UnknownClass.StaticMethod();
    }
}
