using Fitbit.Api.Portable;
using Fitbit.Api.Portable.OAuth2;
using Fitbit.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using TheCrushinator.FitBit.Web.Constants;
using TheCrushinator.FitBit.Web.Extensions;
using TheCrushinator.FitBit.Web.Models.Options;

namespace TheCrushinator.FitBit.Web.Controllers
{
    public class FitbitController : Controller
    {
        private readonly ILogger<FitbitController> _logger;
        private readonly FitbitClientOptions _fitbitClientOptions;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fitbitClientOptions">Client Options for FitBit</param>
        public FitbitController(ILogger<FitbitController> logger, IOptions<FitbitClientOptions> fitbitClientOptions)
        {
            _logger = logger;
            _fitbitClientOptions = fitbitClientOptions.Value;
        }

        public IActionResult PushBeurerData()
        {
            return RedirectToAction("Authorize");
        }

        /// <summary>
        /// Initiate authorisation with redirect to FitBit.com
        /// </summary>
        /// <returns></returns>
        public IActionResult Authorize()
        {
            var appCredentials = GetAppCredentials();

            //Provide the App Credentials. You get those by registering your app at dev.fitbit.com
            //Configure Fitbit authentication request to perform a callback to this constructor's Callback method
            var authenticator = new OAuth2Helper(appCredentials, GetCallbackUri());
            string[] scopes = new string[] { "profile", "activity", "heartrate", "location", "nutrition", "profile", "settings", "sleep", "social", "weight" };

            string authUrl = authenticator.GenerateAuthUrl(scopes, null);

            return Redirect(authUrl);
        }

        /// <summary>
        /// Save token when it is passed through a callback
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<IActionResult> Callback(string code)
        {
            var appCredentials = GetAppCredentials();

            var authenticator = new OAuth2Helper(appCredentials, GetCallbackUri());

            //string code = Request.Params["code"];

            var accessToken = await authenticator.ExchangeAuthCodeForAccessTokenAsync(code);

            // Fetch a fitbit client, this stores the client in the session for future use
            var fitbitClient = GetFitbitClient(accessToken);
            // Update the user profile as part of the sign in
            await UpdateUserProfile();

            ViewBag.UserProfile = await fitbitClient.GetUserProfileAsync();

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Get Fitbit client with existing or new access token.
        /// ----HttpClient and hence FitbitClient are designed to be long-lived for the duration of the session. This method ensures only one client is created for the duration of the session.
        /// ----More info at: http://stackoverflow.com/questions/22560971/what-is-the-overhead-of-creating-a-new-httpclient-per-call-in-a-webapi-client
        /// </summary>
        /// <returns></returns>
        private FitbitClient GetFitbitClient(OAuth2AccessToken accessToken = null)
        {
            var savedToken = HttpContext.Session.Get<OAuth2AccessToken>(SessionKeys.SessionKeyFitbitToken);

            if (accessToken != null) // Replace access token with new one
            {
                // Save the new access token
                HttpContext.Session.Set(SessionKeys.SessionKeyFitbitToken, accessToken);
                // Return a FitBit client with this token
                return GetSessionFitbitClient(accessToken);
            }
            else if (savedToken == null) // No new token and no existing token
            {
                throw new Exception(
                    "First time requesting a FitbitClient from the session you must pass the AccessToken.");
            }
            else // Use existing token
            {
                return GetSessionFitbitClient(savedToken);
            }
        }

        /// <summary>
        /// Store user profile as part of the same session as the fitbit client
        /// </summary>
        /// <returns></returns>
        private async Task UpdateUserProfile()
        {
            var fitbitClient = GetFitbitClient();

            if (fitbitClient != null)
            {
                var profile = await fitbitClient.GetUserProfileAsync();
                HttpContext.Session.Set(SessionKeys.SessionKeyFitbitProfile, profile);
            }
            else
            {
                throw new Exception("First time requesting a FitbitClient from the session you must pass the AccessToken.");
            }
        }

        /// <summary>
        /// Fetch the UserProfile from the session variable (may be null)
        /// </summary>
        /// <returns></returns>
        private UserProfile GetUserProfile()
        {
            return HttpContext.Session.Get<UserProfile>(SessionKeys.SessionKeyFitbitProfile);
        }

        /// <summary>
        /// Get the Fitbit client for the session
        /// TODO: Improve the efficiency
        /// </summary>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private FitbitClient GetSessionFitbitClient(OAuth2AccessToken accessToken)
        {
            var appCredentials = GetAppCredentials();
            return new FitbitClient(appCredentials, accessToken);
        }

        /// <summary>
        /// Figure out the callback uri from the http request
        /// </summary>
        /// <returns></returns>
        private string GetCallbackUri()
        {
            var requestUri = new Uri(Request.GetDisplayUrl());
            var callbackUri = requestUri.GetLeftPart(UriPartial.Authority) + "/fitbit/Callback";
            return callbackUri;
        }

        /// <summary>
        /// Get the Fitbit app credentials from the application settings
        /// </summary>
        /// <returns></returns>
        private FitbitAppCredentials GetAppCredentials()
        {
            return new FitbitAppCredentials()
            {
                ClientId = _fitbitClientOptions.ClientId,
                ClientSecret = _fitbitClientOptions.ClientSecret
            };
        }
    }
}
