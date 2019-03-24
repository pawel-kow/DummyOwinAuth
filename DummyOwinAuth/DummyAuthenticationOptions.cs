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

namespace DummyOwinAuth
{
    public class DummyAuthenticationOptions : AuthenticationOptions
    {
        internal ID4meClient client;
        Dictionary<string, string> authorities = new Dictionary<string, string>();
        public DummyAuthenticationOptions(string userName, string userId)
            : base(Constants.DefaultAuthenticationType)
        {
            Description.Caption = Constants.DefaultAuthenticationType;
            Description.Properties.Add("Requires ID", true);
            CallbackPath = new PathString();
            AuthenticationMode = AuthenticationMode.Passive;
            UserName = userName;
            UserId = userId;



            client = new ID4meClient(
                //TODO: this one needs to be adjusted according to local domain
                validate_url: "http://localhost:53076/id4me-callback",
                client_name: "ASP.NET test with Owin",
                find_authority: name => authorities[name],
                save_authority: (name, reg) => authorities[name] = reg,
                app_type: OIDCApplicationType.native);        
        }

        public PathString CallbackPath { get; set; }

        public string UserName { get; set; }

        public string UserId { get; set; }

        public string SignInAsAuthenticationType { get; set; }

        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
    }
}
