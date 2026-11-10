namespace RiaMoneyTransferAPI.Model
{
    /// <summary>
    /// Configuration for the Ria Partner / Wholesale API (rialink.net).
    /// Bound from the "RiaPartnerApi" configuration section. Credentials should
    /// be supplied via user-secrets, environment variables, or a secret store —
    /// never committed to source control.
    /// </summary>
    public class PartnerApiOptions
    {
        public const string SectionName = "RiaPartnerApi";

        /// <summary>
        /// Base URL of the partner API, e.g. https://stagingapi.rialink.net
        /// </summary>
        public string BaseUrl { get; set; } = "https://stagingapi.rialink.net";

        /// <summary>
        /// Value for the <c>ria-AgentId</c> header.
        /// </summary>
        public string AgentId { get; set; } = string.Empty;

        /// <summary>
        /// Value for the <c>Ocp-Apim-Subscription-Key</c> header.
        /// </summary>
        public string SubscriptionKey { get; set; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(BaseUrl) &&
            !string.IsNullOrWhiteSpace(AgentId) &&
            !string.IsNullOrWhiteSpace(SubscriptionKey);
    }
}
