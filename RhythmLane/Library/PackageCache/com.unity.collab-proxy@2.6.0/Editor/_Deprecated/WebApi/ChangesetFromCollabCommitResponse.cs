using System.ComponentModel;
using Unity.Plastic.Newtonsoft.Json;
using PlasticGui.WebApi.Responses;

namespace Unity.PlasticSCM.Editor.WebApi
{
    // Internal usage. This isn't a public API.
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ChangesetFromCollabCommitResponse
    {
        /// <summary>
        /// Error caused by the request.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("error")]
        public ErrorResponse.ErrorFields Error { get; set; }

        /// <summary>
        /// The repository ID
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("repId")]
        public uint RepId { get; set; }

        /// <summary>
        /// The repository module ID
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("repModuleId")]
        public uint RepModuleId { get; set; }

        /// <summary>
        /// The changeset ID
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("changesetId")]
        public long ChangesetId { get; set; }

        /// <summary>
        /// The branch ID
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [JsonProperty("branchId")]
        public long BranchId { get; set; }
    }
}
