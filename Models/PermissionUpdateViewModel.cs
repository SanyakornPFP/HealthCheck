namespace HealthCheck.Models
{
    public class PermissionUpdateViewModel
    {
        public int UserID { get; set; }
        public List<PermissionViewModel> Permissions { get; set; }
    }
    public class PermissionViewModel
    {
        public int MenuID { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }
}
