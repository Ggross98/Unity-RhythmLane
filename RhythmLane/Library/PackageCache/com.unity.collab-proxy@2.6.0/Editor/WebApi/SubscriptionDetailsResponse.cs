using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Plastic.Newtonsoft.Json;

namespace Unity.PlasticSCM.Editor.WebApi
{
    // Internal usage. This isn't a public API.
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SubscriptionDetailsResponse
    {
        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("subscriptionType")]
        public string ProductType { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("subscriptionSource")]
        public string OrderSource { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("genesisOrgId")]
        public string GenesisOrgId { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("plasticOrganizationName")]
        public string OrganizationName { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("readonlyStatus")]
        public string ReadonlyStatus { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("isAdmin")]
        public bool IsAdmin { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("isOwner")]
        public bool IsOwner { get; set; }

        // Internal usage. This isn't a public API.
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("extraData")]
        public Dictionary<string, object> ExtraData { get; set; }
    }
}
