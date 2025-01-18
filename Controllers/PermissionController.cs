using ContainerEvaluationSystem.Helpers;
using HealthCheck.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RepairCentral.Controllers
{
    [Authorize]
    public class PermissionController : Controller
    {
        private readonly db_HealthCheckModel _db;
        private readonly IWebHostEnvironment _environment;

        public PermissionController(db_HealthCheckModel db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpPost]
        public JsonResult CheckPermission(string action)
        {
            var userId = User.GetLoggedInUserID();
            var userPermissions = GetUserPermissions(userId);

            if (userPermissions.Contains(action))
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "You do not have permission to access this page." });
        }

        // Example method to get user permissions
        private List<string> GetUserPermissions(string UserID)
        {
            // Fetch user permissions from the database
            var permissions = _db.Permissions
                .Where(p => p.UserID == int.Parse(UserID))
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
