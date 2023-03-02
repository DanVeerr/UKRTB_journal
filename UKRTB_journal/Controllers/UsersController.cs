using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UKRTB_journal.Models;

namespace UKRTB_journal.Controllers
{
    [Route("users")]
    public class UsersController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        ApplicationContext _context;
        IWebHostEnvironment _appEnvironment;

        public UsersController(ApplicationContext context, ILogger<FilesController> logger, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _logger = logger;
            _appEnvironment = appEnvironment;
        }

        public async Task<IActionResult> UsersView()
        {
            var users = await _context.Users.ToListAsync();

            return View("/Views/Users/Users.cshtml", users);
        }

        [HttpGet("create")]
        [Authorize]
        public async Task<IActionResult> AddUserView()
        {
            return View("/Views/Users/Create.cshtml");
        }

        [HttpGet("edit")]
        [Authorize]
        public async Task<IActionResult> EditUserView(int userId)
        {
            var users = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            return View("/Views/Users/Edit.cshtml", users);
        }

        [HttpGet("delete")]
        [Authorize]
        public async Task<IActionResult> DeleteUserView(int? userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            return View("/Views/Users/Delete.cshtml", user);
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> AddUser(LoginDto userDto)
        {
            var user = new User { Email = userDto.Login, Password = userDto.Password };
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("UsersView");
        }

        [HttpPost("edit/{id}")]
        [Authorize]
        public async Task<IActionResult> EditUser(User userDto)
        {
            _context.Users.Update(userDto);
            await _context.SaveChangesAsync();

            return RedirectToAction("UsersView");
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<IActionResult> Delete(int? userId)
        {
            if (userId != null)
            {
                User user = await _context.Users.FirstOrDefaultAsync(p => p.Id == userId);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("UsersView");
                }
            }

            return NotFound();
        }
    }
}