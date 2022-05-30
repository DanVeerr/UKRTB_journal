using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UKRTB_journal.Models;
using UKRTB_journal.ViewModels;

namespace UKRTB_journal.Controllers
{
    [Route("students")]
    public class StudentsController : Controller
    {
        private readonly ILogger<StudentsController> _logger;
        ApplicationContext _context;
        IWebHostEnvironment _appEnvironment;

        public StudentsController(ApplicationContext context, ILogger<StudentsController> logger, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _logger = logger;
            _appEnvironment = appEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> StudentsView(int? groupId)
        {
            List<Student> students;
            if(groupId != null)
            {
                students = _context.Students.Where(x => x.GroupId == groupId).ToList();
            }
            else
            {
                students = _context.Students.ToList();
            }
            ViewBag.Groups = _context.Groups.Select(x => new GroupModel { Id = x.Id, Name = x.Name}).ToList();


            return View("/Views/Students/Students.cshtml", students);
        }

        [HttpGet("create")]
        public async Task<IActionResult> AddStudentView()
        {
            var groups = await _context.Groups.ToListAsync();
            ViewBag.Groups = new SelectList(groups, "Id", "Name");

            return View("/Views/Students/Create.cshtml");
        }

        [HttpGet("edit")]
        public async Task<IActionResult> EditStudentView(int studentId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId);

            ViewBag.Groups = _context.Groups.Select(x => new GroupModel { Id = x.Id, Name = x.Name }).ToList();

            return View("/Views/Students/Edit.cshtml", student);
        }

        [HttpGet("delete")]
        public async Task<IActionResult> DeleteStudentView(int? studentId)
        {
            var student = await _context.Students.FirstOrDefaultAsync(x => x.Id == studentId);

            return View("/Views/Students/Delete.cshtml", student);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddStudent(Student studentDto)
        {
            if(studentDto != null)
            {
                _context.Students.Add(studentDto);
                _context.SaveChanges();
            }
            
            return RedirectToAction("StudentsView");
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditStudent(Student studentDto)
        {
            _context.Students.Update(studentDto);
            await _context.SaveChangesAsync();

            return RedirectToAction("StudentsView");
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteStudent(int? studentId)
        {
            if (studentId != null)
            {
                Student student = await _context.Students.FirstOrDefaultAsync(p => p.Id == studentId);
                if (student != null)
                {
                    _context.Students.Remove(student);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("StudentsView");
                }
            }

            return NotFound();
        }
    }
}