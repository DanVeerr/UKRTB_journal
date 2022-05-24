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

        public FilesController(ApplicationContext context, ILogger<FilesController> logger, IWebHostEnvironment appEnvironment, IConfiguration configuration, IInfoService infoService)
        {
            _context = context;
            _logger = logger;
            _appEnvironment = appEnvironment;
            Configuration = configuration;
            this.infoService = infoService;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet("files/upload")]
        public IActionResult UploadFileView()
        {
            var students = _context.Students.ToList();
            var groups = _context.Groups.ToList();

            ViewBag.Students = new SelectList(students, "Id", "Surname");
            ViewBag.Groups = new SelectList(groups, "Id", "Name");

            return View("/Views/Files/Create.cshtml");
        }

        [HttpGet]
        public IActionResult FilesView(int? groupId, int? studentId)
        {
            var files = _context.Files
                .Where(x => studentId == null || x.StudentId == studentId)
                .Where(x => groupId == null || x.GroupId == groupId).ToList();
            var students = _context.Students.Select(x => new StudentModel { Id = x.Id, Surname = x.Surname, Name = x.Name }).ToList();
            var groups = _context.Groups.Select(x => new GroupModel { Id = x.Id, Name = x.Name }).ToList();

            return View("/Views/Files/Files.cshtml", new FilesViewModel { Files = files, Students = students, Groups = groups });
        }

        [HttpGet("files/edit")]
        public async Task<IActionResult> EditFilesView(int fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);

            return View("/Views/Files/Edit.cshtml", new FileWithInfo { FileDescription = file });
        }

        [HttpGet("files/delete")]
        public async Task<IActionResult> DeleteFileView(int? fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);

            return View("/Views/Files/Delete.cshtml", file);
        }

        [HttpPost("files/upload")]
        public async Task<IActionResult> UploadFile(FileWithInfo fileDto)
        {
            if (fileDto != null)
            {
                var sameFile = await _context.Files
                    .FirstOrDefaultAsync(x =>
                        x.FileNumberForType == fileDto.FileDescription.FileNumberForType &&
                        x.Type == fileDto.FileDescription.Type &&
                        x.StudentId == fileDto.FileDescription.StudentId
                    );
                if (sameFile != null)
                {
                    fileDto.FileDescription.Version = sameFile.Version;
                }
                else
                {
                    fileDto.FileDescription.Version = 1;
                }

                var fileInDb = _context.Files.Add(fileDto.FileDescription);
                var path = "/files/" +
                    $"{fileInDb.Entity.Id}-" +
                    $"{fileDto.FileDescription.Version}-" +
                    fileDto.File.FileName.Substring(fileDto.File.FileName.LastIndexOf('.'));

                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await fileDto.File.CopyToAsync(fileStream);
                }

                fileInDb.Entity.Path = path;
                fileInDb.Entity.UploadDate = DateTime.Now;

                await _context.SaveChangesAsync();
                fileDto.FileDescription.Path = path;
                fileDto.FileDescription.UploadDate = DateTime.Now;
                Send(fileDto);
            }

            return RedirectToAction("FilesView");
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditFile(FileWithInfo fileDto)
        {
            if (fileDto != null)
            {
                if (fileDto.File != null)
                {
                    var path = "/files/" +
                        $"{fileDto.FileDescription.Id}" +
                        $"{fileDto.FileDescription.Version}" +
                        fileDto.File.Name.Trim().ToLower()[..fileDto.File.FileName.IndexOf('.')];

                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        fileInfo.Delete();
                    }

                    fileDto.FileDescription.Version++;

                    path = "/files/" +
                        $"{fileDto.FileDescription.Id}" +
                        $"{fileDto.FileDescription.Version}" +
                        fileDto.File.Name.Trim().ToLower()[..fileDto.File.FileName.IndexOf('.')];

                    using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                    {
                        await fileDto.File.CopyToAsync(fileStream);
                    }

                    fileDto.FileDescription.Path = path;
                }
                else
                {
                    fileDto.FileDescription.Version++;
                }


                fileDto.FileDescription.EditDate = DateTime.Now;

                _context.Files.Update(fileDto.FileDescription);
                _context.SaveChanges();
            }

            return RedirectToAction("FilesView");
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteFile(int? fileId)
        {
            if (fileId != null)
            {
                FileDescription file = await _context.Files.FirstOrDefaultAsync(p => p.Id == fileId);
                if (file != null)
                {
                    _context.Files.Remove(file);
                    await _context.SaveChangesAsync();

                    var fileInfo = new FileInfo(file.Path);
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


            builder.TextBody = "Файл";

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

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Code = code });
        }
    }
}