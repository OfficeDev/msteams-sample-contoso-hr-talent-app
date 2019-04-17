using AdaptiveCards;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeamsTalentMgmtAppV3.Extensions
{
    public static class CardExtensions
    {
        public static void RepresentAsBotBuilderAction(this AdaptiveSubmitAction action, CardAction targetAction)
        {
            var wrappedAction = new CardAction
            {
                Type = targetAction.Type,
                Value = targetAction.Value,
                Text = targetAction.Text,
                DisplayText = targetAction.DisplayText
            };

            var serializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};

            var jsonStr = action.DataJson ?? "{}";
            JToken dataJson = JObject.Parse(jsonStr);
            dataJson["msteams"] = JObject.FromObject(wrappedAction, JsonSerializer.Create(serializerSettings));

            action.Title = targetAction.Title;
            action.DataJson = dataJson.ToString();
        }
    }
}
