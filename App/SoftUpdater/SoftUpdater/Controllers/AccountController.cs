//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using SoftUpdater.Contract.Model;
using SoftUpdater.Service;
using SoftUpdater.Common;

namespace SoftUpdater.TaskCollector.Controllers
{
    /// <summary>
    /// Authentification methods
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IErrorNotifyService _errorNotifyService;

        public AccountController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _errorNotifyService = _serviceProvider.GetRequiredService<IErrorNotifyService>();
        }
        
        // GET: AccountController/Login
        /// <summary>
        /// Login page
        /// </summary>
        /// <returns></returns>
        public ActionResult Login(string returnUrl)
        {            
            return View();
        }

        // POST: AccountController/Login
        /// <summary>
        /// Login method
        /// </summary>
        /// <param name="userIdentity">login and password</param>
        /// <param name="returnUrl">url to redirect after authorization</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserIdentity userIdentity, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var source = new CancellationTokenSource(30000);
                    var dataService = _serviceProvider.GetRequiredService<IAuthService>();
                    var identity = await dataService.Auth(userIdentity, source.Token);
                    if (identity == null)
                    {
                        return RedirectToAction("Index", "Error", new { Message = "Неверный логин или пароль" });
                    }
                    // установка аутентификационных куки
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                }
                else
                {
                    return RedirectToAction("Login");
                }
                if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе AccountController::Login: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { ex.Message });
            }
        }

        // POST: AccountController/Logout
        /// <summary>
        /// LogOut method
        /// </summary>
        /// <returns></returns>
        [HttpGet]       
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await _errorNotifyService.Send($"Ошибка в методе AccountController::Logout: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { ex.Message });
            }
        }
    }
}
