using EventRegistrationSystem.Models;
using EventRegistrationSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EventRegistrationSystem.Controllers;

public sealed class HomeController : AppController
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEventRepository _repository;

    public HomeController(ILogger<HomeController> logger, IEventRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        var model = new HomePageViewModel
        {
            CurrentUser = CurrentUser,
            FeaturedEvents = _repository.GetPublicEvents().Take(3).ToList()
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
