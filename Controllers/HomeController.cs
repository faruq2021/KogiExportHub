using Microsoft.AspNetCore.Mvc;
using KogiExportHub.Models;
using KogiExportHub.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Diagnostics;

namespace KogiExportHub.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel();
        
        // Get general statistics only
        viewModel.TotalProposals = await _context.InfrastructureProposals.CountAsync();
        viewModel.ActiveProposals = await _context.InfrastructureProposals
            .Where(p => p.Status == "Under Review" || p.Status == "Approved")
            .CountAsync();
        viewModel.TotalFundingRequested = await _context.FundingRequests
            .SumAsync(f => f.AmountRequested);
        viewModel.TotalFundingApproved = await _context.FundingRequests
            .Where(f => f.Status == "Approved")
            .SumAsync(f => f.AmountApproved ?? 0);
        
        // Remove all user-specific dashboard data loading
        // Dashboard functionality moved to InfrastructureController.Dashboard()
        
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
