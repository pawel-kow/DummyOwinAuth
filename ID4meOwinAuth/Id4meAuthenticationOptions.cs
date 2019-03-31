using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.id4me;
using Id4meOwinAuth.DAL;
using Id4meOwinAuth.Models;
using System.Web.Hosting;
using Microsoft.Owin.Infrastructure;

namespace Id4meOwinAuth
{
    public class Id4meAuthenticationOptions : AuthenticationOptions
    {
        internal Dictionary<string, ID4meClient> clientList = new Dictionary<string, ID4meClient>();
        ID4meRegistratiornsContext registrationsDB;
        private string dbConnectionName;

        public ID4meRegistratiornsContext RegistrationsDB {
            get {
                if (registrationsDB == null)
                    registrationsDB  = new ID4meRegistratiornsContext(dbConnectionName);
                return registrationsDB;
            }
        }

        internal ID4meClient getClient(IOwinRequest request)
        {
            return getClient(request.Scheme + Uri.SchemeDelimiter + request.Host + request.PathBase + '/');
        }

        internal ID4meClient getClient(string baseUrl)
        {
            if (clientList.ContainsKey(baseUrl))
                return clientList[baseUrl];
            else
            {
                var newClient = new ID4meClient(
                    validate_url: baseUrl + CallbackPath,
                    client_name: "ASP.NET test with Owin",
                    find_authority: name =>
                        RegistrationsDB.Registrations.First(x => x.Autority == name && x.BaseUrl == baseUrl).RegistrationData,
                    save_authority: (name, registration) =>
                    {
                        var reg = RegistrationsDB.Registrations.FirstOrDefault(x => x.Autority == name && x.BaseUrl == baseUrl);
                        if (reg == null)
                        {
                            reg = new Id4meClientRegistrations()
                            {
                                Autority = name,
                                BaseUrl = baseUrl,
                                RegistrationData = registration
                            };
                            RegistrationsDB.Registrations.Add(reg);
                        }
                        else
                            reg.RegistrationData = registration;
                        RegistrationsDB.SaveChanges();
                    },
                    app_type: OIDCApplicationType.native);
                clientList[baseUrl] = newClient;
                return newClient;
            }
        }

        Dictionary<string, string> authorities = new Dictionary<string, string>();

        // TODO: add all needed params to the constructor 
        public Id4meAuthenticationOptions(string dbConnectionName)
            : base(Constants.DefaultAuthenticationType)
        {
            Description.Caption = Constants.DefaultAuthenticationType;
            Description.Properties.Add("Requires ID", true);
            CallbackPath = new PathString("/id4me-callback");
            AuthenticationMode = AuthenticationMode.Passive;
            this.dbConnectionName = dbConnectionName;

        }

        public PathString CallbackPath { get; set; }

        public string SignInAsAuthenticationType { get; set; }

        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }    }
}
