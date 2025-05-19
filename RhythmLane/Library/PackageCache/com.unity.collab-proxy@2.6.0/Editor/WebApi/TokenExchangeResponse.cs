using System.ComponentModel;
using Unity.Plastic.Newtonsoft.Json;
using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    /// <summary>
    /// Response to token exchange request.
    /// Internal usage. This isn't a public API.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TokenExchangeResponse
    {
        /// <summary>
        /// Error caused by the request.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("error")]
        public ErrorResponse.ErrorFields Error { get; set; }

        /// <summary>
        /// The user's username.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("user")]
        public string User { get; set; }

        /// <summary>
        /// The access token.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        /// <summary>
        /// The refresh token.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
