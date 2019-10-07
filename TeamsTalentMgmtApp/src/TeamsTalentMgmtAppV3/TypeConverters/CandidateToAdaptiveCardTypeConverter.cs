using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Services.Interfaces;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.TypeConverters
{
    public class CandidateToAdaptiveCardTypeConverter : ITypeConverter<Candidate, AdaptiveCard>
    {
        private readonly ITemplateService _templateService;

        public CandidateToAdaptiveCardTypeConverter(ITemplateService templateService)
        {
            _templateService = templateService;
        }
        
        public AdaptiveCard Convert(Candidate candidate, AdaptiveCard card, ResolutionContext context)
        {
            if (candidate is null)
            {
                return null;
            }

            if (card is null)
            {
                card = new AdaptiveCard();
            }

            card.Version = "1.0";
            card.Body = new List<AdaptiveElement>
            {
                new AdaptiveColumnSet
                {
                    Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Width = "auto",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveImage(candidate.ProfilePicture)
                                {
                                    Size = AdaptiveImageSize.Large,
                                    Style = AdaptiveImageStyle.Person
                                }
                            }
                        },
                        new AdaptiveColumn
                        {
                            Width = "stretch",
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock(candidate.Name)
                                {
                                    Weight = AdaptiveTextWeight.Bolder,
                                    Size = AdaptiveTextSize.Medium
                                },
                                new AdaptiveTextBlock(candidate.Summary)
                                {
                                    Wrap = true
                                }
                            }
                        }
                    }
                },
                new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact("Current role:", candidate.CurrentRole),
                        new AdaptiveFact("Location:", candidate.Location?.LocationAddress ?? string.Empty),
                        new AdaptiveFact("Stage:", candidate.Stage.ToString()),
                        new AdaptiveFact("Position applied:", candidate.Position.Title),
                        new AdaptiveFact("Date applied:", candidate.DateApplied.ToLongDateString()),
                        new AdaptiveFact("Phone number:", candidate.Phone)
                    }
                }
            };

            card.Actions = new List<AdaptiveAction>();

            if (candidate.Stage != InterviewStageType.Offered)
            {
                card.Actions.Add(new AdaptiveShowCardAction
                {
                    Title = "Schedule an interview",
                    Card = _templateService.GetAdaptiveCardForInterviewRequest(candidate, DateTime.Now.Date.AddDays(1.0))
                });
            }

            if (candidate.Comments.Any() || candidate.Interviews.Any())
            {
                var contentUrl = ConfigurationManager.AppSettings["BaseUrl"] + $"/StaticViews/CandidateFeedback.html?{Uri.EscapeDataString($"candidateId={candidate.CandidateId}")}";
                card.Actions.Add(new AdaptiveOpenUrlAction
                {
                    Title = "Open candidate feedback",
                    Url = new Uri(string.Format(CommonConstants.TaskModuleUrlFormat, ConfigurationManager.AppSettings["TeamsAppId"],
                        contentUrl, "Feedback for " + candidate.Name,
                        ConfigurationManager.AppSettings["MicrosoftAppId"], "large", "large"))
                });
            }

            var leaveCommentCommand = new
            {
                commandId = AppCommands.LeaveInternalComment,
                candidateId = candidate.CandidateId
            };
            
            var wrapAction = new CardAction
            {
                Title = "Submit",
                Value = leaveCommentCommand
            };

            var action = new AdaptiveSubmitAction
            {
                Data = leaveCommentCommand
            };

            action.RepresentAsBotBuilderAction(wrapAction);

            card.Actions.Add(new AdaptiveShowCardAction
            {
                Title = "Leave comment",
                Card = new AdaptiveCard
                {
                    Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextInput
                        {
                            Id = "comment",
                            Placeholder = "Leave an internal comment for this candidate",
                            IsMultiline = true
                        }
                    },
                    Actions = new List<AdaptiveAction>
                    {
                        action
                    }
                }
            });

            return card;
        }
    }
}
