using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseApp.EntityLayer.Dto.ExamDto
{
    public class GetAllExamDetailDto
    {
        public string Id { get; set; }
        public string ExamName { get; set; }
        public DateTime ExamDate { get; set; }
        public string? CourseName { get; set; }
        public string? StudentName { get; set; }
        public decimal? Grade { get; set; }
    }
}
