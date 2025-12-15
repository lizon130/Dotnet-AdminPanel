using System.Collections.Generic;

namespace ProductApp.ViewModels
{
    public class UserRolesViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public List<int>? SelectedRoleIds { get; set; }
        public List<int>? UserRoles { get; set; }
        public List<RoleCheckbox> AllRoles { get; set; } = new List<RoleCheckbox>();
    }

    public class RoleCheckbox
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class ManageUserRolesViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<SelectRoleViewModel> UserRoles { get; set; } = new List<SelectRoleViewModel>();
    }

    public class SelectRoleViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool Selected { get; set; }
    }
}