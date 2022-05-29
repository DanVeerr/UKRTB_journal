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
    public class MetodFilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        ApplicationContext _context;
        IWebHostEnvironment _appEnvironment;
        IConfiguration Configuration { get; }
        private EmailSettings _emailSettings;
        IInfoService infoService;

        public MetodFilesController(ApplicationContext context, ILogger<FilesController> logger, IWebHostEnvironment appEnvironment, IConfiguration configuration, IInfoService infoService)
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

        [HttpGet("metodfiles/upload")]
        public IActionResult UploadMetodFileView()
        {
            var students = _context.Students.ToList();
            var groups = _context.Groups.ToList();

            return View("/Views/MetodFiles/Create.cshtml");
        }

        [HttpGet]
        public IActionResult MetodFilesView(int? groupId, int? studentId)
        {
            var files = _context.Files.ToList();

            return View("/Views/MetodFiles/MetodFiles.cshtml");
        }

        [HttpGet("metodfiles/edit")]
        public async Task<IActionResult> EditMetodFilesView(int fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);

            return View("/Views/MetodFiles/Edit.cshtml", new FileWithInfo { FileDescription = file });
        }

        [HttpGet("metodfiles/delete")]
        public async Task<IActionResult> DeleteMetodFileView(int? fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);

            return View("/Views/MetodFiles/Delete.cshtml", file);
        }

        [HttpPost("metodfiles/upload")]
        public async Task<IActionResult> UploadMetodFile(FileWithInfo fileDto)
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
                var path = "/metodFiles/" +
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
            }

            return RedirectToAction("FilesView");
        }

        [HttpPost("metodfiles/edit")]
        public async Task<IActionResult> EditMetodFile(FileWithInfo fileDto)
        {
            if (fileDto != null)
            {
                if (fileDto.File != null)
                {
                    var path = "/metodFiles/" +
                        $"{fileDto.FileDescription.Id}" +
                        $"{fileDto.FileDescription.Version}" +
                        fileDto.File.Name.Trim().ToLower()[..fileDto.File.FileName.IndexOf('.')];

                    var fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        fileInfo.Delete();
                    }

                    fileDto.FileDescription.Version++;

                    path = "/metodFiles/" +
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

        [HttpPost("metodfiles/delete")]
        public async Task<IActionResult> DeleteMetodFile(int? fileId)
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Code = code });
        }
    }
}