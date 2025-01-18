using Microsoft.AspNetCore.Mvc;
using HealthCheck.Data;
using HealthCheck.Models;
using System.Security;
using System.Text;
using System.Security.Cryptography;
using ContainerEvaluationSystem.Helpers;
using Microsoft.AspNetCore.Authorization;


namespace HealthCheks.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly db_HealthCheckModel _db;
        private readonly IWebHostEnvironment _environment;

        public AdminController(db_HealthCheckModel db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        public IActionResult Users(string searchString)
        {
            ViewBag.Position = _db.Positions.Where(p => p.IsActive).ToList();
            // Fetch user data based on searchString (optional)

            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.UserList = _db.Users
                                    .Where(u => u.IsActive && (u.Username.Contains(searchString) || u.FirstName.Contains(searchString) || u.LastName.Contains(searchString)))
                                    .ToList();
            }
            else
            {
                ViewBag.UserList = _db.Users.Where(u => u.IsActive).ToList();
            }

            return View();
        }

        #region Create User 

        [HttpPost]
        public async Task<IActionResult> CreateUser(string Username, string FirstName, string LastName, string Email, string PhoneNumber, int PositionID, string CardID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("CreateAdmin"))
            {
                return Json(new { success = false, message = "You do not have permission to create." });
            }

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(CardID))
            {
                return Json(new { success = false, message = "กรุณากรอกข้อมูลให้ครบถ้วน." });
            }

            var isDuplicateUsername = _db.Users.Any(u => u.Username == Username);
            var isDuplicateCardID = _db.Users.Any(u => u.CardID == CardID);
            var isDuplicateEmail = _db.Users.Any(u => u.Email == Email);

            if (isDuplicateUsername)
            {
                return Json(new { success = false, message = "Username already exists." });
            }
            else if (isDuplicateCardID)
            {
                return Json(new { success = false, message = "CardID already exists." });
            }
            else if (isDuplicateEmail)
            {
                return Json(new { success = false, message = "Email already exists." });
            }

            //การแปลงข้อมูลให้อยู่ในรูปแบบที่ไม่สามารถย้อนกลับได้ (Hashing)     
            string CardIDHash = ComputeSha256Hash(CardID);

            var user = new User
            {
                Username = Username,
                PasswordHash = CardIDHash,
                FirstName = FirstName,
                LastName = LastName,
                Email = Email,
                Phone = PhoneNumber,
                PositionID = PositionID,
                CardID = CardID,
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();


            // Retrieve all menu items to create default permissions for the new user
            var menus = _db.Menus.ToList();

            foreach (var menu in menus)
            {
                var permission = new Permission
                {
                    UserID = user.UserID,
                    MenuID = menu.MenuID,
                    CanView = true, // Set default permissions as required
                    CanCreate = false, // Set default permissions as required
                    CanUpdate = false,
                    CanDelete = false
                };

                _db.Permissions.Add(permission);
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "User created successfully." });
        }

        #endregion

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

        [HttpGet]
        public JsonResult ModelUser(int UserID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("ViewAdmin"))
            {
                return Json(new { success = false, message = "You do not have permission to view." });
            }

            var user = _db.Users
                .Where(u => u.UserID == UserID && u.IsActive)
                .Select(u => new
                {
                    u.UserID,
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.PositionID,
                    u.CardID,
                })
                .FirstOrDefault();

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            return Json(user);
        }

        #region Update User
        [HttpPost]
        public async Task<IActionResult> UpdateUser(int UserID, string Username, string FirstName, string LastName, string Email, string PhoneNumber, int PositionID, string CardID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateAdmin"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var user = _db.Users.FirstOrDefault(u => u.UserID == UserID && u.IsActive);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            if (_db.Users.Any(u => u.Username == Username && u.UserID != UserID))
            {
                return Json(new { success = false, message = "Username already exists." });
            }

            user.Username = Username;
            user.FirstName = FirstName;
            user.LastName = LastName;
            user.Email = Email;
            user.Phone = PhoneNumber;
            user.PositionID = PositionID;
            user.CardID = CardID;
            user.UpdatedAt = DateTime.Now;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "User updated successfully." });
        }
        #endregion

        #region Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int UserID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("DeleteAdmin"))
            {
                return Json(new { success = false, message = "You do not have permission to delete." });
            }
            var user = _db.Users.FirstOrDefault(u => u.UserID == UserID && u.IsActive);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }
            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "User deleted successfully." });
        }
        #endregion

        #region Permisstion
        public IActionResult Permissions(string searchString)
        {
            // Fetch user data based on searchString (optional)
            if (!string.IsNullOrEmpty(searchString))
            {
                ViewBag.UserList = _db.Users
                                    .Where(u => u.IsActive && (u.Username.Contains(searchString) || u.FirstName.Contains(searchString) || u.LastName.Contains(searchString)))
                                    .ToList();
            }
            else
            {
                ViewBag.UserList = _db.Users.Where(u => u.IsActive).ToList();
            }

            return View();
        }


        [HttpGet]
        public JsonResult GetDetailsPermissions(int UserID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("ViewAdmin"))
            {
                return Json(new { success = false, message = "You do not have permission to view." });
            }

            var user = _db.Users.FirstOrDefault(u => u.UserID == UserID);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Fetch permissions logic from the database
            var permissions = _db.Permissions
                .Where(p => p.UserID == UserID)
                .ToList();

            if (permissions == null || permissions.Count == 0)
            {
                return Json(new { success = false, message = "Permissions not found for this user." });
            }

            var permissionsDict = new Dictionary<string, object>();
            foreach (var perm in permissions)
            {
#pragma warning disable CS8604 // Possible null reference argument.
                permissionsDict[perm.MenuID.ToString()] = new
                {
                    CanView = perm.CanView,
                    CanCreate = perm.CanCreate,
                    CanUpdate = perm.CanUpdate,
                    CanDelete = perm.CanDelete
                };
#pragma warning restore CS8604 // Possible null reference argument.
            }

            var response = new
            {
                groupUserID = user.UserID,
                groupName = user.Username,
                firstName = user.FirstName,
                lastName = user.LastName,
                permissions = permissionsDict
            };

            return Json(new { success = true, permissions = response.permissions, groupUserID = response.groupUserID, groupName = response.groupName, firstName = response.firstName, lastName = response.lastName });
        }

        [HttpPost]
        public JsonResult UpdatePermissions([FromBody] PermissionUpdateViewModel model)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateAdmin"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            if (model == null || model.Permissions == null || model.Permissions.Count == 0)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            try
            {
                // Get the user from the database
                var user = _db.Users.FirstOrDefault(u => u.UserID == model.UserID);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Loop through the permissions and update them in the database
                foreach (var perm in model.Permissions)
                {
                    var existingPermission = _db.Permissions
                        .FirstOrDefault(p => p.UserID == model.UserID && p.MenuID == perm.MenuID);

                    if (existingPermission != null)
                    {
                        // Update existing permission
                        existingPermission.CanView = perm.CanView;
                        existingPermission.CanCreate = perm.CanCreate;
                        existingPermission.CanUpdate = perm.CanUpdate;
                        existingPermission.CanDelete = perm.CanDelete;
                    }
                    else
                    {
                        // Add new permission
                        var newPermission = new Permission
                        {
                            UserID = model.UserID,
                            MenuID = perm.MenuID,
                            CanView = perm.CanView,
                            CanUpdate = perm.CanUpdate,
                            CanDelete = perm.CanDelete
                        };
                        _db.Permissions.Add(newPermission);
                    }
                }

                // Save changes to the database
                _db.SaveChanges();

                return Json(new { success = true, message = "Permissions updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating permissions: " + ex.Message });
            }
        }
        ///Region Permissions - End
        #endregion

        // Example method to get user permissions
        private List<string> GetUserPermissions(int userId)
        {
            // Fetch user permissions from the database
            var permissions = _db.Permissions
                .Where(p => p.UserID == userId)
                .Select(p => new
                {
                    p.CanView,
                    p.CanCreate,
                    p.CanUpdate,
                    p.CanDelete,
                    p.Menu.MenuID,
                    p.Menu.MenuName
                })
                .ToList();

            var userPermissions = new List<string>();

            foreach (var permission in permissions)
            {
                if ((bool)permission.CanView)
                {
                    userPermissions.Add("View" + permission.MenuName);
                }
                if ((bool)permission.CanCreate)
                {
                    userPermissions.Add("Create" + permission.MenuName);
                }
                if ((bool)permission.CanUpdate)
                {
                    userPermissions.Add("Update" + permission.MenuName);
                }
                if ((bool)permission.CanDelete)
                {
                    userPermissions.Add("Delete" + permission.MenuName);
                }
            }

            return userPermissions;
        }
    }
}
