using AutoMapper;
using CourseApp.DataAccessLayer.UnitOfWork;
using CourseApp.EntityLayer.Dto.InstructorDto;
using CourseApp.EntityLayer.Entity;
using CourseApp.ServiceLayer.Abstract;
using CourseApp.ServiceLayer.Utilities.Constants;
using CourseApp.ServiceLayer.Utilities.Result;
using Microsoft.EntityFrameworkCore;

namespace CourseApp.ServiceLayer.Concrete;

public class InstructorManager : IInstructorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public InstructorManager(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IDataResult<IEnumerable<GetAllInstructorDto>>> GetAllAsync(bool track = true)
    {
        var instructorList = await _unitOfWork.Instructors.GetAll(false).ToListAsync();
        var instructorListMapping = _mapper.Map<IEnumerable<GetAllInstructorDto>>(instructorList);
        if (!instructorList.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllInstructorDto>>(null, ConstantsMessages.InstructorListFailedMessage);
        }
        return new SuccessDataResult<IEnumerable<GetAllInstructorDto>>(instructorListMapping, ConstantsMessages.InstructorListSuccessMessage);
    }

    public async Task<IDataResult<GetByIdInstructorDto>> GetByIdAsync(string id, bool track = true)
    {
        // Null check eklendi
        if (string.IsNullOrWhiteSpace(id))
        {
            return new ErrorDataResult<GetByIdInstructorDto>(null, "Instructor ID boş olamaz.");
        }

        // id uzunluğu kontorlu
        if (id.Length <= 5)
        {
            return new ErrorDataResult<GetByIdInstructorDto>(null, "ID kısa.");
        }

        var idPrefix = id[5]; // IndexOutOfRangeException riski kalldrıdıl
        
        var hasInstructor = await _unitOfWork.Instructors.GetByIdAsync(id, false);
        //ınstructor false ise
        if (hasInstructor == null)
        {
            return new ErrorDataResult<GetByIdInstructorDto>(null, "Instructor bulunamadı.");
        }
        
        var hasInstructorMapping = _mapper.Map<GetByIdInstructorDto>(hasInstructor);
        // hasInstructorMapping null check
        if (hasInstructorMapping == null)
        {
            return new ErrorDataResult<GetByIdInstructorDto>(null, "Instructor yok.");
        }

        var name = hasInstructorMapping.Name; 
        return new SuccessDataResult<GetByIdInstructorDto>(hasInstructorMapping, ConstantsMessages.InstructorGetByIdSuccessMessage);
    }

    public async Task<IResult> CreateAsync(CreatedInstructorDto entity)
    {
        var createdInstructor = _mapper.Map<Instructor>(entity);
        await _unitOfWork.Instructors.CreateAsync(createdInstructor);
        var result = await _unitOfWork.CommitAsync();
        if(createdInstructor == null) return new ErrorResult("Null");
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.InstructorCreateSuccessMessage);
        }
        return new ErrorResult(ConstantsMessages.InstructorCreateFailedMessage);
    }

    public async Task<IResult> Remove(DeletedInstructorDto entity)
    {
        var deletedInstructor = _mapper.Map<Instructor>(entity);
        _unitOfWork.Instructors.Remove(deletedInstructor);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.InstructorDeleteSuccessMessage);
        }
        return new ErrorResult(ConstantsMessages.InstructorDeleteFailedMessage);
    }

    public async Task<IResult> Update(UpdatedInstructorDto entity)
    {
        // O Null check eklenid
        if (entity == null)
        {
            return new ErrorResult("Güncelleme işlemi için geçersiz.");
        }

        var updatedInstructor = _mapper.Map<Instructor>(entity);

        //updatedInstructor null check
        if (updatedInstructor == null)
        {
            return new ErrorResult("Instructor işlemi başarısız.");
        }
        var instructorName = updatedInstructor.Name; // Null reference riski
        
        _unitOfWork.Instructors.Update(updatedInstructor);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.InstructorUpdateSuccessMessage);
        }
        // ErrorResult ile değiştirildi
        return new ErrorResult(ConstantsMessages.InstructorUpdateFailedMessage); 
    }

    //private void UseNonExistentNamespace()
    //{
     //   var x = NonExistentNamespace.NonExistentClass.Create();
    //}
}
