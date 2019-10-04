using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace TeamsTalentMgmtAppV4.Extensions
{
    public static class TeamsExtensions
    {
        public static string GetTextWithoutCommand(this ITurnContext context, string commandMatch)
        {
            var teamsContext = context.TurnState.Get<ITeamsContext>();
            var query = teamsContext?.GetActivityTextWithoutMentions()?.Trim() ?? string.Empty;

            var regex = new Regex(commandMatch, RegexOptions.IgnoreCase);
            if (regex.Match(query).Success || query.IndexOf(commandMatch, StringComparison.OrdinalIgnoreCase) == 0)
            {
                query = regex.Replace(query, string.Empty, 1);
            }

            return query.Trim();
        }

        public static bool HasFileAttachments(this IMessageActivity activity)
        {
            return activity.Attachments != null && activity.Attachments.Any(x => string.Equals(x.ContentType, FileDownloadInfo.ContentType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
