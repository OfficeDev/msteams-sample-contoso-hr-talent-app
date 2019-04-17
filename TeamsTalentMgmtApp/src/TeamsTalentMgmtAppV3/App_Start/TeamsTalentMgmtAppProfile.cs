using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams.Models;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamsTalentMgmtAppV3.Models.Commands;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Models.Dto;
using TeamsTalentMgmtAppV3.Services.Interfaces;
using TeamsTalentMgmtAppV3.TypeConverters;

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
            
            CreateMap<PositionCreateCommand, Position>()
                .ForMember(d => d.Title, opt => opt.MapFrom(s => s.JobTitle))
                .ForMember(d => d.Level, opt => opt.MapFrom(s => s.JobLevel))
                .ForMember(d => d.LocationId, opt => opt.MapFrom(s => s.JobLocation))
                .ForMember(d => d.HiringManagerId, opt => opt.MapFrom(s => s.JobHiringManager))
                .ForMember(d => d.PositionId, opt => opt.MapFrom(s => s.PositionId))
                .ForMember(d => d.Description, opt => opt.MapFrom(s => s.JobDescription))
                ;

            CreateMap<Location, LocationDto>()
                .ForMember(d => d.City, opt => opt.MapFrom(s => s.City))
                .ForMember(d => d.State, opt => opt.MapFrom(s => s.State))
                ;

            CreateMap<Comment, CommentDto>()
                .ForMember(d => d.AuthorName, opt => opt.MapFrom(s => s.AuthorName))
                .ForMember(d => d.AuthorRole, opt => opt.MapFrom(s => s.AuthorRole))
                .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Text))
                ;

            CreateMap<Interview, InterviewDto>()
                .ForMember(d => d.CandidateId, opt => opt.MapFrom(s => s.CandidateId))
                .ForMember(d => d.RecruiterId, opt => opt.MapFrom(s => s.RecruiterId))
                .ForMember(d => d.FeedbackText, opt => opt.MapFrom(s => s.FeedbackText))
                .ForMember(d => d.InterviewDate, opt => opt.MapFrom(s => s.InterviewDate))
                .ForMember(d => d.InterviewId, opt => opt.MapFrom(s => s.InterviewId))
                .ForMember(d => d.Recruiter, opt => opt.MapFrom(s => s.Recruiter))
                ;

            CreateMap<Recruiter, RecruiterDto>()
                .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
                .ForMember(d => d.ProfilePicture, opt => opt.MapFrom(s => s.ProfilePicture))
                .ForMember(d => d.RecruiterId, opt => opt.MapFrom(s => s.RecruiterId))
                ;

            CreateMap<Candidate, CandidateDto>()
               .ForMember(d => d.CandidateId, opt => opt.MapFrom(s => s.CandidateId))
               .ForMember(d => d.CurrentRole, opt => opt.MapFrom(s => s.CurrentRole))
               .ForMember(d => d.Location, opt => opt.MapFrom(s => s.Location))
               .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
               .ForMember(d => d.PositionId, opt => opt.MapFrom(s => s.Position.PositionId))
               .ForMember(d => d.ProfilePicture, opt => opt.MapFrom(s => s.ProfilePicture))
               .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone))
               .ForMember(d => d.Stage, opt => opt.MapFrom(s => s.Stage))
               .ForMember(d => d.PositionTitle, opt => opt.MapFrom(s => s.Position.Title))
               .ForMember(d => d.Interviews, opt => opt.MapFrom(s => s.Interviews))
               .ForMember(d => d.Comments, opt => opt.MapFrom(s => s.Comments))
               ;

            CreateMap<Position, PositionDto>()
               .ForMember(d => d.Candidates, opt => opt.MapFrom(s => s.Candidates))
               .ForMember(d => d.DaysOpen, opt => opt.MapFrom(s => s.DaysOpen))
               .ForMember(d => d.Location, opt => opt.MapFrom(s => s.Location))
               .ForMember(d => d.PositionExternalId, opt => opt.MapFrom(s => s.PositionExternalId))
               .ForMember(d => d.PositionId, opt => opt.MapFrom(s => s.PositionId))
               .ForMember(d => d.HiringManager, opt => opt.MapFrom(s => s.HiringManager))
               .ForMember(d => d.FullDescription, opt => opt.MapFrom(s => s.Description))
               .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
               ;
        }
    }
}