using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UKRTB_journal.Models;
using UKRTB_journal.ViewModels;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net;
using Distinct.Telemedicine.Contracts.Models;
using Microsoft.AspNetCore.Authorization;
using UKRTB_journal.Services;

namespace UKRTB_journal.Controllers
{
    public class FilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        ApplicationContext _context;
        IWebHostEnvironment _appEnvironment;
        IConfiguration Configuration { get; }
        private EmailSettings _emailSettings;
        IInfoService infoService;
        bool isSaveFiles;
        public FilesController(ApplicationContext context, ILogger<FilesController> logger, IWebHostEnvironment appEnvironment, IConfiguration configuration, IInfoService infoService)
        {
            _context = context;
            _logger = logger;
            _appEnvironment = appEnvironment;
            Configuration = configuration;
            this.infoService = infoService;
            Configuration.GetValue<string>("isSaveFiles");
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [HttpGet("files/upload")]
        [Authorize]
        public IActionResult UploadFileView(int? groupId)
        {
            var students = _context.Students.Where(x => groupId == null || x.GroupId == groupId).ToList();
            ViewBag.Groups = _context.Groups.Select(x => new GroupModel { Id = x.Id, Name = x.Name }).ToList();

            ViewBag.Students = new SelectList(students, "Id", "Surname");

            return View("/Views/Files/Create.cshtml");
        }

        [HttpGet]
        public IActionResult FilesView(int? groupId, int? studentId)
        {
            var files = _context.Files
                .Where(x => studentId == null || x.StudentId == studentId)
                .Where(x => groupId == null || x.GroupId == groupId).ToList();
            var student = _context.Students.FirstOrDefault(x => x.Id == studentId);
            var students = new List<StudentModel>();
            if (student is null)
            {
                students = _context.Students
                    .Where(x => groupId == null || x.GroupId == groupId)
                 .Select(x => new StudentModel { Id = x.Id, Surname = x.Surname, Name = x.Name, GroupId = x.GroupId }).ToList();
            }
            else
            {
                students = _context.Students
                    .Where(x => x.GroupId == groupId || (student != null & x.GroupId == student.GroupId))
                    .Select(x => new StudentModel { Id = x.Id, Surname = x.Surname, Name = x.Name, GroupId = x.GroupId }).ToList();
            }
            
            var groups = _context.Groups.Select(x => new GroupModel { Id = x.Id, Name = x.Name }).ToList();

            return View("/Views/Files/Files.cshtml", new FilesViewModel { GroupId = groupId, StudentId = studentId, Files = files, Students = students, Groups = groups });
        }

        [HttpGet("files/edit")]
        [Authorize]
        public async Task<IActionResult> EditFilesView(int fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);
            ViewBag.Students = _context.Students.Select(x => new StudentModel { Id = x.Id, Surname = x.Surname, Name = x.Name, GroupId = x.GroupId }).ToList();
            ViewBag.Groups = _context.Groups.Select(x => new GroupModel { Id = x.Id, Name = x.Name }).ToList();
            return View("/Views/Files/Edit.cshtml", new FileWithInfo { FileDescription = file });
        }

        [HttpGet("files/mark")]
        [Authorize]
        public async Task<IActionResult> MarkFilesView(int fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);

