namespace TheCrushinator.FitBit.Web.Models.Options
{
    /// <summary>
    /// Fitbit client configuration parameters
    /// </summary>
    public class FitbitClientOptions
    {
        /// <summary>
        /// OAuth2 ClientId
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// OAuth2 Client Secret
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
