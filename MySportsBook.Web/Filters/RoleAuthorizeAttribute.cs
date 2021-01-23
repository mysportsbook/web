using MySportsBook.Web.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MySportsBook.Web.Filters
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly RoleAction _roleaction;
        public RoleAuthorizeAttribute(RoleAction RoleAction)
        {
            this._roleaction = RoleAction;
        }
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            //bool authorize = false;
            //var userId = Convert.ToString(httpContext.Session["UserId"]);
            //if (!string.IsNullOrEmpty(userId))
            //    using (var context = new SqlDbContext())
            //    {
            //        var userRole = (from u in context.Users
            //                        join r in context.Roles on u.RoleId equals r.Id
            //                        where u.UserId == userId
            //                        select new
            //                        {
            //                            r.Name
            //                        }).FirstOrDefault();
            //        foreach (var role in allowedroles)
            //        {
            //            if (role == userRole.Name) return true;
            //        }
            //    }


            //return authorize;
            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(
               new RouteValueDictionary
               {
                    { "controller", "Account" },
                    { "action", "UnAuthorized" }
               });
        }
    }
}  
