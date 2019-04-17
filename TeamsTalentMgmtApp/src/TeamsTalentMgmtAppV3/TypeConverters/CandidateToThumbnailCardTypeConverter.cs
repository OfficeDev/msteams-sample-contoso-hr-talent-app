using System.Collections.Generic;
using AutoMapper;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Models.DatabaseContext;
using TeamsTalentMgmtAppV3.Models.Extensions;

namespace TeamsTalentMgmtAppV3.TypeConverters
{
    public class CandidateToThumbnailCardTypeConverter : ITypeConverter<Candidate, ThumbnailCard>
    {
        public ThumbnailCard Convert(Candidate candidate, ThumbnailCard card, ResolutionContext _)
        {
            if (candidate is null)
            {
                return null;
            }

            if (card is null)
            {
                card = new ThumbnailCard();
            }

            card.Title = candidate.Name;
            card.Text = $"Current role: {candidate.CurrentRole} | {candidate.Location.GetLocationString()}";
            card.Images = new List<CardImage>
            {
                new CardImage(candidate.ProfilePicture)
            };
            
            return card;
        }
    }
}