using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.MicrosoftGraph
{
    public sealed class TeamFunSettings
    {
        [JsonProperty("allowGiphy")]
        public bool AllowGiphy { get; set; }

        [JsonProperty("giphyContentRating")]
        public string GiphyContentRating { get; set; }
    }
}