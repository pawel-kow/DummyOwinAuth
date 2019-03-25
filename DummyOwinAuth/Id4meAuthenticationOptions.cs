using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.id4me;
using System.Web.Hosting;
using Microsoft.Owin.Infrastructure;

namespace Id4meOwinAuth
{
    public class Id4meAuthenticationOptions : AuthenticationOptions
    {
        internal ID4meClient client;
        Dictionary<string, string> authorities = new Dictionary<string, string>();

        // TODO: add all needed params to the constructor 
        public Id4meAuthenticationOptions()
            : base(Constants.DefaultAuthenticationType)
        {
            Description.Caption = Constants.DefaultAuthenticationType;
            Description.Properties.Add("Requires ID", true);
            CallbackPath = new PathString("/id4me-callback");
            AuthenticationMode = AuthenticationMode.Passive;

            client = new ID4meClient(
                //TODO: this one needs to be adjusted according to local domain
                // Idea: have a map of client IDs for BaseURL, so that the callbacks may match (may cause trouble with storage of registaration)
                validate_url: "http://localhost:53076/" + CallbackPath,
                client_name: "ASP.NET test with Owin",
                find_authority: name => authorities[name],
                save_authority: (name, reg) => authorities[name] = reg,
                app_type: OIDCApplicationType.native);        
        }

        public PathString CallbackPath { get; set; }

        public string SignInAsAuthenticationType { get; set; }

        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
    }
}
