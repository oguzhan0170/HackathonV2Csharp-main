using CourseApp.DataAccessLayer.Abstract;
using CourseApp.DataAccessLayer.Context;
using CourseApp.EntityLayer.Entity;

namespace CourseApp.DataAccessLayer.Concrete
{
    public class InstructorRepository : GenericRepository<Instructor>, IInstructorRepository
    {
        public InstructorRepository(AppDbContext context) : base(context)
        {
        }
    }
}
