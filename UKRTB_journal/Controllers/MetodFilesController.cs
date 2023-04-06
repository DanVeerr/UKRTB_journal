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
            return View("/Views/MetodFiles/Create.cshtml");
        }

        [HttpGet]
        public IActionResult MetodFilesView()
        {
            var files = _context.MetodFiles.ToList();
            
            return View("/Views/MetodFiles/MetodFiles.cshtml", files);
        }

        [HttpGet("metodfiles/delete")]
        public async Task<IActionResult> DeleteMetodFileView(int? fileId)
        {
            var file = await _context.Files.FirstOrDefaultAsync(x => x.Id == fileId);

            return View("/Views/MetodFiles/Delete.cshtml", file);
        }

        [HttpPost("metodfiles/uploadfile")]
        public async Task<IActionResult> UploadMetodFile(MetodFileWithInfo fileDto)
        {
            if (fileDto != null)
            {
                var sameFile = await _context.MetodFiles
                    .FirstOrDefaultAsync(x =>
                        x.Type == fileDto.MetodFileDescription.Type
                    );

                if (sameFile != null)
                {
                    var fileInfo = new FileInfo(sameFile.FullPath);
                    if (fileInfo != null)
                    {
                        fileInfo.Delete();
                    }
                }
                
                var path = GetPath(fileDto);

                using (var fileStream = new FileStream($"{_appEnvironment.WebRootPath}" + path, FileMode.Create))
                {
                    await fileDto.File.CopyToAsync(fileStream);
                }

                fileDto.MetodFileDescription.FullPath = path;
                _context.MetodFiles.Add(fileDto.MetodFileDescription);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MetodFilesView");
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

        public string GetPath(MetodFileWithInfo fileDto)
        {
            return "/metodFiles/" +
                    $"{fileDto.MetodFileDescription.Type}-" +
                    $"{fileDto.MetodFileDescription.PublicName}-" +
                    fileDto.File.FileName.Substring(fileDto.File.FileName.LastIndexOf('.'));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? code)
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier, Code = code });
        }
    }
}