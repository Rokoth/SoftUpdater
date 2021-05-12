﻿//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SoftUpdater.Contract.Model;
using SoftUpdater.Service;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Controllers
{
    /// <summary>
    /// Client controller
    /// </summary>
    public class ClientController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ClientController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // GET: ClientController
        /// <summary>
        /// List method
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// paged partial view
        /// </summary>
        /// <param name="page">page number from 0</param>
        /// <param name="size">page size</param>
        /// <param name="sort">sorting</param>
        /// <param name="name">name filter</param>
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> ListPaged(int page = 0, int size = 10, string sort = null, string name = null)
        {
            try
            {
                var userId = User.Identity.Name;
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Client, ClientFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new ClientFilter(size, page, sort, name, Guid.Parse(userId)), source.Token);
                Response.Headers.Add("x-pages", result.AllCount.ToString());
                return PartialView(result.Data);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { ex.Message });
            }
        }

        // GET: ClientController/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Client, ClientFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                Client result = await _dataService.GetAsync(id, source.Token);
                var updater = new ClientUpdater()
                {
                    Description = result.Description,
                    Id = result.Id,
                    Login = result.Login,
                    Name = result.Name,
                    BasePath = result.BasePath,                    
                    PasswordChanged = false
                };
                return View(updater);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // POST: ClientController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Edit(Guid id, ClientUpdater updater)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IUpdateDataService<Client, ClientUpdater>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                Client result = await _dataService.UpdateAsync(updater, source.Token);
                return RedirectToAction("Details", new { id = result.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
    }
}
