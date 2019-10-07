using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Services.Interfaces;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Services
{
    public sealed class TemplateService : ITemplateService
    {
        private readonly ILocationService _locationService;
        private readonly IRecruiterService _recruiterService;

        public TemplateService(ILocationService locationService,
            IRecruiterService recruiterService)
        {
            _locationService = locationService;
            _recruiterService = recruiterService;
        }

        public AdaptiveCard GetAdaptiveCardForNewJobPosting(string description = null)
        {
            var locations = _locationService.GetAllLocations().GetAwaiter().GetResult();
            var hiringManagers = _recruiterService.GetAllHiringManagers().GetAwaiter().GetResult();

            var command = new
            {
                commandId = AppCommands.OpenNewPosition
            };
            
            var wrapAction = new CardAction
            {
                Title = "Create posting",
                Value = command
            };

            var action = new AdaptiveSubmitAction
            {
                Data = command
            };

            action.RepresentAsBotBuilderAction(wrapAction);

            return new AdaptiveCard
            {
                Version = "1.0",
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock("Enter in basic information about this posting")
                    {
                        IsSubtle = true,
                        Wrap = true,
                        Size = AdaptiveTextSize.Small
                    },
                    new AdaptiveTextBlock("Title")
                    {
                        Wrap = true
                    },
                    new AdaptiveTextInput
                    {
                        Id = "jobTitle",
                        Placeholder = "E.g. Senior PM"
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock("Level") {Wrap = true},
                                    new AdaptiveChoiceSetInput
                                    {
                                        Id = "jobLevel",
                                        Style = AdaptiveChoiceInputStyle.Compact,
                                        Choices = Enumerable.Range(7, 4).Select(x =>
                                        {
                                            var s = x.ToString();
                                            return new AdaptiveChoice
                                            {
                                                Title = s,
                                                Value = s
                                            };
                                        }).ToList(),
                                        Value = "7"
                                    }
                                }
                            },

                            new AdaptiveColumn
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock("Post by")
                                    {
                                        Wrap = true
                                    },
                                    new AdaptiveDateInput
                                    {
                                        Id = "jobPostingDate",
                                        Placeholder = "Posting date",
                                        Value = DateTime.Now.ToShortDateString()
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock("Location"),
                                    new AdaptiveChoiceSetInput
                                    {
                                        Id = "jobLocation",
                                        Style = AdaptiveChoiceInputStyle.Compact,
                                        Choices = locations.Select(x => new AdaptiveChoice
                                        {
                                            Value = x.LocationId.ToString(),
                                            Title = x.City
                                        }).ToList(),
                                        Value = Convert.ToString(locations[0].LocationId)
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock("Hiring manager"),
                                    new AdaptiveChoiceSetInput
                                    {
                                        Id = "jobHiringManager",
                                        Style = AdaptiveChoiceInputStyle.Compact,
                                        Choices = hiringManagers.Select(x => new AdaptiveChoice
                                        {
                                            Value = x.RecruiterId.ToString(),
                                            Title = x.Name
                                        }).ToList(),
                                        Value = Convert.ToString(hiringManagers[0].RecruiterId)
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveTextBlock("Description"),
                    new AdaptiveTextInput
                    {
                        Id = "jobDescription",
                        IsMultiline = true,
                        Placeholder = "E.g. Senior Product Manager in charge of driving complicated work and stuff.",
                        Value = description
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    action
                }
            };
        }

        public AdaptiveCard GetAdaptiveCardForInterviewRequest(Candidate candidate, DateTime interviewDate)
        {
            var interviewers = _recruiterService.GetAllInterviewers().GetAwaiter().GetResult();
            
            var command = new
            {
                commandId = AppCommands.ScheduleInterview,
                candidateId = candidate.CandidateId
            };
            
            var wrapAction = new CardAction
            {
                Title = "Schedule",
                Value = command
            };

            var action = new AdaptiveSubmitAction
            {
                Data = command
            };

            action.RepresentAsBotBuilderAction(wrapAction);
            
            return new AdaptiveCard
            {
                Version = "1.0",
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = $"Set interview date for {candidate.Name}",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Large
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(candidate.ProfilePicture),
                                        Size = AdaptiveImageSize.Medium,
                                        Style = AdaptiveImageStyle.Person
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"Position: {candidate.Position.Title}",
                                        Wrap = true
                                    },
                                    new AdaptiveTextBlock
                                    {
                                        Text = $"Position ID: {candidate.Position.PositionExternalId}",
                                        Spacing = AdaptiveSpacing.None,
                                        Wrap = true,
                                        IsSubtle = true
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = "interviewerId",
                        Style = AdaptiveChoiceInputStyle.Compact,
                        Choices = interviewers.Select(x => new AdaptiveChoice
                        {
                            Value = x.RecruiterId.ToString(),
                            Title = x.Name
                        }).ToList(),
                        Value = Convert.ToString(interviewers[0].RecruiterId)
                    },
                    new AdaptiveDateInput
                    {
                        Id = "interviewDate", Placeholder = "Enter in a date for the interview", Value = interviewDate.ToShortDateString()
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = "interviewType",
                        Style = AdaptiveChoiceInputStyle.Compact,
                        IsMultiSelect = false,
                        Choices = new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice {Title = "Phone screen", Value = "phoneScreen"},
                            new AdaptiveChoice {Title = "Full loop", Value = "fullLoop"},
                            new AdaptiveChoice {Title = "Follow-up", Value = "followUp"}
                        },
                        Value = "phoneScreen"
                    },
                    new AdaptiveToggleInput {Id = "isRemote", Title = "Remote interview"}
                },
                Actions = new List<AdaptiveAction>
                {
                    action
                }
            };
        }
    }
}