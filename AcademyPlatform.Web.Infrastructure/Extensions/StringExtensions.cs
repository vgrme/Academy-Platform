﻿namespace AcademyPlatform.Web.Infrastructure.Extensions
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Security;

    public static class StringExtensions
    {
        public static string Protect(this string unprotectedText, string purpose)
        {
            var unprotectedBytes = Encoding.UTF8.GetBytes(unprotectedText);
            var protectedBytes = MachineKey.Protect(unprotectedBytes, purpose);
            var protectedText = Convert.ToBase64String(protectedBytes);
            return protectedText;
        }

        public static string Unprotect(this string protectedText, string purpose)
        {
            var protectedBytes = Convert.FromBase64String(protectedText);
            var unprotectedBytes = MachineKey.Unprotect(protectedBytes, purpose);
            var unprotectedText = Encoding.UTF8.GetString(unprotectedBytes);
            return unprotectedText;
        }

        public static string StripControllerSufix(this string controllerName)
        {
            return controllerName.EndsWith("Controller") ? controllerName.Substring(0, controllerName.Length - 10) : controllerName;
        }

        public static string ToSafeReturnUrl(this UrlHelper urlHelper, string returnUrl)
		{
			var url = new Uri(returnUrl, UriKind.RelativeOrAbsolute);
			var host = urlHelper.RequestContext.HttpContext.Request.Url.Host;
			if (!url.IsAbsoluteUri)
			{
				return new Uri(new Uri("http://" + host), url).AbsoluteUri;
			}
			if (url.Host == host)
			{
				return new Uri(new Uri("http://" + host), url.GetComponents(
					UriComponents.Path | UriComponents.Query | UriComponents.Fragment, UriFormat.UriEscaped)).AbsoluteUri;
			}
			
			return new Uri(new Uri("http://" + host), FormsAuthentication.DefaultUrl).AbsoluteUri;
		}
    }
}
