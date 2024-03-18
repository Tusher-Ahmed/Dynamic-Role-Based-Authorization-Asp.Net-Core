using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoleMenuWeb.Data;
using RoleMenuWeb.Models;
using System.Reflection;

namespace RoleMenuWeb.Services
{
    public class ParentMenuAttribute : Attribute
    {
        public string ParentName { get; }

        public ParentMenuAttribute(string parentName)
        {
            ParentName = parentName;
        }
    }
    public class MenuService
    {
        private readonly AppDbContext _context;
        public MenuService(AppDbContext context)
        {
            _context = context;
        }

        public void AddMenuItems()
        {
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);
            if (controllers.Any())
            {
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

                            if (!isPresent && allowAnonymousAttribute == null)
                            {
                                var returnType = method.ReturnType;

                                if (typeof(IActionResult).IsAssignableFrom(returnType) || typeof(Task<IActionResult>).IsAssignableFrom(returnType))
                                {
                                    var menuItem = new NavigationMenu
                                    {
                                        Name = method.Name,
                                        ControllerName = controllerName,
                                        ActionName = method.Name,
                                    };
                                    var parentMenuAttribute = method.GetCustomAttribute<ParentMenuAttribute>();

                                    if (parentMenuAttribute != null)
                                    {
                                        var parentMenuItem = _context.NavigationMenu.FirstOrDefault(item => item.Name == parentMenuAttribute.ParentName);
                                        if (parentMenuItem != null)
                                        {
                                            menuItem.ParentMenuId = parentMenuItem.Id;
                                        }
                                    }
                                    _context.NavigationMenu.Add(menuItem);
                                }                                
                            }
                        }
                    }
                }
               // _context.SaveChanges();
            }
        }
    }
}
