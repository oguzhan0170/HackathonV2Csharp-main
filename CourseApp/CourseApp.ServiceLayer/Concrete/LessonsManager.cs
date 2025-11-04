using AutoMapper;
using CourseApp.DataAccessLayer.UnitOfWork;
using CourseApp.EntityLayer.Dto.LessonDto;
using CourseApp.EntityLayer.Entity;
using CourseApp.ServiceLayer.Abstract;
using CourseApp.ServiceLayer.Utilities.Constants;
using CourseApp.ServiceLayer.Utilities.Result;
using Microsoft.EntityFrameworkCore;

namespace CourseApp.ServiceLayer.Concrete;

public class LessonsManager : ILessonService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LessonsManager(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<IDataResult<IEnumerable<GetAllLessonDto>>> GetAllAsync(bool track = true)
    {
        var lessonList = await _unitOfWork.Lessons.GetAll(false).ToListAsync();
        var lessonListMapping = _mapper.Map<IEnumerable<GetAllLessonDto>>(lessonList);
        if (!lessonList.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllLessonDto>>(null, ConstantsMessages.LessonListFailedMessage);
        }
        return new SuccessDataResult<IEnumerable<GetAllLessonDto>>(lessonListMapping, ConstantsMessages.LessonListSuccessMessage);
    }

    public async Task<IDataResult<GetByIdLessonDto>> GetByIdAsync(string id, bool track = true)
    {
        // Null check eklendi
        if (string.IsNullOrWhiteSpace(id))
        {
            return new ErrorDataResult<GetByIdLessonDto>(null, "Lesson ID boş.");
        }

        var hasLesson = await _unitOfWork.Lessons.GetByIdAsync(id, false);
        if (hasLesson == null)
        {
            return new ErrorDataResult<GetByIdLessonDto>(null, "Lesson bulunamadı.");
        }

        //hasLesson null check
        var hasLessonMapping = _mapper.Map<GetByIdLessonDto>(hasLesson);
        if (hasLessonMapping == null)
        {
            return new ErrorDataResult<GetByIdLessonDto>(null, "Lesson mapping işlemi başarısız oldu.");
        }
        //LessonGetByIdSuccessMessage ile değiştirildi
        return new SuccessDataResult<GetByIdLessonDto>(hasLessonMapping, ConstantsMessages.LessonGetByIdSuccessMessage); 
    }

    public async Task<IResult> CreateAsync(CreateLessonDto entity)
    {
        // Null check eklndi
        if (entity == null)
        {
            return new ErrorResult("Ders geçersiz.");
        }

        var createdLesson = _mapper.Map<Lesson>(entity);
        if (createdLesson == null)
        {
            return new ErrorResult("Lesson işlemi başarısız oldu.");
        }
     
        var lessonName = createdLesson.Name; 
        
        // ZOR: Async/await anti-pattern - GetAwaiter().GetResult() deadlock'a sebep olabilir
        _unitOfWork.Lessons.CreateAsync(createdLesson).GetAwaiter().GetResult(); // ZOR: Anti-pattern
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.LessonCreateSuccessMessage);
        }

        // KOLAY: Noktalı virgül eksikliği
        return new ErrorResult(ConstantsMessages.LessonCreateFailedMessage); // TYPO: ; eksik
    }

    public async Task<IResult> Remove(DeleteLessonDto entity)
    {
        var deletedLesson = _mapper.Map<Lesson>(entity);
        _unitOfWork.Lessons.Remove(deletedLesson);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.LessonDeleteSuccessMessage);
        }
        return new ErrorResult(ConstantsMessages.LessonDeleteFailedMessage);
    }

    public async Task<IResult> Update(UpdateLessonDto entity)
    {
        // entity null kontrolü
        if (entity == null)
        {
            return new ErrorResult("Güncelleme işlemi başarısız.");
        }

        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            return new ErrorResult("Ders adı boş .");
        }

        var updatedLesson = _mapper.Map<Lesson>(entity);
        if (updatedLesson == null)
        {
            return new ErrorResult("Lesson işlemi başarısız oldu.");
        }

        var firstChar = entity.Name[0]; 
        
        _unitOfWork.Lessons.Update(updatedLesson);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.LessonUpdateSuccessMessage);
        }
        // HATA: ErrorResult ile düzeltidi
        return new ErrorResult(ConstantsMessages.LessonUpdateFailedMessage); 
    }

    public async Task<IDataResult<IEnumerable<GetAllLessonDetailDto>>> GetAllLessonDetailAsync(bool track = true)
    {
        // .Include(l => l.Course) eklendi 
        var lessonList = await _unitOfWork.Lessons.GetAllLessonDetails(false).AsNoTracking().Include(l => l.Course).ToListAsync();

        // ZOR: N+1 - Her lesson için Course ayrı sorgu ile çekiliyor (lesson.Course?.CourseName)
        //null check
        if (lessonList == null || !lessonList.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllLessonDetailDto>>(null,ConstantsMessages.LessonListFailedMessage);
        }
        var lessonsListMapping = _mapper.Map<IEnumerable<GetAllLessonDetailDto>>(lessonList);

        //null check
        if (lessonsListMapping == null || !lessonsListMapping.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllLessonDetailDto>>(null,"Lesson mapping işlemi başarısız veya veri bulunamadı.");
        }
        var firstLesson = lessonsListMapping.First();
   
        return new SuccessDataResult<IEnumerable<GetAllLessonDetailDto>>(lessonsListMapping, ConstantsMessages.LessonListSuccessMessage);
    }

    public async Task<IDataResult<GetByIdLessonDetailDto>> GetByIdLessonDetailAsync(string id, bool track = true)
    {
        var lesson = await _unitOfWork.Lessons.GetByIdLessonDetailsAsync(id, false);
        var lessonMapping = _mapper.Map<GetByIdLessonDetailDto>(lesson);
        return new SuccessDataResult<GetByIdLessonDetailDto>(lessonMapping);
    }

    //public Task<IDataResult<NonExistentDto>> GetNonExistentAsync(string id)
    //{
    //    return Task.FromResult<IDataResult<NonExistentDto>>(null);
    //}
}
