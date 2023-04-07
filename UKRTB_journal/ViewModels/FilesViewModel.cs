using UKRTB_journal.Models;

namespace UKRTB_journal.ViewModels
{
    public class FilesViewModel
    {
        public int? StudentId { get; set; }
        public int? GroupId { get; set; }
        public int? TypeFileName { get; set; } 
        public IEnumerable<FileDescription> Files { get; set; }
        public IEnumerable<StudentModel> Students { get; set; }
        public IEnumerable<GroupModel> Groups { get; set; }
    }
}
