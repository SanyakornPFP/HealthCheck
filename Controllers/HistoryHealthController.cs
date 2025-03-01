using HealthCheck.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthChecks.Controllers
{
    [Authorize]
    public class HistoryHealthController : Controller
    {
        private readonly db_HealthCheckModel _db;

        public HistoryHealthController(db_HealthCheckModel db)
        {
            _db = db;
        }

        public IActionResult HistoryHealthPage()
        {
            var HistoryHealthCheck = _db.HistoryHealths
         .AsEnumerable() // แปลงเป็น IEnumerable เพื่อใช้ LINQ to Objects
         .Where(s => DateTime.TryParseExact(s.Alchkdate, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out _)) // กรองเฉพาะข้อมูลที่สามารถแปลงเป็น DateTime ได้
         .GroupBy(p => p.Alchkdate) // Group by the date part of Alchkdate
         .Select(g => new
         {
             Alchkdate = g.Key, // Grouped date in string format
             TotalCount = g.Count() // Count of HistoryHealthID in the same date
         }).ToList();

            ViewBag.HistoryHealthCheck = HistoryHealthCheck
                .OrderByDescending(s => DateTime.ParseExact(s.Alchkdate, "dd-MM-yyyy", null)) // แปลง Alchkdate เป็น DateTime และเรียงลำดับ
                .ToList();

            return View();
        }

        public IActionResult HistoryDetail(string Alchkdate)
        {
            ViewBag.Alchkdate = Alchkdate;

            ViewBag.HistoryHealthDetail = _db.HistoryHealths
                .AsEnumerable() // แปลงเป็น IEnumerable เพื่อใช้ LINQ to Objects
                .Where(p => p.Alchkdate == Alchkdate) // แปลง Alchkdate เป็น DateTime และเปรียบเทียบ
                .ToList();

            return View();
        }
    }
}
