using CourseApp.DataAccessLayer.UnitOfWork;
using CourseApp.EntityLayer.Dto.CourseDto;
using CourseApp.EntityLayer.Entity;
using CourseApp.ServiceLayer.Abstract;
using CourseApp.ServiceLayer.Utilities.Constants;
using CourseApp.ServiceLayer.Utilities.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

namespace CourseApp.ServiceLayer.Concrete;

public class CourseManager : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseManager(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IDataResult<IEnumerable<GetAllCourseDto>>> GetAllAsync(bool track = true)
    {
        // sorgu için Instructor eklendi
        var courseList = await _unitOfWork.Courses.GetAll(false).AsNoTracking().Include(c => c.Instructor).ToListAsync();

        // .Include(c => c.Instructor) ile tek sorguda tüm veriler yüklendi.
        var result = courseList.Select(course => new GetAllCourseDto
        {
            CourseName = course.CourseName,
            CreatedDate = course.CreatedDate,
            EndDate = course.EndDate,
            Id = course.ID,
            InstructorID = course.InstructorID,
            // ınculde ile tek sorguda çekilmesi için tanımlnadı
            InstructorName = course.Instructor?.Name ?? "",
            // ?? false ile boş değerlerini atlar
            IsActive = course?.IsActive ?? false,
            StartDate = course.StartDate
        }).ToList();

        // result boş olup olmadığı kontorl eddilecek
        if (result == null || !result.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllCourseDto>>(null, "Kurs listesi boş veya bulunamadı.");
        }
        var firstCourse = result[0]; // IndexOutOfRangeException riski

        return new SuccessDataResult<IEnumerable<GetAllCourseDto>>(result, ConstantsMessages.CourseListSuccessMessage);
    }

    public async Task<IDataResult<GetByIdCourseDto>> GetByIdAsync(string id, bool track = true)
    {
        //null check eklşendi
        if (string.IsNullOrWhiteSpace(id))
        {
            return new ErrorDataResult<GetByIdCourseDto>(null, "Geçersiz ID.");
        }
        var hasCourse = await _unitOfWork.Courses.GetByIdAsync(id, track);
           
        //null check eklendi
        if (hasCourse == null)
        {
            return new ErrorDataResult<GetByIdCourseDto>(null, "Belirtilen ID ile kurs bulunamadı.");
        }

        //?? string.Empty,  ile güvenli atlama yaptım
        var course = new GetByIdCourseDto
        {
            CourseName = hasCourse.CourseName ?? string.Empty, 
            CreatedDate = hasCourse.CreatedDate,
            EndDate = hasCourse.EndDate,
            InstructorID = hasCourse.InstructorID,
            IsActive = hasCourse.IsActive,
            StartDate = hasCourse.StartDate,
            Id = hasCourse.ID
        };

        return new SuccessDataResult<GetByIdCourseDto>(course, ConstantsMessages.CourseGetByIdSuccessMessage);
    }
    public async Task<IResult> CreateAsync(CreateCourseDto entity)
    {
        var createdCourse = new Course
        {
            CourseName = entity.CourseName,
            CreatedDate = entity.CreatedDate,
            EndDate = entity.EndDate,
            InstructorID = entity.InstructorID,
            IsActive = entity.IsActive,
            StartDate = entity.StartDate,
        };

        await _unitOfWork.Courses.CreateAsync(createdCourse);

        var result = await _unitOfWork.CommitAsync();

        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.CourseCreateSuccessMessage);
        }

        return new ErrorResult(ConstantsMessages.CourseCreateFailedMessage);
    }
    public async Task<IResult> Remove(DeleteCourseDto entity)
    {
        var deletedCourse = new Course
        {
            ID = entity.Id,
        };
        _unitOfWork.Courses.Remove(deletedCourse);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.CourseDeleteSuccessMessage);
        }

        return new ErrorResult(ConstantsMessages.CourseDeleteFailedMessage);
    }

    public async Task<IResult> Update(UpdateCourseDto entity)
    {
        var updatedCourse = await _unitOfWork.Courses.GetByIdAsync(entity.Id);
        if (updatedCourse == null)
        {
            return new ErrorResult(ConstantsMessages.CourseUpdateFailedMessage);
        }

        updatedCourse.CourseName = entity.CourseName;
        updatedCourse.StartDate = entity.StartDate;
        updatedCourse.EndDate = entity.EndDate;
        updatedCourse.InstructorID = entity.InstructorID;
        updatedCourse.IsActive = entity.IsActive;

        _unitOfWork.Courses.Update(updatedCourse);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.CourseUpdateSuccessMessage);
        }
        return new ErrorResult(ConstantsMessages.CourseUpdateFailedMessage);
    }

    public async Task<IDataResult<IEnumerable<GetAllCourseDetailDto>>> GetAllCourseDetail(bool track = true)
    {
        //Include(c => c.Instructor) eklendi
        var courseListDetailList = await _unitOfWork.Courses.GetAllCourseDetail(false).AsNoTracking().Include(c => c.Instructor).ToListAsync();

        var courseDetailDtoList = courseListDetailList.Select(x => new GetAllCourseDetailDto
        {
            CourseName = x.CourseName,
            StartDate = x.StartDate,
            EndDate = x.EndDate,
            CreatedDate = x.CreatedDate,
            Id = x.ID,
            InstructorID = x.InstructorID,
            // ınstructor bilgisinden Name alınıyor
            InstructorName = x.Instructor?.Name ?? "",
            IsActive = x.IsActive,
        }).ToList();

        // ORTA: Null reference - courseDetailDtoList null olabilir
        var firstDetail = courseDetailDtoList.First(); // Null/Empty durumunda exception
        if (firstDetail == null)
        {
            return new ErrorDataResult<IEnumerable<GetAllCourseDetailDto>>(null, "Kurs detayları alınamadı.");
        }

        return new SuccessDataResult<IEnumerable<GetAllCourseDetailDto>>(courseDetailDtoList, ConstantsMessages.CourseDetailsFetchedSuccessfully);
    }

    private IResult CourseNameIsNullOrEmpty(string courseName)
    {
        if(courseName == null || courseName.Length == 0)
        {
            return new ErrorResult("Kurs Adı Boş Olamaz");
        }
        return new SuccessResult();
    }

    private async Task<IResult> CourseNameUniqeCheck(string id,string courseName)
    {
        var courseNameCheck = await _unitOfWork.Courses.GetAll(false).AnyAsync(c => c.CourseName == courseName);
        if(!courseNameCheck)
        {
            return new ErrorResult("Bu kurs adi ile zaten bir kurs var");
        }
        return new SuccessResult();
    }

    private  IResult CourseNameLenghtCehck(string courseName)
    {
        if(courseName == null || courseName.Length < 2 || courseName.Length > 50)
        {
            return new ErrorResult("Kurs Adı Uzunluğu 2 - 50 Karakter Arasında Olmalı");
        }
        return new SuccessResult();
    }

    private IResult IsValidDateFormat(string date)
    {
        DateTime tempDate;
        bool isValid = DateTime.TryParse(date, out tempDate);

        if (!isValid)
        {
            return new ErrorResult("Geçersiz tarih formatı.");
        }
        return new SuccessResult();
    }
    private IResult CheckCourseDates(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            return new ErrorResult("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
        }
        return new SuccessResult();
    }
    
    private IResult CheckInstructorNameIsNullOrEmpty(string instructorName)
    {
        if (string.IsNullOrEmpty(instructorName))
        {
            return new ErrorResult("Eğitmen alanı boş olamaz");
        }

        return new SuccessResult();
    }
}

