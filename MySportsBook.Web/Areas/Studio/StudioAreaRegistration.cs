using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MySportsBook.Web.Areas.Studio
{
    public class StudioAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Studio";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Studio_default",
                "Studio/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}