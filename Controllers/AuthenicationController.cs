using HealthCheck.Data;
using HealthCheck.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RepairCentral.Controllers
{
    public class AuthenicationController : Controller
    {
        private readonly db_HealthCheckModel _db;
        public AuthenicationController(db_HealthCheckModel db)
        {
            _db = db;
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult ChangePass()
        {
            return View();
        }

        [HttpGet]
        public JsonResult CheckCardID(string CardID)
        {
            // ค้นหาผู้ใช้จากเลขบัตรประจำตัวและ EmpID
            var user = _db.Users.FirstOrDefault(u => u.CardID == CardID);

            // หากผู้ใช้ไม่ถูกต้อง, ส่ง false
            if (user == null)
            {
                return Json(false); // ผู้ใช้ที่ระบุไม่ถูกต้อง
            }

            // หากพบผู้ใช้ที่ตรงกัน, ส่ง true
            return Json(true); // ผู้ใช้ที่ระบุถูกต้อง
        }

        [HttpPost]
        public async Task<IActionResult> UpdateModelPassword(string NewPassword, string CardID)
        {
            if (string.IsNullOrEmpty(CardID) || string.IsNullOrEmpty(NewPassword))
            {
                return BadRequest("ข้อมูลไม่ครบถ้วน"); // ส่งสถานะ HTTP 400
            }

            // ค้นหาผู้ใช้จากเลขบัตรประจำตัว
            var user = _db.Users.FirstOrDefault(u => u.CardID == CardID);

            if (user == null)
            {
                return NotFound("ไม่พบผู้ใช้ที่ระบุ"); // ส่งสถานะ HTTP 404
            }

            // เปลี่ยนรหัสผ่าน
            user.PasswordHash = ComputeSha256Hash(NewPassword);

            _db.Users.Update(user); // อัปเดตผู้ใช้ในฐานข้อมูล
            await _db.SaveChangesAsync(); // บันทึกการเปลี่ยนแปลง

            return Ok("การเปลี่ยนรหัสผ่านสำเร็จ"); // ส่งสถานะ HTTP 200
        }

        public async Task<IActionResult> LoginResponse()
        {
            if (User.Identity!.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("th-TH")),
                        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(1) }
                    );

                return RedirectToAction("Dashboard", "Home");
            }
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        public async Task<IActionResult> LoginResponse(string Username, string Password, bool RememberMe)
        {
            try
            {
                //var user = _db.ViewUsers.FirstOrDefault(s => s.Username == Username && s.PasswordHash == Password);
                var user = _db.Users
                    .FirstOrDefault(s => s.Username == Username && s.PasswordHash == ComputeSha256Hash(Password));

                if (user == null)
                {
                    return Unauthorized(); // ส่งสถานะ HTTP 401 ถ้าผู้ใช้หรือรหัสผ่านไม่ถูกต้อง
                }

                var props = new AuthenticationProperties
                {
                    IsPersistent = RememberMe,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                };

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim("UserID", user.UserID.ToString()),
                new Claim("Username", user.Username.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName+ "  " + user.LastName),
                new Claim("Email", user.Email),
                new Claim("Phone", user.Phone),
                new Claim("ProfileImage", user.ImagesPath ?? ""),
                new Claim("Position", _db.Positions.Where(p=>p.PositionID == user.PositionID).Select(p=>p.PositionName).FirstOrDefault()),
                new Claim("ViewAdmin", _db.Permissions.Where(p=>p.UserID == user.UserID && p.MenuID == 1).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
                new Claim("ViewHealthCheck", _db.Permissions.Where(p=>p.UserID == user.UserID && p.MenuID == 2).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
                new Claim("ViewAlienlist", _db.Permissions.Where(p=>p.UserID == user.UserID && p.MenuID == 3).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
                new Claim("ViewDefualtConfig", _db.Permissions.Where(p=>p.UserID == user.UserID && p.MenuID == 4).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
                new Claim("ViewLogSystem", _db.Permissions.Where(p=>p.UserID == user.UserID && p.MenuID == 5).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
            };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.NameIdentifier, ClaimsIdentity.DefaultRoleClaimType);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

                return Ok(); // ส่งสถานะ HTTP 200 หากเข้าสู่ระบบสำเร็จ
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}"); // บันทึกข้อผิดพลาด
                return StatusCode(500, "เกิดข้อผิดพลาดภายในเซิร์ฟเวอร์"); // ส่งสถานะ HTTP 500 พร้อมข้อความที่เหมาะสม
            }
        }


        [HttpGet]
        public JsonResult CheckUserPassword(string Username, string Password)
        {
            //var model = _db.Users.Where(s => s.Username == Username && s.PasswordHash == Password).FirstOrDefault();
            var model = _db.Users.Where(s => s.Username == Username && s.PasswordHash == ComputeSha256Hash(Password)).FirstOrDefault();
            return Json(model);
        }

        [HttpGet]
        public JsonResult CheckBeen(string Username, string Password)
        {
            //var model = _db.Users.Where(s => s.Username == Username && s.PasswordHash == Password).FirstOrDefault();
            var model = _db.Users.Where(s => s.Username == Username && s.PasswordHash == ComputeSha256Hash(Password)).FirstOrDefault();
            return Json(model);
        }

        public async Task<IActionResult> Logout()
        {

            // Clear the existing external cookie
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login), "Authenication");
        }

        //Hashing คือการแปลงข้อมูลให้อยู่ในรูปแบบที่ไม่สามารถย้อนกลับได้ เหมาะสำหรับการตรวจสอบความถูกต้องของข้อมูล เช่น การตรวจสอบรหัสผ่าน
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
