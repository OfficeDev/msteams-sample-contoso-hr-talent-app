using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.TaskModule
{
    public class TaskModuleTaskInfo
    {
        public TaskModuleTaskInfo(string title = default, object height = default, object width = default, string url = default, Attachment card = default, string fallbackUrl = default, string completionBotId = default)
        {
            Title = title;
            Height = height;
            Width = width;
            Url = url;
            Card = card;
            FallbackUrl = fallbackUrl;
            CompletionBotId = completionBotId;
        }

        /// <summary>
        /// Gets or sets appears below the app name and to the right of the
        /// app icon.
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets this can be a number, representing the task module's
        /// height in pixels, or a string, one of: small, medium, large.
        /// </summary>
        [JsonProperty(PropertyName = "height")]
        public object Height { get; set; }

        /// <summary>
        /// Gets or sets this can be a number, representing the task module's
        /// width in pixels, or a string, one of: small, medium, large.
        /// </summary>
        [JsonProperty(PropertyName = "width")]
        public object Width { get; set; }

        /// <summary>
        /// Gets or sets the URL of what is loaded as an iframe inside the
        /// task module. One of url or card is required.
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the JSON for the Adaptive card to appear in the task
        /// module.
        /// </summary>
        [JsonProperty(PropertyName = "card")]
        public Attachment Card { get; set; }

        /// <summary>
        /// Gets or sets if a client does not support the task module feature,
        /// this URL is opened in a browser tab.
        /// </summary>
        [JsonProperty(PropertyName = "fallbackUrl")]
        public string FallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets if a client does not support the task module feature,
        /// this URL is opened in a browser tab.
        /// </summary>
        [JsonProperty(PropertyName = "completionBotId")]
        public string CompletionBotId { get; set; }

    }
}