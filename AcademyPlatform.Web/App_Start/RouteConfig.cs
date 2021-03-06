﻿namespace AcademyPlatform.Web
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapMvcAttributeRoutes();

            //routes.MapRoute(
            //    name: "CoursesPrettyUrl",
            //    url: "Courses/{id}",
            //    defaults: new { controller = "Courses", action = "Details" },
            //    namespaces: new[] { "AcademyPlatform.Web.Controllers" }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "AcademyPlatform.Web.Controllers" }
            );


        }
    }
}
