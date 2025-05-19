using System.ComponentModel;
using Unity.Plastic.Newtonsoft.Json;
using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    // Internal usage. This isn't a public API.
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsCollabProjectMigratedResponse
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("error")]
        public ErrorResponse.ErrorFields Error { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("IsMigrated")]
        public bool IsMigrated { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("WebServerUri")]
        public string WebServerUri { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("PlasticCloudOrganizationName")]
        public string PlasticCloudOrganizationName { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("Credentials")]
        public CredentialsResponse Credentials { get; set; }
    }
}