            return View("/Views/Files/Mark.cshtml", new FileWithInfo { FileDescription = file });
        }

        [HttpGet("files/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteFileView(int? fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);

            return View("/Views/Files/Delete.cshtml", file);
        }

        [HttpPost("files/upload")]
        [Authorize]
        public async Task<IActionResult> UploadFile(FileWithInfo fileDto)
        {
            if (fileDto != null)
            {
                var sameFile = await _context.Files.FirstOrDefaultAsync(x =>
                        x.FileNumberForType == fileDto.FileDescription.FileNumberForType &&
                        x.Type == fileDto.FileDescription.Type &&
                        x.StudentId == fileDto.FileDescription.StudentId
                    );
                if (sameFile != null)
                {
                    fileDto.FileDescription.Version = sameFile.Version + 1;
                }
                else
                {
                    fileDto.FileDescription.Version = 1;
                }
                
                fileDto.FileDescription.GroupId = _context.Students.FirstOrDefault(x => x.Id == fileDto.FileDescription.StudentId)?.GroupId ?? 1;

                var path = GetPath(fileDto);
                fileDto.FileDescription.Path = path;
                fileDto.FileDescription.UploadDate = DateTime.Now;

                _context.Files.Add(fileDto.FileDescription);
                
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await fileDto.File.CopyToAsync(fileStream);
                }

                await _context.SaveChangesAsync();
                
                Send(fileDto);
                if (!isSaveFiles)
                {
                    var fileInfo = new FileInfo(_appEnvironment.WebRootPath + fileDto.FileDescription.Path);
                    if (fileInfo != null)
                    {
                        fileInfo.Delete();
                    }
                }
            }

            return RedirectToAction("FilesView");
        }

        [HttpPost("files/edit/{filedescription.id}")]
        [Authorize]
        public async Task<IActionResult> EditFile(FileWithInfo fileDto)
        {
            if (fileDto != null)
            {
                FileDescription file = await _context.Files.FirstOrDefaultAsync(p => p.Id == fileDto.FileDescription.Id);
                file.Version++;

                file.StudentId = fileDto.FileDescription.StudentId;
                file.GroupId = fileDto.FileDescription.GroupId;
                file.FileNumberForType = fileDto.FileDescription.FileNumberForType;
                file.Type = fileDto.FileDescription.Type;
                file.EditDate = DateTime.Now;

                _context.Files.Update(file);
                _context.SaveChanges();
            }

            return RedirectToAction("FilesView");
        }

        [HttpPost("files/mark/{filedescription.id}")]
        [Authorize]
        public async Task<IActionResult> MarkFile(FileWithInfo fileDto)
        {
            if (fileDto != null)
            {
                FileDescription file = await _context.Files.FirstOrDefaultAsync(p => p.Id == fileDto.FileDescription.Id);
                file.Mark = fileDto.FileDescription.Mark;

                _context.Files.Update(file);
                _context.SaveChanges();
            }

            return RedirectToAction("FilesView");
        }

        [HttpPost("files/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteFile(int? fileId)
        {
            if (fileId != null)
            {
                FileDescription file = await _context.Files.FirstOrDefaultAsync(p => p.Id == fileId);
                if (file != null)
                {
                    _context.Files.Remove(file);
                    await _context.SaveChangesAsync();

                    var fileInfo = new FileInfo(_appEnvironment.WebRootPath + file.Path);
                    if (fileInfo != null)
                    {
                        fileInfo.Delete();
                    }

                    return RedirectToAction("FilesView");
                }
            }

            return NotFound();
        }

        public async void Send(FileWithInfo file)
        {
            _emailSettings = new EmailSettings()
            {
                Host = Configuration.GetValue<string>("smtpHost"),
                Port = Configuration.GetValue<int>("smtpPort"),
                Username = Configuration.GetValue<string>("smtpUsername"),
                Password = Configuration.GetValue<string>("smtpPassword"),
                From = Configuration.GetValue<string>("smtpFrom"),
                Sender = Configuration.GetValue<string>("SystemMainName"),
                Email = Configuration.GetValue<string>("smtpEmail")
            };
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailSettings.Sender, _emailSettings.From));
            emailMessage.To.Add(new MailboxAddress("", _emailSettings.Email));
            emailMessage.Subject = $"{infoService.GetStudentSurName(file.FileDescription.StudentId)} {infoService.GetGroupName(file.FileDescription.GroupId)} {file.FileDescription.Type} {file.FileDescription.FileNumberForType}";
            var builder = new BodyBuilder();


            builder.TextBody = $"{file.FileDescription.Description}";

            // We may also want to attach a calendar event for Monica party...
            builder.Attachments.Add(_appEnvironment.WebRootPath + file.FileDescription.Path);

            // Now we just need to set the message body and we're done
            emailMessage.Body = builder.ToMessageBody();

            try
            {
                using var _smtpClient = new SmtpClient();

                _smtpClient.AuthenticationMechanisms.Clear();
                _smtpClient.AuthenticationMechanisms.Add("NTLM");

                _smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await _smtpClient.ConnectAsync(
                    _emailSettings.Host,
                    _emailSettings.Port,
                    SecureSocketOptions.Auto
                );
                await _smtpClient.AuthenticateAsync(new NetworkCredential(_emailSettings.Username,
                    _emailSettings.Password));
                await _smtpClient.SendAsync(emailMessage);
                await _smtpClient.DisconnectAsync(true);
            }
            catch (Exception e)
            {
                var errorMessage = "Произошла ошибка при отправке письма";
            }
        }

        public string GetPath(FileWithInfo fileDto)
        {
            return "/files/" +
                    $"{infoService.GetGroupName(fileDto.FileDescription.GroupId)}-" +
                    $"{infoService.GetStudentSurName(fileDto.FileDescription.StudentId)}-" +
                    $"{fileDto.FileDescription.Type}-" +
                    $"{fileDto.FileDescription.FileNumberForType}-" +
                    $"{fileDto.FileDescription.Version}-" +
                    fileDto.File.FileName.Substring(fileDto.File.FileName.LastIndexOf('.'));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Code = code });
        }
    }
}