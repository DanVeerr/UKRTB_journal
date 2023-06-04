using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using UKRTB_journal.Models;
using UKRTB_journal.Services;
using Microsoft.AspNetCore.Authorization;

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

        [HttpGet("metodfiles/upload")]
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMetodFileView()
        {
            var files = await _context.MetodFiles.ToListAsync();

            return View("/Views/MetodFiles/Delete.cshtml", files);
        }

        [HttpPost("metodfiles/uploadfile")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UploadMetodFile(MetodFileWithInfo fileDto)
        {
            if (fileDto != null)
            {
                var sameFile = await _context.MetodFiles
                    .FirstOrDefaultAsync(x =>
                        x.Type == fileDto.MetodFileDescription.Type &&
                        x.PublicName == fileDto.MetodFileDescription.PublicName
                    );

                if (sameFile != null)
                {
                    var fileInfo = new FileInfo($"{_appEnvironment.WebRootPath}" + sameFile.FullPath);
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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteMetodFile(int id)
        {
            if (id != null)
            {
                MetodFileDescription file = await _context.MetodFiles.FirstOrDefaultAsync(p => p.Id == id);
                if (file != null)
                {
                    _context.MetodFiles.Remove(file);
                    await _context.SaveChangesAsync();

                    var fileInfo = new FileInfo($"{_appEnvironment.WebRootPath}" + file.FullPath);
                    if (fileInfo != null)
                    {
                        fileInfo.Delete();
                    }

                    return RedirectToAction("MetodFilesView");
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