using HealthCheck.Data;
using HealthCheck.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthChecks.Controllers
{
    [Authorize]
    public class LogSystemController : Controller
    {
        private readonly db_HealthCheckModel _db;

        public LogSystemController(db_HealthCheckModel db)
        {
            _db = db;
        }

        public IActionResult Logs(string searchString, DateTime? startDate, DateTime? endDate)
        {
            var logs = _db.LogSystems.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                logs = logs.Where(p => p.ActionTitle.Contains(searchString) ||
                                       p.User.Username.Contains(searchString) ||
                                       p.TableName.Contains(searchString) ||
                                       p.Detail.Contains(searchString));
            }

            if (startDate.HasValue)
            {
                logs = logs.Where(p => p.TimeStamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                logs = logs.Where(p => p.TimeStamp <= endDate.Value);
            }

            ViewBag.LogSystem = logs.Select(p => new
            {
                ActionTitle = p.ActionTitle,
                UserName = p.User.Username,
                TableName = p.TableName,
                RecordID = p.RecordID,
                TimeStamp = p.TimeStamp,
                Detail = p.Detail
            }).ToList();

            return View();
        }
    }
}
