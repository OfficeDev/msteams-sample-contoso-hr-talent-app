using System;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;

namespace TeamsTalentMgmtAppV3.Extensions
{
	/// <summary>
	/// Helps with some activity processing.
	/// </summary>
	public static class TeamsExtensions
	{
		private const string ComposeExtensionInvokeNameOfFetchTask = "composeExtension/fetchTask";

		private const string ComposeExtensionInvokeNameOfSubmitAction = "composeExtension/submitAction";
		
		private const string ComposeExtensionInvokeNameOfCardButtonClickEvent = "composeExtension/onCardButtonClicked";

		public static Attachment ToAttachment(this AdaptiveCard card)
		{
			var attachment = new Attachment
			{
				ContentType = AdaptiveCard.ContentType,
				Content = card
			};
			return attachment;
		}

		public static bool IsFetchTask(this Activity activity)
		{
			return string.Equals(activity.Name,ComposeExtensionInvokeNameOfFetchTask, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsSubmitAction(this Activity activity)
		{
			return string.Equals(activity.Name,ComposeExtensionInvokeNameOfSubmitAction, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsCardButtonClickEvent(this Activity activity)
		{
			return string.Equals(activity.Name,ComposeExtensionInvokeNameOfCardButtonClickEvent, StringComparison.OrdinalIgnoreCase);
		}

        public static bool IsFileConsentCardResponse(this Activity activity)
		{
			return string.Equals(activity.Name,FileConsentCardResponse.InvokeName, StringComparison.OrdinalIgnoreCase);
		}

		public static bool HasFileAttachments(this Activity activity)
		{
			return activity.Attachments != null && activity.Attachments.Any(x => string.Equals(x.ContentType, FileDownloadInfo.ContentType, StringComparison.OrdinalIgnoreCase));
		}
		
		public static void SendTypingActivity(this Activity activity)
		{
			var client = new ConnectorClient(new Uri(activity.ServiceUrl));
			var isTypingReply = activity.CreateReply();
			isTypingReply.Type = ActivityTypes.Typing;
			client.Conversations.ReplyToActivityAsync(isTypingReply);
		}
		
		public static string GetTextWithoutCommand(this IActivity activity, string commandMatch)
		{
			var query = string.Empty;
			if (activity is Activity act && act.Text.HasValue())
			{
				query = act.GetTextWithoutMentions();
			}

			if (query.Contains(commandMatch))
			{
				query = query.Replace(commandMatch, string.Empty);
			}
            
			return query.NormalizeUtterance();
		}
		
		public static bool IsAdaptiveCardActionQuery(this Activity activity)
		{
			return activity.Type == ActivityTypes.Message &&
			       string.IsNullOrEmpty(activity.Name) &&
			       string.IsNullOrEmpty(activity.Text) &&
			       activity.Value != null;
		}
	}
}