using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using UKRTB_journal.Models;

namespace UKRTB_journal.Controllers
{
    [Route("groups")]
    public class GroupsController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        ApplicationContext _context;
        IWebHostEnvironment _appEnvironment;

        public GroupsController(ApplicationContext context, ILogger<FilesController> logger, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _logger = logger;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> GroupsView()
        {
            var groups = await _context.Groups.ToListAsync();

            return View("/Views/Groups/Groups.cshtml", groups);
        }

        [HttpGet("create")]
        [Authorize]
        public async Task<IActionResult> AddGroupView()
        {
            return View("/Views/Groups/Create.cshtml");
        }

        [HttpGet("edit")]
        [Authorize]
        public async Task<IActionResult> EditGroupView(int groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);

            return View("/Views/Groups/Edit.cshtml", group);
        }

        [HttpGet("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteGroupView(int? groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);

            return View("/Views/Groups/Delete.cshtml", group);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> AddGroup(Group groupDto)
        {
            _context.Groups.Add(groupDto);
            _context.SaveChanges();

            return RedirectToAction("GroupsView");
        }

        [HttpPost("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> EditGroup(Group groupDto)
        {
            _context.Groups.Update(groupDto);
            await _context.SaveChangesAsync();

            return RedirectToAction("GroupsView");
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> Delete(int? groupId)
        {
            if (groupId != null)
            {
                Group group = await _context.Groups.FirstOrDefaultAsync(p => p.Id == groupId);
                if (group != null)
                {
                    _context.Groups.Remove(group);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("GroupsView");
                }
            }

            return NotFound();
        }
    }
}