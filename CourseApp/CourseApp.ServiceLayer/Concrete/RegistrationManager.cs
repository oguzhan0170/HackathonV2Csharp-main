using AutoMapper;
using CourseApp.DataAccessLayer.UnitOfWork;
using CourseApp.EntityLayer.Dto.RegistrationDto;
using CourseApp.EntityLayer.Entity;
using CourseApp.ServiceLayer.Abstract;
using CourseApp.ServiceLayer.Utilities.Constants;
using CourseApp.ServiceLayer.Utilities.Result;
using Microsoft.EntityFrameworkCore;

namespace CourseApp.ServiceLayer.Concrete;

public class RegistrationManager : IRegistrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public RegistrationManager(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IDataResult<IEnumerable<GetAllRegistrationDto>>> GetAllAsync(bool track = true)
    {
        var registrationList = await _unitOfWork.Registrations.GetAll(false).ToListAsync();
        var registrationListMapping = _mapper.Map<IEnumerable<GetAllRegistrationDto>>(registrationList);
        if (!registrationList.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllRegistrationDto>>(null, ConstantsMessages.RegistrationListFailedMessage);
        }
        return new SuccessDataResult<IEnumerable<GetAllRegistrationDto>>(registrationListMapping, ConstantsMessages.RegistrationListSuccessMessage);
    }

    public async Task<IDataResult<GetByIdRegistrationDto>> GetByIdAsync(string id, bool track = true)
    {
        var hasRegistration = await _unitOfWork.Registrations.GetByIdAsync(id, false);
        var hasRegistrationMapping = _mapper.Map<GetByIdRegistrationDto>(hasRegistration);
        return new SuccessDataResult<GetByIdRegistrationDto>(hasRegistrationMapping, ConstantsMessages.RegistrationGetByIdSuccessMessage);
    }

    public async Task<IResult> CreateAsync(CreateRegistrationDto entity)
    {
        // entity Null check eklendi
        if (entity == null)
        {
            return new ErrorResult("Kayıt verisi yok.");
        }

        var createdRegistration = _mapper.Map<Registration>(entity);
        // createdRegistration null check
        if (createdRegistration == null)
        {
            return new ErrorResult("Başarısız işlem.");
        }

        //registrationPrice fiyat kontrolü
        if (createdRegistration.Price <= 0)
        {
            return new ErrorResult("Geçersiz fiyat bilgisi.");
        }

        var registrationPrice = createdRegistration.Price;

        // ZOR: Async/await anti-pattern - GetAwaiter().GetResult() deadlock'a sebep olabilir
        //kayıt işlemi için await eklendi,.GetAwaiter().GetResult() kaldırıldı
        await _unitOfWork.Registrations.CreateAsync(createdRegistration); 
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.RegistrationCreateSuccessMessage);
        }

        // KOLAY: Noktalı virgül eksikliği
        return new ErrorResult(ConstantsMessages.RegistrationCreateFailedMessage); // TYPO: ; eksik
    }

    public async Task<IResult> Remove(DeleteRegistrationDto entity)
    {
        var deletedRegistration = _mapper.Map<Registration>(entity);
        _unitOfWork.Registrations.Remove(deletedRegistration);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.RegistrationDeleteSuccessMessage);
        }
        return new ErrorResult(ConstantsMessages.RegistrationDeleteFailedMessage);
    }

    public async Task<IResult> Update(UpdatedRegistrationDto entity)
    {
        // entity Null check eklendi
        if (entity == null)
        {
            return new ErrorResult("Kayıt verisi yok.");
        }

        var updatedRegistration = _mapper.Map<Registration>(entity);
        if (updatedRegistration == null)
        {
            return new ErrorResult("Registration işlemi başarısız oldu.");
        }
        //updatedRegistrationPrice kontorlü
        if (updatedRegistration.Price <= 0)
        {
            return new ErrorResult("Geçersiz fiyat değeri.");
        }

        // ORTA: Tip dönüşüm hatası - decimal'i int'e direkt cast
        //dto da zaten decimal olarak tutuluytor bu yüzden bu dönüşüme gerek yok
        //decimal validPrice = updatedRegistration.Price;

        _unitOfWork.Registrations.Update(updatedRegistration);
        var result = await _unitOfWork.CommitAsync();
        if (result > 0)
        {
            return new SuccessResult(ConstantsMessages.RegistrationUpdateSuccessMessage);
        }
        // ErrorResult ile değiştiirldi
        return new ErrorResult(ConstantsMessages.RegistrationUpdateFailedMessage); 
    }

    public async Task<IDataResult<IEnumerable<GetAllRegistrationDetailDto>>> GetAllRegistrationDetailAsync(bool track = true)
    {
        // Include eklendi, course ve student tek sorguda çekilmeis için
        var registrationData = await _unitOfWork.Registrations.GetAllRegistrationDetail(false).Include(r => r.Course).Include(r => r.Student).ToListAsync();
        
        // ZOR: N+1 - Her registration için Course ve Student ayrı sorgu ile çekiliyor
        // Örnek: registration.Course?.CourseName her iterasyonda DB sorgusu

        if(!registrationData.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllRegistrationDetailDto>>(null,ConstantsMessages.RegistrationListFailedMessage);
        }

        var registrationDataMapping = _mapper.Map<IEnumerable<GetAllRegistrationDetailDto>>(registrationData);
        //registrationDataMapping null check
        if (registrationDataMapping == null || !registrationDataMapping.Any())
        {
            return new ErrorDataResult<IEnumerable<GetAllRegistrationDetailDto>>(null, "Registration mapping işlemi başarısız.");
        }
        var firstRegistration = registrationDataMapping.ToList()[0]; 
        
        return new SuccessDataResult<IEnumerable<GetAllRegistrationDetailDto>>(registrationDataMapping, ConstantsMessages.RegistrationListSuccessMessage);  
    }

    public async Task<IDataResult<GetByIdRegistrationDetailDto>> GetByIdRegistrationDetailAsync(string id, bool track = true)
    {
        throw new NotImplementedException();
    }

    //public void AccessNonExistentProperty()
    //{
    //    var registration = new Registration();
    //    var value = registration.NonExistentProperty;
    //}
}
