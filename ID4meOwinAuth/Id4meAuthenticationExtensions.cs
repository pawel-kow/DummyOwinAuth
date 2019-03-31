using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Id4meOwinAuth
{
    public static class Id4meAuthenticationExtensions
    {
        public static IAppBuilder UseID4meAuthentication(this IAppBuilder app, Id4meAuthenticationOptions options)
        {
            return app.Use(typeof(Id4meAuthenticationMiddleware), app, options);
        }
    }
}
