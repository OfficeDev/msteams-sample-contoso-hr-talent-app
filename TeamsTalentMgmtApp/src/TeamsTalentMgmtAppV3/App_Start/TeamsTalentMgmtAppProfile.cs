using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams.Models;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamsTalentMgmtAppV3.Services.Interfaces;
using TeamsTalentMgmtAppV3.TypeConverters;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3
{
    public sealed class TeamsTalentMgmtAppProfile : Profile
    {
        public TeamsTalentMgmtAppProfile(ITemplateService templateService)
        {
            CreateMap<Candidate, AdaptiveCard>().ConvertUsing(new CandidateToAdaptiveCardTypeConverter(templateService));
            CreateMap<Candidate, ThumbnailCard>().ConvertUsing(new CandidateToThumbnailCardTypeConverter());
            CreateMap<Candidate, ComposeExtensionAttachment>().ConvertUsing(new CandidateToComposeExtensionAttachmentTypeConverter());
            CreateMap<Candidate, CardListItem>().ConvertUsing(new CandidateToCardListItemTypeConverter());
            CreateMap<Candidate, O365ConnectorCard>().ConvertUsing(new CandidateToO365CardTypeConverter());

            CreateMap<Position, AdaptiveCard>().ConvertUsing(new PositionToAdaptiveCardTypeConverter());
            CreateMap<Position, ThumbnailCard>().ConvertUsing(new PositionToThumbnailCardTypeConverter());
            CreateMap<Position, ComposeExtensionAttachment>().ConvertUsing(new PositionToComposeExtensionAttachmentTypeConverter());
            CreateMap<Position, CardListItem>().ConvertUsing(new PositionToCardListItemTypeConverter());
        }
    }
}