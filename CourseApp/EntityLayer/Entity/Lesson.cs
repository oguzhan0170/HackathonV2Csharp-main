namespace CourseApp.EntityLayer.Entity;

public class Lesson : BaseEntity
{
    public string? Name { get; set; } // tile yerine NAme olarak değiştirdm
    public DateTime Date { get; set; }
    public byte Duration { get; set; }
    public string? Content { get; set; }
    public string? CourseID { get; set; }
    public string? Time { get; set; }

    //navigation property
    public Course? Course { get; set; }
}
