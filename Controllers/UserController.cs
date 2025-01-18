using ContainerEvaluationSystem.Helpers;
using HealthCheck.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HazardInsight.Controllers
{
    public class UserController : Controller
    {
        private readonly db_HealthCheckModel _db;

        public UserController(db_HealthCheckModel db)
        {
            _db = db;

        }

        public IActionResult ProfileInfo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string NewPassword, string EmpID)
        {

            if (string.IsNullOrEmpty(EmpID) || string.IsNullOrEmpty(NewPassword))
            {
                return BadRequest("ข้อมูลไม่ครบถ้วน"); // ส่งสถานะ HTTP 400
            }

            try
            {
                // ค้นหาผู้ใช้จากเลขบัตรประจำตัว
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == EmpID);

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
            catch (Exception ex)
            {
                string message = "An error occurred while creating hazard category.";
                if (ex.InnerException != null)
                {
                    message += " Inner exception: " + ex.InnerException.Message;
                }
                return Json(new
                {
                    success = false,
                    message
                });
            }
        }


        [HttpGet]
        public async Task<JsonResult> CheckCardID(string CardID, string EmpID)
        {
            // ค้นหาผู้ใช้จากเลขบัตรประจำตัวและ EmpID
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == EmpID && u.CardID == CardID);

            // หากผู้ใช้ไม่ถูกต้อง, ส่ง false
            if (user == null)
            {
                return Json(false); // ผู้ใช้ที่ระบุไม่ถูกต้อง
            }

            // หากพบผู้ใช้ที่ตรงกัน, ส่ง true
            return Json(true); // ผู้ใช้ที่ระบุถูกต้อง
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(IFormFile ProfileImage, string Email, string Phone)
        {
            /// ตรวจสอบว่าอีเมลล์ถูกต้อง
            if (Email != null && !Email.Contains("@"))
            {
                return BadRequest("อีเมลล์ไม่ถูกต้อง");
            }

            // ค้นหาผู้ใช้ที่กำลังเข้าสู่ระบบ
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserID == int.Parse(User.GetLoggedInUserID()));

            if (user == null)
            {
                return NotFound("ไม่พบผู้ใช้");
            }

            try
            {
                // อัปเดตอีเมลล์
                user.Email = Email;
                user.Phone = Phone;

                // จัดการไฟล์โปรไฟล์
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var profileImagePath = Path.Combine("wwwroot", "profile", $"{user.Username}.jpg");
                    using (var stream = new FileStream(profileImagePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }
                    user.ImagesPath = Path.GetFileName(profileImagePath);
                }

                await _db.SaveChangesAsync();

                var dataUser = _db.Users.FirstOrDefault(s => s.UserID == int.Parse(User.GetLoggedInUserID()));

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim("UserID", dataUser.UserID.ToString()),
                new Claim("Username", dataUser.Username.ToString()),
                new Claim(ClaimTypes.Name, dataUser.FirstName+ "  " + dataUser.LastName),
                new Claim("Email", dataUser.Email),
                new Claim("Phone", dataUser.Phone),
                new Claim("ProfileImage", dataUser.ImagesPath ?? ""),
                new Claim("Position", _db.Positions.Where(p=>p.PositionID == dataUser.PositionID).Select(p=>p.PositionName).FirstOrDefault()),
                new Claim("ViewAdmin", _db.Permissions.Where(p=>p.UserID == dataUser.UserID && p.MenuID == 1).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
                new Claim("ViewHealthCheck", _db.Permissions.Where(p=>p.UserID == dataUser.UserID && p.MenuID == 2).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
                new Claim("ViewAlienlist", _db.Permissions.Where(p=>p.UserID == dataUser.UserID && p.MenuID == 3).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
                new Claim("ViewDefualtConfig", _db.Permissions.Where(p=>p.UserID == dataUser.UserID && p.MenuID == 4).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
                new Claim("ViewLogSystem", _db.Permissions.Where(p=>p.UserID == dataUser.UserID && p.MenuID == 5).Select(p => p.CanView).FirstOrDefault() == true ? "True" : "Fasle"),
            };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.NameIdentifier, ClaimsIdentity.DefaultRoleClaimType);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // บันทึกการเปลี่ยนแปลงในฐานข้อมูล

                return Ok("อัปเดตโปรไฟล์สำเร็จ");
            }
            catch (Exception ex)
            {
                string message = "An error occurred while creating hazard category.";
                if (ex.InnerException != null)
                {
                    message += " Inner exception: " + ex.InnerException.Message;
                }
                return Json(new
                {
                    success = false,
                    message
                });
            }

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
