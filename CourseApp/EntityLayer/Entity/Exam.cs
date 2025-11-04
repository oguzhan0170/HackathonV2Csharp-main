using System.ComponentModel.DataAnnotations.Schema;

namespace CourseApp.EntityLayer.Entity;

public class Exam:BaseEntity
{
    public string? Name { get; set; }
    public DateTime Date { get; set; }
   
    public Student? Student { get; set; }
    public ICollection<ExamResult>? ExamResults { get; set; }
}
