using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SoftUpdater.Contract.Model;
using SoftUpdater.Models;

namespace SoftUpdater.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index([FromQuery] string message, [FromQuery] string source = null)
        {
            return View(new ErrorMessage()
            {
                Message = message,
                Source = source
            });
        }
    }
}