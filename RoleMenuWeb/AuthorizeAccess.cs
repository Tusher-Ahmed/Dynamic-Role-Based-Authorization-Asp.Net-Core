using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RoleMenuWeb.Data;
using RoleMenuWeb.Models;
using System;

namespace RoleMenuWeb
{
    public class AuthorizeAccess : ActionFilterAttribute
    {

        public AuthorizeAccess()
        {
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser=filterContext.HttpContext.User;
            var context = filterContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
          
            var name = currentUser.Identity.Name;
            var actionDescriptor = filterContext.ActionDescriptor;
            var authorizeAttributeName = GetAuthorizeAttributeName(actionDescriptor);

            var userRoles = filterContext.HttpContext.User.Claims
        .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);

            var controllerName = actionDescriptor.RouteValues["controller"];
            var actionName = actionDescriptor.RouteValues["action"];

            var controllerType = filterContext.Controller.GetType();
            var hasAuthorizeC = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            string authorizeKeyword = hasAuthorizeC  ? "Authorize" : "";

            #region CommentedAuth
            // var hasAllowAnonymous = filterContext.ActionDescriptor.FilterDescriptors.Any(filterDescriptor => filterDescriptor.Filter is AllowAnonymousAttribute);

            //if (hasAllowAnonymous)
            //{
            //    base.OnActionExecuting(filterContext);               

            //    if (currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name !=null)
            //    {
            //        var menuList = GetNavigationMenusByUsername(filterContext, name);
            //        filterContext.HttpContext.Items["MenuList"] = menuList;
            //    }
            //    return;
            //}
            //var hasAuthorize = filterContext.ActionDescriptor.FilterDescriptors.Any(filterDescriptor => filterDescriptor.Filter is  AuthorizeAttribute);
            //if (hasAuthorize)
            //{
            //    base.OnActionExecuting(filterContext);

            //    if (currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null)
            //    {
            //        var menuList = GetNavigationMenusByUsername(filterContext, name);
            //        var isAllowedMenu = menuList.Where(x => x.ControllerName == controllerName && x.ActionName == actionName).Any();
            //        if (!isAllowedMenu )
            //        {
            //            RedirectToPermissionDenied(filterContext);
            //        }
            //        filterContext.HttpContext.Items["MenuList"] = menuList;
            //    }
            //    else
            //    {
            //        RedirectToLogin(filterContext);
            //    }
            //}
            #endregion

            if(userRoles == null && currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null)
            {
                var menuList = GetNavigationMenusByUsername(filterContext, name);
                var isAllowedMenu = menuList.Where(x => x.ControllerName == controllerName && x.ActionName == actionName).Any();

                if (!isAllowedMenu && authorizeKeyword != "" && authorizeAttributeName == null)
                {
                    RedirectToPermissionDenied(filterContext);
                }

                filterContext.HttpContext.Items["MenuList"] = menuList.GroupBy(a=> new { a.ControllerName, a.ActionName }).Where(x=>x.Count() == 1).SelectMany(x=>x).ToList();                
            }

            else if (currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null && userRoles.Value != "SuperAdmin")
            {
                var menuList = GetNavigationMenusByUsername(filterContext, name);
                var isAllowedMenu = menuList.Where(x => x.ControllerName == controllerName && x.ActionName == actionName).Any();

                if (!isAllowedMenu && authorizeKeyword != "" && authorizeAttributeName == null)
                {
                    RedirectToPermissionDenied(filterContext);
                }

                filterContext.HttpContext.Items["MenuList"] = menuList.GroupBy(a => new { a.ControllerName, a.ActionName }).Where(x => x.Count() == 1).SelectMany(x => x).ToList();
            }

            else if(currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null && userRoles.Value == "SuperAdmin")
            {
                var menuList = GetAllNavigationForSuperAdmin(filterContext);
               // var menu = menuList.FirstOrDefault(x => x.ActionName == "ControllerAndActionMethod");
                var newList = menuList.GroupBy(a => new { a.ControllerName, a.ActionName }).Where(x => x.Count() == 1).SelectMany(x => x).ToList();
                //if(menu != null)
                //{
                //    newList.Add(menu);
                //}
                filterContext.HttpContext.Items["MenuList"] = newList;
            }
           

        }

        private string GetAuthorizeAttributeName(ActionDescriptor actionDescriptor)
        {
            foreach (var metadata in actionDescriptor.EndpointMetadata)
            {
                if (metadata is IAllowAnonymous)
                {
                    return "AllowAnonymous";
                }
            }
            return null;
        }
        private List<NavigationMenu> GetNavigationMenusByUsername(ActionExecutingContext filterContext,string name)
        {
            var context = filterContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            var sql = @"
        SELECT nm.Id, nm.ActionName, nm.ControllerName,nm.Name,nm.ParentMenuId
        FROM AspNetRoleMenuPermission AS rmp
        LEFT JOIN AspNetNavigationMenu AS nm ON nm.Id = rmp.NavigationMenuId
        LEFT JOIN AspNetUsers AS u ON u.UserName = {0}
        LEFT JOIN AspNetUserRoles AS ur ON ur.UserId = u.Id
        WHERE rmp.RoleId = ur.RoleId";
            var menuList = context.NavigationMenu.FromSqlRaw(sql, name).ToList();
            return menuList;
        }

        private List<NavigationMenu> GetAllNavigationForSuperAdmin(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var menuList = context.NavigationMenu.ToList();          
            return menuList;
        }
        private void RedirectToPermissionDenied(ActionExecutingContext filterContext)
        {
            var rvd = new RouteValueDictionary();
            rvd.Add("message", "Permission Denied. User appropiate login");
            filterContext.Result = new RedirectToRouteResult("PermissionDenied", rvd);
        }
        private void RedirectToLogin(ActionExecutingContext filterContext)
        {
            var rvd = new RouteValueDictionary();
            rvd.Add("message", "Permission Denied. User appropiate login");
            filterContext.Result = new RedirectToRouteResult("Login", rvd);
        }


    }
}
