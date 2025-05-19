using System.ComponentModel;

using Unity.Plastic.Newtonsoft.Json;

using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    /// <summary>
    /// Response to current user admin check request.
    /// Internal usage. This isn't a public API.
    /// </summary>
    public class CurrentUserAdminCheckResponse
    {
        /// <summary>
        /// Error caused by the request.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("error")]
        public ErrorResponse.ErrorFields Error { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("isCurrentUserAdmin")]
        public bool IsCurrentUserAdmin { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("organizationName")]
        public string OrganizationName { get; set; }
    }
}
