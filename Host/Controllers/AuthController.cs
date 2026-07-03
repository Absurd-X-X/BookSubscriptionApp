using Application.ViewModels;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Application.Command.AddReader;
using static Application.Command.ForgotPassword;
using static Application.Commands.Login;
using static Application.Commands.ResendVerification;
using static Application.Commands.ResetPassword;
using static Application.Commands.VerifyEmail;

namespace Host.Controllers
{
    public class AuthController(IMediator mediator) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(AddReaderCommand command)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message =
                    string.Join(",", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                });

            var result = await mediator.Send(command);

            if (!result.Status)
            {
                TempData["Error"] = result.Message; 
                    return Json(new
                    {
                        success = false,
                        message = result.Message
                    });
            }

            TempData["Success"] = result.Message;

            return Json(new
            {
                success = false,
                message = result.Message,
                redirectUrl = Url.Action(
                            nameof(VerifyEmail), new { email = result.Data!.Email })
            });
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(string email, string token)
        {
            var result = await mediator.Send(
                new VerifyEmailCommand(email, token));

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                ViewBag.Email = email;
                return View();
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> ResendVerification(ResendVerificationData data)
        {
            var result = await mediator.Send(new ResendVerificationCommand(data.Email));

            TempData[result.Status ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(VerifyEmail), new { email = data.Email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendVerification(string email)
        {
            var result = await mediator.Send(new ResendVerificationCommand(email));

            TempData[result.Status ? "Success" : "Error"] = result.Message;
            return RedirectToAction(nameof(VerifyEmail), new { email });
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity is { IsAuthenticated: true })
            {
                if (User.IsInRole("admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (User.IsInRole("library"))
                {
                    return RedirectToAction("LibraryDashboard", "Library");
                }

                if (User.IsInRole("reader"))
                {
                    return RedirectToAction("ReaderDashboard", "Reader");
                }

                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            if (!ModelState.IsValid) 
                return Json(new { success = false, message = 
                    string.Join("," , ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))});

            var result = await mediator.Send(command);

            if (!result.Status)
            {
                const string notVerifiedMessage = "you must be verified before logging in.";

                if (result.Message.Equals(notVerifiedMessage, StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new
                    {
                        success = false,
                        message = result.Message,
                        redirectUrl = Url.Action(nameof(ResendVerification), new { Email = command.Email })
                    });
                }

                return Json(new { success = false, message = result.Message });
            }


            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, result.Data!.UserId.ToString()),
                new("CustomerId", result.Data.CustomerId.ToString()),
                new(ClaimTypes.Name, result.Data.FullName),
                new(ClaimTypes.Email, result.Data.Email),
                new(ClaimTypes.Role, result.Data.Role)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = command.RememberMe,
                    ExpiresUtc = command.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(7)
                        : DateTimeOffset.UtcNow.AddHours(1)
                });

            string redirectUrl = result.Data.Role.ToLower() switch
            {
                "admin" => Url.Action("Index", "Admin")!,
                "library" => Url.Action("LibraryDashboard", "Library")!,
                "reader" => Url.Action("ReaderDashboard", "Reader")!,
                _ => Url.Action("Index", "Home")!
            };

            return Json(new { success = true, redirectUrl });
        }


        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordCommand command)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message =
                    string.Join(",", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)))
                });

            var result = await mediator.Send(command);
            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                return Json(new { success = false, message = result.Message });
            }

            TempData["Success"] = result.Message; 
            
            return Json(new
            {
                success = true,
                message = result.Message,
                redirectUrl = Url.Action(nameof(ResendVerification), new { Email = command.Email })
            });
        }


        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordCommand command)
        {
            if (!ModelState.IsValid) return View(command);

            var result = await mediator.Send(command);

            if (!result.Status)
            {
                TempData["Error"] = result.Message;
                ViewBag.Email = command.Email;
                return View(command);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Login));
        }

        // ACCESS DENIED

        public IActionResult AccessDenied() => View();

        // LOGOUT

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}