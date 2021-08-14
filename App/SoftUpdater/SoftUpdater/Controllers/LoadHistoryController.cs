//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoftUpdater.Common;
using SoftUpdater.Contract.Model;
using SoftUpdater.Service;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoftUpdater.Controllers
{
    /// <summary>
    /// Контроллер истории загрузок обновлений
    /// </summary>
    public class LoadHistoryController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly IErrorNotifyService _errorNotifyService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="serviceProvider"></param>
        public LoadHistoryController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<LoadHistoryController>>();
            _errorNotifyService = _serviceProvider.GetRequiredService<IErrorNotifyService>();
        }

        /// <summary>
        /// Список истории загрузок обновлений
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Список истории загрузок обновлений
        /// </summary>
        /// <param name="page">page number from 0</param>
        /// <param name="size">page size</param>
        /// <param name="sort">sorting</param>        
        /// <returns></returns>
        [Authorize]
        public async Task<ActionResult> ListPaged(int page = 0, int size = 10, string sort = null)
        {
            try
            {
                var userId = User.Identity.Name;
                var _dataService = _serviceProvider.GetRequiredService<IGetDataService<LoadHistory, LoadHistoryFilter>>();
                CancellationTokenSource source = new CancellationTokenSource(30000);
                var result = await _dataService.GetAsync(new LoadHistoryFilter(size, page, sort), source.Token);
                Response.Headers.Add("x-pages", result.PageCount.ToString());
                return PartialView(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в методе получения истории загрузок: {ex.Message} \r\n {ex.StackTrace} ");
                await _errorNotifyService.Send($"Ошибка в методе LoadHistoryController::ListPaged: {ex.Message} {ex.StackTrace}");
                return RedirectToAction("Index", "Error", new { ex.Message });
            }
        }
    }
}
