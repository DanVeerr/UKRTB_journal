using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UKRTB_journal.Models;

namespace UKRTB_journal.Controllers
{
    [Route("groups")]
    public class GroupsController : Controller
    {
        ApplicationContext _context;

        public GroupsController(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GroupsView()
        {
            var groups = await _context.Groups.ToListAsync();

            return View("/Views/Groups/Groups.cshtml", groups);
        }

        [HttpGet("create")]
        public async Task<IActionResult> AddGroupView()
        {
            return View("/Views/Groups/Create.cshtml");
        }

        [HttpGet("edit")]
        public async Task<IActionResult> EditGroupView(int groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);

            return View("/Views/Groups/Edit.cshtml", group);
        }

        [HttpGet("delete")]
        public async Task<IActionResult> DeleteGroupView(int? groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);

            return View("/Views/Groups/Delete.cshtml", group);
        }

        [HttpPost("create")]
        public async Task<IActionResult> AddGroup(Group groupDto)
        {
            _context.Groups.Add(groupDto);
            _context.SaveChanges();

            return RedirectToAction("GroupsView");
        }

        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditGroup(Group groupDto)
        {
            _context.Groups.Update(groupDto);
            await _context.SaveChangesAsync();

            return RedirectToAction("GroupsView");
        }

        [HttpPost("delete")]
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