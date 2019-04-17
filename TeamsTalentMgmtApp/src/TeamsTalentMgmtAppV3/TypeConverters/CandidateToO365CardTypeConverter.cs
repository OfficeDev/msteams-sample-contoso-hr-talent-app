using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AutoMapper;
using Microsoft.Bot.Connector.Teams.Models;
using TeamsTalentMgmtAppV3.Constants;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Models.Extensions;

namespace TeamsTalentMgmtAppV3.TypeConverters
{
    public class CandidateToO365CardTypeConverter : ITypeConverter<Candidate, O365ConnectorCard>
    {
        
        public O365ConnectorCard Convert(Candidate candidate, O365ConnectorCard destination, ResolutionContext context)
        {
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            var configurationCompletedCard = new O365ConnectorCardSection(
                    activityTitle: $"Stage has been changed for {candidate.Name}",
                    activityImage: $"{baseUrl}/api/candidates/{candidate.CandidateId}/profilePicture",
                    facts: new List<O365ConnectorCardFact>
                    {
                        new O365ConnectorCardFact("Stage:", $"**{candidate.PreviousStage.ToString()}** --> **{candidate.Stage.ToString()}**"),
                        new O365ConnectorCardFact("Current role:", candidate.CurrentRole),
                        new O365ConnectorCardFact("Location:", candidate.Location.GetLocationString()),
                        new O365ConnectorCardFact("Position applied:", candidate.Position.Title),
                        new O365ConnectorCardFact("Phone number:", candidate.Phone)
                    },
                    markdown: true
                );

            if (candidate.Comments.Any() || candidate.Interviews.Any())
            {
                var contentUrl = $"{baseUrl}/StaticViews/CandidateFeedback.html?candidateId={candidate.CandidateId}";
                
                var openFeedback = new Uri(string.Format(CommonConstants.TaskModuleUrlFormat, ConfigurationManager.AppSettings["TeamsAppId"],
                            contentUrl, "Feedback for " + candidate.Name,
                            ConfigurationManager.AppSettings["MicrosoftAppId"], "large", "large"));
                
                configurationCompletedCard.PotentialAction = new List<O365ConnectorCardActionBase> {
                    new O365ConnectorCardViewAction(O365ConnectorCardViewAction.Type)
                    {
                        Name = "Open candidate feedback",
                        Target = new List<string> { openFeedback.ToString() }
                    }
                };
            }

            var card = new O365ConnectorCard
            {
                Sections = new List<O365ConnectorCardSection> { configurationCompletedCard },
                Summary = $"Stage has been changed for {candidate.Name}"
            };

            return card;
        }
    }
}