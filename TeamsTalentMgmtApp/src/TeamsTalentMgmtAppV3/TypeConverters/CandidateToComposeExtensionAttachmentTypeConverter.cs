using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using TeamsTalentMgmtAppV3.Extensions;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.TypeConverters
{
    public class CandidateToComposeExtensionAttachmentTypeConverter : ITypeConverter<Candidate, ComposeExtensionAttachment>
    {
        public ComposeExtensionAttachment Convert(Candidate candidate, ComposeExtensionAttachment attachment, ResolutionContext context)
        {
            var card = context.Mapper.Map<AdaptiveCard>(candidate);
            var preview = context.Mapper.Map<ThumbnailCard>(candidate);

            return card.ToAttachment().ToComposeExtensionAttachment(preview.ToAttachment());
        }
    }
}