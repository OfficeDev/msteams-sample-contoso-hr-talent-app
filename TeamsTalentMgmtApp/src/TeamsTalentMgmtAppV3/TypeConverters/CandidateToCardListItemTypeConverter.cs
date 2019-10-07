using AutoMapper;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.Bot;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.TypeConverters
{
    public class CandidateToCardListItemTypeConverter : ITypeConverter<Candidate, CardListItem>
    {
        public CardListItem Convert(Candidate candidate, CardListItem cardListItem, ResolutionContext context)
        {
            if (candidate is null)
            {
                return null;
            }

            if (cardListItem is null)
            {
                cardListItem = new CardListItem();
            }

            var botCommand = BotCommands.CandidateDetailsDialogCommand;
            if (context.Items.TryGetValue("botCommand", out var botCommandValue))
            {
                botCommand = botCommandValue.ToString();
            }
            
            cardListItem.Icon = candidate.ProfilePicture;
            cardListItem.Type = CardListItemTypes.ResultItem;
            cardListItem.Title = $"<b>{candidate.Name}</b>";
            cardListItem.Subtitle = $"Current role: {candidate.CurrentRole} | Stage: {candidate.Stage.ToString()} | {candidate.Location?.LocationAddress ?? string.Empty}";
            cardListItem.Tap = new CardAction(ActionTypes.ImBack, value: $"{botCommand} {candidate.Name}");

            return cardListItem;
        }
    }
}