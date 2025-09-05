using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;
using SalahStreakApp.Models;

namespace SalahStreakApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _dbContext;


    public HomeController(ILogger<HomeController> logger, ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public IActionResult Index()
    {
        // Dashboard Header Cards
        int participantCount = _dbContext.Participants.Where(p => p.IsActive == true).Count();
        int roundCount = _dbContext.Rounds.Where(r => r.IsActive == true && r.IsCompleted == false).Count();
        int winnerCount = _dbContext.Winners.Where(w => w.IsRewarded == false).Count();
        int rewardCount = _dbContext.Rewards.Where(r => r.IsActive == true && r.DeliveryStatus == "Pending").Count();
        ViewData["ParticipantCount"] = participantCount;
        ViewData["RoundCount"] = roundCount;
        ViewData["WinnerCount"] = winnerCount;
        ViewData["RewardCount"] = rewardCount;

        // Dashboard Chart
        var totalParticipants = _dbContext.Participants.Count();
        var result = (from g in _dbContext.AgeGroups
                      let count = _dbContext.Participants
                                            .Count(p => p.Age >= g.MinAge && p.Age <= g.MaxAge)
                      select new
                      {
                          AgeGroup = g.Name,
                          Percentage = totalParticipants == 0
                                       ? 0
                                       : (count * 100.0 / totalParticipants)
                      }).ToList();
        ViewData["ChartData"] = result;

        //Dashboard Winners Card
        var winnersData = (from w in _dbContext.Winners
                           join p in _dbContext.Participants on w.ParticipantId equals p.Id
                           join r in _dbContext.Rounds on w.RoundId equals r.Id
                           join g in _dbContext.AgeGroups on w.AgeGroupId equals g.Id
                           orderby w.FinalScore descending
                           select new
                           {
                               Participant = p.FullName,
                               AgeGroup = g.Name,
                               FinalScore = (w.FinalScore / 200.0) * 100
                           })
                      .Take(5)
                      .ToList();

        // Step 2: Projection with rank in memory
        var winners = winnersData.Select((x, index) => new
        {
            Rank = index + 1,
            User = x.Participant,
            AgeGroup = x.AgeGroup,
            Completion = x.FinalScore,
            Status = "Active",
            Image = "images\\default-user.png"
        }).ToList();
        ViewData["Winners"] = winners;


        //Dashboard DataTable
        // Tabs: Age Groups
        var ageGroups = _dbContext.AgeGroups
            .Select(g => g.Name)
            .ToList();

        // Users with progress
        var users = (from w in _dbContext.Winners
                     join p in _dbContext.Participants on w.ParticipantId equals p.Id
                     join r in _dbContext.Rounds on w.RoundId equals r.Id
                     let ageGroup = _dbContext.AgeGroups
                                    .Where(g => p.Age >= g.MinAge && p.Age <= g.MaxAge)
                                    .Select(g => g.Name)
                                    .FirstOrDefault()
                     select new
                     {
                         Name = p.FullName,
                         AgeGroup = ageGroup,
                         LoopDates = $"{r.StartDate:MMM d, yyyy} - {r.EndDate:MMM d, yyyy}",
                         PrayersCompleted = $"{w.FinalScore}/200",
                         Completion = ((double)w.FinalScore / 200) * 100,
                         Status = "Active"
                     }).ToList();

        ViewData["AgeGroups"] = ageGroups;
        ViewData["Users"] = users;
        return View();
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
