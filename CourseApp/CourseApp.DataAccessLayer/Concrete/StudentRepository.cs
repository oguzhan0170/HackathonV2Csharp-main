using CourseApp.DataAccessLayer.Abstract;
using CourseApp.DataAccessLayer.Context;
using CourseApp.EntityLayer.Entity;

namespace CourseApp.DataAccessLayer.Concrete;

public class StudentRepository : GenericRepository<Student>, IStudentRepository
{
    public StudentRepository(AppDbContext context) : base(context)
    {
    }
}
