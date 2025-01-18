using HealthCheck.Data;
using Microsoft.AspNetCore.Mvc;

namespace HealthChecks.Controllers
{
    public class HistoryHealthController : Controller
    {
        private readonly db_HealthCheckModel _db;

        public HistoryHealthController(db_HealthCheckModel db)
        {
            _db = db;
        }

        public IActionResult HistoryHealthPage()
        {
            ViewBag.HistoryHealthCheck = _db.HistoryHealths
              .GroupBy(p => p.Alchkdate) // Group by the date part of Alchkdate
              .Select(g => new
              {
                  Alchkdate = g.Key, // Grouped date
                  TotalCount = g.Count() // Count of HistoryHealthID in the same date
              }).ToList();

            return View();
        }

        public IActionResult HistoryDetail(string Alchkdate)
        {
            ViewBag.Alchkdate = Alchkdate;

            ViewBag.HistoryHealthDetail = _db.HistoryHealths
             .Where(p => p.Alchkdate == Alchkdate)
             .ToList();

            return View();
        }
    }
}
