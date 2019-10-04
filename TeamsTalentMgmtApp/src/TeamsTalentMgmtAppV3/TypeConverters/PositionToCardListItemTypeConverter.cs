using AutoMapper;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.Bot;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.TypeConverters
{
    public class PositionToCardListItemTypeConverter : ITypeConverter<Position, CardListItem>
    {
        public CardListItem Convert(Position position, CardListItem cardListItem, ResolutionContext context)
        {
            if (position is null)
            {
                return null;
            }

            if (cardListItem is null)
            {
                cardListItem = new CardListItem();
            }

            var botCommand = BotCommands.PositionsDetailsDialogCommand;
            if (context.Items.TryGetValue("botCommand", out var botCommandValue))
            {
                botCommand = botCommandValue.ToString();
            }
            
            cardListItem.Icon = position.HiringManager.ProfilePicture;
            cardListItem.Type = CardListItemTypes.ResultItem;
            cardListItem.Title = $"<b>{position.PositionId} - {position.Title}</b>";
            cardListItem.Subtitle = $"Applicants: {position.Candidates.Count} | Hiring manager: {position.HiringManager.Name} | Days open: {position.DaysOpen}";
            cardListItem.Tap = new CardAction(ActionTypes.ImBack, value: $"{botCommand} {position.PositionExternalId}");

            return cardListItem;
        }
    }
}