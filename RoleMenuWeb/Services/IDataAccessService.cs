using RoleMenuWeb.Models;
using RoleMenuWeb.ViewModel;
using System.Security.Claims;

namespace RoleMenuWeb.Services
{
    public interface IDataAccessService
    {
        //Task<bool> GetMenuItemsAsync(ClaimsPrincipal ctx, string ctrl, string act);
        //Task<List<NavigationMenuViewModel>> GetMenuItemsAsync(ClaimsPrincipal principal);
        List<MenuViewModel> GetAllControllerAction();
        Task<List<NavigationMenuViewModel>> GetPermissionsByRoleIdAsync(string id);
        Task<bool> SetPermissionsByRoleIdAsync(string id, IEnumerable<Guid> permissionIds);
        List<NavigationMenu> LoadUserMenuByUserEmail(string email);
        List<NavigationMenuViewModel> LoadMethods();
        void AddMenu(NavigationMenu menu);
        NavigationMenu GetNavigationMenuById(string id);
    }
}
