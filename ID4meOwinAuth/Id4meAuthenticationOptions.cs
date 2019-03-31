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
        internal ID4meClient client;
        ID4meRegistratiornsContext registrationsDB;
        private string dbConnectionName;

        public ID4meRegistratiornsContext RegistrationsDB {
            get {
                if (registrationsDB == null)
                    registrationsDB  = new ID4meRegistratiornsContext(dbConnectionName);
                return registrationsDB;
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

            client = new ID4meClient(
                //TODO: this one needs to be adjusted according to local domain
                // Idea: have a map of client IDs for BaseURL, so that the callbacks may match (may cause trouble with storage of registaration)
                validate_url: "http://localhost:53076/" + CallbackPath,
                client_name: "ASP.NET test with Owin",
                find_authority: name =>
                    RegistrationsDB.Registrations.First(x => x.Autority == name && x.BaseUrl == "http://localhost:53076/").RegistrationData,
                save_authority: (name, registration) =>
                {
                    var reg = RegistrationsDB.Registrations.FirstOrDefault(x => x.Autority == name && x.BaseUrl == "http://localhost:53076/");
                    if (reg == null)
                    {
                        reg = new Id4meClientRegistrations()
                        {
                            Autority = name,
                            BaseUrl = "http://localhost:53076/",
                            RegistrationData = registration
                        };
                        RegistrationsDB.Registrations.Add(reg);
                    }
                    else
                        reg.RegistrationData = registration;
                    RegistrationsDB.SaveChanges();
                },
                app_type: OIDCApplicationType.native);        
        }

        public PathString CallbackPath { get; set; }

        public string SignInAsAuthenticationType { get; set; }

        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }    }
}
