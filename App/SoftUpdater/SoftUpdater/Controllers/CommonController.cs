//Copyright 2021 Dmitriy Rokoth
//Licensed under the Apache License, Version 2.0
//
//ref 1
using Microsoft.AspNetCore.Mvc;

namespace SoftUpdater.Controllers
{
    /// <summary>
    /// Common api calls (without models)
    /// </summary>
    public class CommonController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
