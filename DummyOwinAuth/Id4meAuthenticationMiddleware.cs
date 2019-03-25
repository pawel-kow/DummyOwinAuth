using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.DataHandler;

namespace Id4meOwinAuth
{
    // One instance is created when the application starts.
    public class Id4meAuthenticationMiddleware : AuthenticationMiddleware<Id4meAuthenticationOptions>
    {
        public Id4meAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, Id4meAuthenticationOptions options)
            : base(next, options)
        { 
            if(string.IsNullOrEmpty(Options.SignInAsAuthenticationType))
            {
                options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }
            if(options.StateDataFormat == null)
            {
                var dataProtector = app.CreateDataProtector(typeof(Id4meAuthenticationMiddleware).FullName,
                    options.AuthenticationType);

                options.StateDataFormat = new PropertiesDataFormat(dataProtector);
            }
        }

        // Called for each request, to create a handler for each request.
        protected override AuthenticationHandler<Id4meAuthenticationOptions> CreateHandler()
        {
            return new Id4meAuthenticationHandler();
        }
    }
}
