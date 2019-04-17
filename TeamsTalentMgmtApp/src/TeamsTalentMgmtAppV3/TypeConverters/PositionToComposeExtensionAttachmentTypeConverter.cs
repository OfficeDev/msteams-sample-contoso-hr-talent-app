using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.TypeConverters
{
    public class PositionToComposeExtensionAttachmentTypeConverter : ITypeConverter<Position, ComposeExtensionAttachment>
    {
        public ComposeExtensionAttachment Convert(Position position, ComposeExtensionAttachment _, ResolutionContext context)
        {
            var card = context.Mapper.Map<AdaptiveCard>(position);
            var preview = context.Mapper.Map<ThumbnailCard>(position);
            
            return card.ToAttachment().ToComposeExtensionAttachment(preview.ToAttachment());
        }
    }
}