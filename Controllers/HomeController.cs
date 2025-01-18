using HealthCheck.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthChecks.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly db_HealthCheckModel _db;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public HomeController(IWebHostEnvironment hostingEnvironment, db_HealthCheckModel db)
        {
            _hostingEnvironment = hostingEnvironment;
            _db = db;
        }
        public IActionResult Dashboard()
        {
            ViewBag.HazardObjectTotal = _db.Alienlists.Count(a => a.IsActive);
            ViewBag.FormHazardTotal = _db.Healthchecks.Count(h => h.IsActive);
            ViewBag.UserTotal = _db.Users.Count(u => u.IsActive);
            ViewBag.EmployerTotal = _db.Employees.Count(e => e.IsActive);

            ViewBag.FormMains = _db.Healthchecks
                .OrderByDescending(h => h.CreatedAt)
                .Take(10)
                .Select(h => new
                {
                    CodeForm = h.HealthID,
                    FormName = h.Chkname,
                    EditCount = h.Alchkdesc,
                    CreatedAt = h.CreatedAt
                }).ToList();


            ViewBag.UpdateloadHistory = _db.HistoryHealths
                .OrderByDescending(s => s.HistoryHealthID)
                .Take(20)
              .ToList();

            ViewBag.LogSystem = _db.LogSystems
                .OrderByDescending(l => l.TimeStamp)
                .Take(10)
                .Select(l => new
                {
                    Action = l.ActionTitle,
                    NewValue = l.Detail,
                    OldValue = l.TableName,
                    ChangeDate = l.TimeStamp
                }).ToList();

            return View();
        }



        [HttpGet]
        public JsonResult GetChartData()
        {
            var data = _db.Healthchecks
                .Where(h => h.CreatedAt.HasValue && h.CreatedAt.Value.Year == DateTime.Now.Year)
                .GroupBy(h => h.CreatedAt.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                }).ToList();

            var months = Enumerable.Range(1, 12).Select(i => new DateTime(DateTime.Now.Year, i, 1).ToString("MMM")).ToArray();
            var counts = new int[12];

            foreach (var item in data)
            {
                counts[item.Month - 1] = item.Count;
            }

            return Json(new { success = true, data = new { months, counts } });
        }

    }
}
