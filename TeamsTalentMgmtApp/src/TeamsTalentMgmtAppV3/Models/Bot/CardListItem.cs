using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace TeamsTalentMgmtAppV3.Models.Bot
{
	public class CardListItem
	{
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[JsonProperty(PropertyName = "icon")]
		public string Icon { get; set; }

		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }

		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "subtitle")]
		public string Subtitle { get; set; }

		[JsonProperty(PropertyName = "tap")]
		public CardAction Tap { get; set; }
	}
}
