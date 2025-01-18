using Azure.Core;
using HealthCheck.Data;
using HealthCheck.Models;
using HealthChecks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.ComponentModel;
using System.Reflection.Emit;

namespace HealthChecks.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckResultDailyController : Controller
    {
        private readonly db_HealthCheckModel _db;

        public HealthCheckResultDailyController(db_HealthCheckModel db)
        {
            _db = db;
        }

        [HttpGet("GetResultDaily")]
        public IActionResult GetResultDaily([FromBody] RequestHealthDaily model)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

            var tokenResult = _db.Tokens.Where(t => t.TokenCode == model.Token).FirstOrDefault();
            if (tokenResult == null)
            {
                return Ok(new { statuscode = "-101", statusdesc = "token ไม่ถูกต้องกรุณาลองใหม่อีกครั้ง" });
            }

            int pageSize = 100;
            int pageNumber = int.TryParse(model.DataPage, out int dp) ? dp : 1;

            var query = _db.Healthchecks.AsQueryable();

            if (!string.IsNullOrEmpty(model.SearchDate))
            {
                query = query.Where(h => h.Alchkdate == model.SearchDate);
            }

            var totalRecords = query.Count();

            if (totalRecords == 0)
            {
                return Ok(new { statuscode = "-102", statusdesc = "ไม่พบรายการผลตรวจสุขภาพจากวันที่ค้นหา" });
            }

            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var resultList = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new
                {
                    alcode = h.Alcode,
                    alchkhos = h.Alchkhos,
                    alchkstatus = h.AlchkstatusID,
                    alchkdate = h.Alchkdate,
                    alchkprovid = h.Alchkprovid,
                    licenseno = h.Licenseno,
                    chkname = h.Chkname,
                    chkposition = h.Chkposition,
                    alchkdesc = h.Alchkdesc,
                    alchkdoc = h.Alchkdoc != null ? $"{baseUrl}/{h.Alchkdoc}" : null
                })
            .ToList();

            var response = new
            {
                statuscode = "1",
                statusdesc = "ดึงข้อมูลสำเร็จ",
                datapage = model.DataPage,
                totalpage = totalPages.ToString(),
                totalrecorde = totalRecords.ToString(),
                resultlist = resultList
            };

            return Ok(response);
        }
    }
}
