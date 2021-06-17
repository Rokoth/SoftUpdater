//Copyright 2021 Dmitriy Rokoth
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
                Response.Headers.Add("x-pages", result.PageCount.ToString());
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
                updater.UserId = Guid.Parse(User.Identity.Name);
                Client result = await _dataService.UpdateAsync(updater, source.Token);
                return RedirectToAction("Details", new { id = result.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController
        [Authorize]
        public ActionResult History()
        {
            return View();
        }

        [Authorize]
        public async Task<ActionResult> HistoryListPaged(int page = 0, int size = 10, string sort = null, string name = null, Guid? id = null)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<ClientHistory, ClientHistoryFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new ClientHistoryFilter(size, page, sort, name, id), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Details/5
        [Authorize]
        public async Task<ActionResult> Details([FromRoute]Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Client, ClientFilter>>();
                var cancellationTokenSource = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(id, cancellationTokenSource.Token);
                return View(result);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Details/5
        [Authorize]
        public async Task<ActionResult> Simple([FromRoute] Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Client, ClientFilter>>();
                var cancellationTokenSource = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(id, cancellationTokenSource.Token);
                return PartialView(result);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Create
        [Authorize]
        public ActionResult Create()
        {
            //Fill default fields
            var user = new ClientCreator();
            return View(user);
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Create(ClientCreator creator)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IAddDataService<Client, ClientCreator>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                creator.UserId = Guid.Parse(User.Identity.Name);
                Client result = await _dataService.AddAsync(creator, source.Token);
                return RedirectToAction(nameof(Details), new { id = result.Id });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // GET: UserController/Delete/5
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Client, ClientFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                Client result = await _dataService.GetAsync(id, source.Token);
                return View(result);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id, Client model)
        {
            try
            {
                var _dataService = _serviceProvider.GetRequiredService<IDeleteDataService<Client>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                Client result = await _dataService.DeleteAsync(id, source.Token);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }

        public ActionResult ListSelect()
        {
            return PartialView();
        }

        public async Task<IActionResult> ListSelectPaged(string name = null, int page = 0, int size = 10, string sort = null)
        {
            try
            {
                var userId = User.Identity.Name;
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<Client, ClientFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new ClientFilter(size, page, sort, name, Guid.Parse(userId)), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Message = ex.Message });
            }
        }
    }
}
