using UKRTB_journal.Models;
using Microsoft.EntityFrameworkCore;

namespace UKRTB_journal.Services
{
    public class InfoService : IInfoService
    {
        private readonly ILogger<InfoService> _logger;
        ApplicationContext _context;
        public InfoService(ApplicationContext context, ILogger<InfoService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string GetStudentName(int studentId)
        {
            var student = _context.Students.FirstOrDefault(x => x.Id == studentId);

            return student?.Name ?? "Неизвестный";
        }

        public string GetStudentSurName(int studentId)
        {
            var student = _context.Students.FirstOrDefault(x => x.Id == studentId);

            return student?.Surname ?? "Неизвестный";
        }

        public string GetTypeOfFileName(int typeOfFile) => typeOfFile switch
        {
            1 => "Лабараторная работа",
            2 => "Практическая работа",
            3 => "Конспект",
            4 => "Другое",
            _ => "Неизвестный"
        };

        public string GetGroupName(int groupId)
        {
            var group = _context.Groups.FirstOrDefault(x => x.Id == groupId);

            return group?.Name ?? "Неизвестная";
        }

    }
}
