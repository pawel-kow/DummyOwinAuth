using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using org.id4me;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Id4meOwinAuth
{
    // Created by the factory in the Id4meAuthenticationMiddleware class.
    class Id4meAuthenticationHandler : AuthenticationHandler<Id4meAuthenticationOptions>
    {
        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            var properties = Options.StateDataFormat.Unprotect(Request.Query["state"]);

            if (!string.IsNullOrEmpty(Request.Query["code"]) && properties.Dictionary.ContainsKey("id4me.ctx"))
            {
                var client = Options.getClient(Request);

                var ctx = ID4meContext.Deserialize(properties.Dictionary["id4me.ctx"]);
                var idtoken = client.get_idtoken(ctx, Request.Query["code"]);
                var user_info = client.get_user_info(ctx);
                // ASP.Net Identity requires the NameIdentitifer field to be set or it won't  
                // accept the external login (AuthenticationManagerExtensions.GetExternalLoginInfo)
                var identity = new ClaimsIdentity(Options.SignInAsAuthenticationType);
                var unique_id = idtoken["iss"] + "#" + idtoken["sub"];
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, unique_id, null, Options.AuthenticationType));
                if (user_info.ContainsKey(OIDCClaimName.preferred_username))
                    identity.AddClaim(new Claim(ClaimTypes.Name, (string)user_info[OIDCClaimName.preferred_username]));
                else
                    identity.AddClaim(new Claim(ClaimTypes.Name, ctx.id));
                if (user_info.ContainsKey(OIDCClaimName.given_name))
                    identity.AddClaim(new Claim(ClaimTypes.GivenName, (string)user_info[OIDCClaimName.given_name]));
                if (user_info.ContainsKey(OIDCClaimName.family_name))
                    identity.AddClaim(new Claim(ClaimTypes.Surname, (string)user_info[OIDCClaimName.family_name]));
                if (user_info.ContainsKey(OIDCClaimName.email))
                    identity.AddClaim(new Claim(ClaimTypes.Email, (string)user_info[OIDCClaimName.email]));

                return Task.FromResult(new AuthenticationTicket(identity, properties));
            }
            else
                // TODO: some real handling
                throw new AccessViolationException("Cannot log in, code not there");
        }

        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode == 401)
            {
                var challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

                // Only react to 401 if there is an authentication challenge for the authentication 
                // type of this handler.
                if (challenge != null)
                {
                    var state = challenge.Properties;

                    if (string.IsNullOrEmpty(state.RedirectUri))
                    {
                        state.RedirectUri = Request.Uri.ToString();
                    }

                    var id = challenge.Properties.Dictionary["user_id"];
                    var ctx = Options.getClient(Request).get_rp_context(id);
                    ctx.GenerateNewNonce();
                    state.Dictionary["id4me.ctx"] = ctx.Serialize();

                    var stateString = Options.StateDataFormat.Protect(state);

                    var url = Options.getClient(Request).get_consent_url(
                        context: ctx, useNonce: true, useNonceFromContext: true, state: stateString,
                        claimsrequest: new ID4meClaimsRequest()
                        {
                            userinfo = new Dictionary<string, OIDCClaimRequestOptions>
                            {
                                { OIDCClaimName.given_name, null },
                                { OIDCClaimName.family_name, null },
                                { OIDCClaimName.email, new OIDCClaimRequestOptions() { essential = true, reason = "To assign E-mail" } },
                                { OIDCClaimName.preferred_username, null }
                            }
                        });

                    Response.Redirect(url);
                }
            }

            return Task.FromResult<object>(null);
        }

        public override async Task<bool> InvokeAsync()
        {
            // This is always invoked on each request. For passive middleware, only do anything if this is
            // for our callback path when the user is redirected back from the authentication provider.
            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
            {
                var ticket = await AuthenticateAsync();

                if (ticket != null)
                {
                    Context.Authentication.SignIn(ticket.Properties, ticket.Identity);

                    Response.Redirect(ticket.Properties.RedirectUri);

                    // Prevent further processing by the owin pipeline.
                    return true;
                }
            }
            // Let the rest of the pipeline run.
            return false;
        }
    }
}
