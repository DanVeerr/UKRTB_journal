using System.ComponentModel;

namespace UKRTB_journal.Models
{
    public class FileWithInfo
    {
        public IFormFile File { get; set; }

        public FileDescription FileDescription { get; set; }
    }
    public class MetodFileWithInfo
    {
        public IFormFile File { get; set; }

        public MetodFileDescription MetodFileDescription { get; set; }
    }

    public class FileDescription
    {
        public int Id { get; set; }

        public string Path { get; set; }
        
        public string? Description { get; set; }

        public DateTime UploadDate { get; set; }

        public DateTime EditDate { get; set; }

        public TypeOfFile Type { get; set; }

        public int FileNumberForType { get; set; }
        
        public int StudentId { get; set; }

        public int GroupId { get; set; }

        public int Version { get; set; }
    }

    public class MetodFileDescription
    {
        public string PublicName { get; set; }

        public int Id { get; set; }

        public string Path { get; set; }

        public string? Description { get; set; }

        public DateTime UploadDate { get; set; }

        public DateTime EditDate { get; set; }

        public TypeOfFile Type { get; set; }

        public int Version { get; set; }
    }

    public enum TypeOfFile
    {
        [Description("Лабараторная работа")]
        LabaratoryWork = 1,

        [Description("Практическая работа")]
        PracticalWork,

        [Description("Конспект")]
        Note,

        [Description("Другое")]
        Other
    }

    public class Student
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }

        public int GroupId { get; set; }

        public string Email { get; set; }
    }

    public class Group
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Year { get; set; }

        public string Speciality { get; set; }
    }
}
