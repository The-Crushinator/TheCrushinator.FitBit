using Fitbit.Api.Portable;
using Fitbit.Api.Portable.OAuth2;
using Fitbit.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using TheCrushinator.FitBit.Web.Constants;
using TheCrushinator.FitBit.Web.Extensions;
using TheCrushinator.FitBit.Web.Models;
using TheCrushinator.FitBit.Web.Models.Options;
using TheCrushinator.FitBit.Web.Services.Interfaces;

namespace TheCrushinator.FitBit.Web.Controllers
{
    public class FitbitController : Controller
    {
        private readonly ILogger<FitbitController> _logger;
        private readonly IBeurerService _beurerService;
        private readonly FitbitContext _context;
        private readonly FitbitClientOptions _fitbitClientOptions;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="fitbitClientOptions">Client Options for FitBit</param>
        /// <param name="beurerService"></param>
        /// <param name="context"></param>
        public FitbitController(ILogger<FitbitController> logger, IOptions<FitbitClientOptions> fitbitClientOptions, IBeurerService beurerService, FitbitContext context)
        {
            _logger = logger;
            _beurerService = beurerService;
            _context = context;
            _fitbitClientOptions = fitbitClientOptions.Value;
        }

        public IActionResult PushBeurerData()
        {
            return RedirectToAction("Authorize");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<IActionResult> LogNextWeightEntry()
        {
            var nextWeight = await _beurerService.GetNextScaleEntry();

            if (nextWeight != null)
            {
                var fitbitClient = GetFitbitClient();
                var localRecordDateTime = nextWeight.RecordDateTimeUtc.ToLocalTime();

                if (fitbitClient != null)
                {
                    _logger.LogInformation($"Beginning submission of {nextWeight.WeightKg} kg and {nextWeight.BodyFatPct} for {localRecordDateTime}...");
                    var weightCheck = await fitbitClient.GetWeightAsync(localRecordDateTime);
                    if (!weightCheck.Weights.Any(x => Math.Abs(x.Weight - nextWeight.WeightKg) < 0.01 && x.DateTime == localRecordDateTime))
                    {
                        var weight = await fitbitClient.LogWeightAsync(nextWeight.WeightKg, localRecordDateTime);
                        nextWeight.FitBitWeightUploadDateTimeUtc = weight.DateTime;
                        nextWeight.FitbitWeightLogId = weight.LogId;
                        await _context.SaveChangesAsync();

                        ViewBag.Weight = weight;
                        _logger.LogInformation("Added weight information");
                    }
                    else
                    {
                        nextWeight.FitBitWeightUploadDateTimeUtc = weightCheck.Weights.First().DateTime;
                        nextWeight.FitbitWeightLogId = weightCheck.Weights.First().LogId;
                        await _context.SaveChangesAsync();
                        _logger.LogWarning("Matching weight entry already exists, setting upload time for local version");
                    }

                    // Body fat (skipping entries that are 0)
                    if (nextWeight.BodyFatPct != 0)
                    {
                        var fatCheck = await fitbitClient.GetFatAsync(localRecordDateTime);
                        if (!fatCheck.FatLogs.Any(x => Math.Abs(x.Fat - nextWeight.BodyFatPct) < 0.01 && x.DateTime == localRecordDateTime))
                        {
                            var fat = await fitbitClient.LogFatAsync(nextWeight.BodyFatPct, localRecordDateTime);
                            nextWeight.FitBitFatUploadDateTimeUtc = fat.DateTime;
                            nextWeight.FitbitFatLogId = fat.LogId;
                            await _context.SaveChangesAsync();

                            ViewBag.Fat = fat;
                            _logger.LogInformation("Added fat information");
                        }
                        else
                        {
                            nextWeight.FitBitFatUploadDateTimeUtc = fatCheck.FatLogs.First().DateTime;
                            nextWeight.FitbitFatLogId = fatCheck.FatLogs.First().LogId;
                            await _context.SaveChangesAsync();
                            _logger.LogWarning("Matching fat entry already exists, setting upload time for local version");
                        }
                    }
                    else
                    {
                        nextWeight.FitBitFatUploadDateTimeUtc = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        _logger.LogWarning("No fat entry defined, adding datetime and skipping");
                    }

                    ViewBag.ScaleEntry = nextWeight;
                }
                else
                {
                    throw new Exception("First time requesting a FitbitClient from the session you must pass the AccessToken.");
                }
            }

            return View();
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
