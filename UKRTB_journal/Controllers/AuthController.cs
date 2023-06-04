using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using UKRTB_journal.Models;

namespace UKRTB_journal.Controllers
{
    public class AuthController : Controller
    {
        ApplicationContext _context;

        public AuthController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet("Login")]
        public IActionResult LoginView([FromQuery] string login, [FromQuery] string? errorMessageType)
        {
            ViewBag.ErrorMessageType = errorMessageType;
            ViewBag.ErrorMessage = errorMessageType is null || errorMessageType == "0" ? 
                null :
                errorMessageType == "1" ? "Пользователь с таким email не найден" : "Неправильный пароль или логин";

            return View("/Views/Auth/Login.cshtml");
        }

        [HttpPost("auth/login")]
        public async Task<IActionResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid) 
                {
                    return RedirectToAction(
                        actionName: "LoginView",
                        controllerName: "Auth",
                        new { login = loginDto.Login, errorMessageType = 0 }
                    );
                }
                var user = _context.Users.FirstOrDefault(x => x.Email.ToLower().Trim() == loginDto.Login.ToLower().Trim());
                if (user == null)
                {
                    return RedirectToAction(
                        actionName: "LoginView",
                        controllerName: "Auth", 
                        new { login = loginDto.Login, errorMessageType = 1 }
                   );
                }

                if (user.Password.Trim() != loginDto?.Password?.Trim())
                {
                    return RedirectToAction(
                        actionName: "LoginView",
                        controllerName: "Auth",
                        new { login = loginDto.Login, errorMessageType = 2 }
                    );
                }

                var claims = new List<Claim>
                {
                    new (ClaimsIdentity.DefaultNameClaimType, loginDto.Login),
                    new (ClaimsIdentity.DefaultRoleClaimType, user.IsAdmin ? "admin" : "student")
                };
                // создаем объект ClaimsIdentity
                ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
                // установка аутентификационных куки
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));

                return RedirectToAction(actionName: "FilesView", controllerName: "Files");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return RedirectToAction(actionName: "LoginView", controllerName: "Auth", new LoginDto { Login = loginDto.Login });
            }

        }
    }
}
