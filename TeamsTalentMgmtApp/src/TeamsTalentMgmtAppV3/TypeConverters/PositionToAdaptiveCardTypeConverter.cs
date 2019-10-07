using System.Collections.Generic;
using AdaptiveCards;
using AutoMapper;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;

namespace TeamsTalentMgmtAppV3.TypeConverters
{
    public class PositionToAdaptiveCardTypeConverter : ITypeConverter<Position, AdaptiveCard>
    {
        public AdaptiveCard Convert(Position position, AdaptiveCard card, ResolutionContext context)
        {
            if (position is null)
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
                new AdaptiveTextBlock(position.Title)
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Size = AdaptiveTextSize.Medium
                },

                new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
	                    new AdaptiveFact("Position ID:", position.PositionExternalId),
	                    new AdaptiveFact("Location:", position.Location?.LocationAddress ?? string.Empty),
	                    new AdaptiveFact("Days open:", position.DaysOpen.ToString()),
						new AdaptiveFact("Applicants:", position.Candidates.Count.ToString()),
                        new AdaptiveFact("Hiring manager:", position.HiringManager.Name)
                    }
                },

	            new AdaptiveTextBlock($"Description: {position.Description}")
	            {
					Wrap = true
				}
			};

            return card;
        }
    }
}