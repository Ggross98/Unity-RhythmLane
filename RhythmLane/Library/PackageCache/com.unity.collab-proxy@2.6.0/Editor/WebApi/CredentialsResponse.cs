using System.ComponentModel;
using Unity.Plastic.Newtonsoft.Json;

using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    /// <summary>
    /// Response to credentials request.
    /// Internal usage. This isn't a public API.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class CredentialsResponse
    {
        /// <summary>
        /// Error caused by the request.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("error")]
        public ErrorResponse.ErrorFields Error { get; set; }

        /// <summary>
        /// Type of the token.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public enum TokenType : int
        {
            /// <summary>
            /// Password token.
            /// </summary>
            Password = 0,

            /// <summary>
            /// Bearer token.
            /// </summary>
            Bearer = 1,
        }

        /// <summary>
        /// Get the type of the token.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonIgnore]
        public TokenType Type
        {
            get { return (TokenType)TokenTypeValue; }
        }

        /// <summary>
        /// The user's email.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("email")]
        public string Email;

        /// <summary>
        /// The credential's token.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("token")]
        public string Token;

        /// <summary>
        /// The token type represented as an integer.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("tokenTypeValue")]
        public int TokenTypeValue;
    }
}
