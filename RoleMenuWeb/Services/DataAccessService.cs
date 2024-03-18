using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RoleMenuWeb.Data;
using RoleMenuWeb.Models;
using RoleMenuWeb.ViewModel;
using System.Data;
using System;
using System.Security.Claims;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace RoleMenuWeb.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly AppDbContext _context;
        public DataAccessService(AppDbContext context)
        {
            _context = context;
        }

        public void AddMenu(NavigationMenu menu)
        {
            _context.AddAsync(menu);
            _context.SaveChanges();
        }

        public List<MenuViewModel> GetAllControllerAction()
        {
            var sql = @"
select 
nm.Id,
nm.Name,
nm.ParentmenuId,
nm.ControllerName,
nm.ActionName,
CASE WHEN m.NavigationMenuId = nm.Id THEN 'true' ELSE 'false' END AS permitted
from AspNetNavigationMenu as nm
left join Menus as m on nm.Id = m.NavigationMenuId
";
            var menuList=_context.NavigationMenu.FromSqlRaw(sql).ToList();
            List<MenuViewModel> list = new List<MenuViewModel>();
            foreach (var menu in menuList)
            {
                var m = new MenuViewModel
                {                    
                    ActionName = menu.ActionName,
                    ControllerName = menu.ControllerName,
                    ParentMenuId = menu.ParentMenuId,
                    NavigationMenuId = menu.Id,
                    Name = menu.Name,
                    Permitted = menu.Permitted
                };
                list.Add(m);
            }
            return list;
        }

        public NavigationMenu GetNavigationMenuById(string id)
        {
            return _context.NavigationMenu.Find(id);
        }

        public async Task<List<NavigationMenuViewModel>> GetPermissionsByRoleIdAsync(string id)
        {
            var items = await (from m in _context.NavigationMenu
                               join rm in _context.RoleMenuPermission
                               on new { X1 = m.Id, X2 = id } equals new { X1 = rm.NavigationMenuId, X2 = rm.RoleId }
                               into rmp
                               from rm in rmp.DefaultIfEmpty()
                               select new NavigationMenuViewModel()
                               {
                                   Id = m.Id,
                                   Name = m.Name,
                                   ActionName = m.ActionName,
                                   ControllerName = m.ControllerName,                               
                                   Permitted = rm.RoleId == id
                               })
                                .AsNoTracking()
                                .ToListAsync();



            return items;
        }

        public List<NavigationMenuViewModel> LoadMethods()
        {
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);
            if (controllers.Any())
            {
                List<NavigationMenuViewModel> list = new List<NavigationMenuViewModel>();
                foreach (var controller in controllers)
                {
                    var controllerName = controller.Name.Substring(0, controller.Name.Length - 10);
                    var controllerAttributes = controller.GetCustomAttributes(true);
                    var authorizeAttribute = controllerAttributes.FirstOrDefault(attr => attr.GetType() == typeof(AuthorizeAttribute)) as AuthorizeAttribute;
                    if (authorizeAttribute != null)
                    {
                        var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                        foreach (var method in methods)
                        {
                            var attributes = method.GetCustomAttributes(true);
                            var allowAnonymousAttribute = attributes.FirstOrDefault(attr => attr.GetType() == typeof(AllowAnonymousAttribute)) as AllowAnonymousAttribute;
                            var isPresent = _context.NavigationMenu.Where(item => item.ControllerName == controllerName && item.ActionName == method.Name).Any();

                            if ( allowAnonymousAttribute == null)
                            {
                                var returnType = method.ReturnType;

                                if (typeof(IActionResult).IsAssignableFrom(returnType) || typeof(Task<IActionResult>).IsAssignableFrom(returnType))
                                {
                                    var menuItem = new NavigationMenuViewModel
                                    {                                        
                                        Name = method.Name,
                                        ControllerName = controllerName,
                                        ActionName = method.Name,
                                        Permitted = isPresent
                                    };
                                   
                                    list.Add(menuItem);
                                }
                            }
                        }
                    }
                }
                return list;
            }
            return new List<NavigationMenuViewModel>();
        }

        public List<NavigationMenu> LoadUserMenuByUserEmail(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetPermissionsByRoleIdAsync(string id, IEnumerable<Guid> permissionIds)
        {
            var existing = await _context.RoleMenuPermission.Where(x => x.RoleId == id).ToListAsync();
            _context.RemoveRange(existing);
            await _context.SaveChangesAsync();

            foreach (var item in permissionIds)
            {
                await _context.RoleMenuPermission.AddAsync(new RoleMenuPermission()
                {
                    RoleId = id,
                    NavigationMenuId = item,
                });
            }
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        
    }
}
