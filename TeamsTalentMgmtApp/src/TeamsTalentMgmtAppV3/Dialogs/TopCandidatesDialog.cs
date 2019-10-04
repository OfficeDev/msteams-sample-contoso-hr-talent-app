using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Dialogs
{
    [Serializable]
    public class TopCandidatesDialog : IDialog<object>
    {
        private readonly IPositionService _positionService;
        private readonly IMapper _mapper;

        public TopCandidatesDialog(IPositionService positionService,
            IMapper mapper)
        {
            _positionService = positionService;
            _mapper = mapper;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            
            var text = context.Activity.GetTextWithoutCommand(BotCommands.TopCandidatesDialogCommand);
            var positions = await _positionService.Search(text, 15, context.CancellationToken);
            if (positions.Any())
            {
                reply.Attachments = new List<Attachment>();
                
                if (positions.Count == 1)
                {
                    var candidates = positions[0].Candidates;
                    if (candidates.Any())
                    {
                        var cardListItems = _mapper.Map<List<CardListItem>>(candidates);
                        var attachment = new Attachment
                        {
                            ContentType = ListCard.ContentType,
                            Content = new ListCard
                            {
                                Title = "Top candidates who have recently applied to your position:",
                                Items = cardListItems
                            }
                        };
                        
                        reply.Attachments.Add(attachment);
                    }
                    else
                    {
                        reply.Text = $"There are no candidates for position ID: {positions[0].PositionExternalId}";
                    }
                }
                else
                {
                    var cardListItems = _mapper.Map<List<CardListItem>>(positions, 
                        opt => opt.Items["botCommand"] = BotCommands.TopCandidatesDialogCommand);
				
                    var attachment = new Attachment
                    {
                        ContentType = ListCard.ContentType,
                        Content = new ListCard
                        {
                            Title = "I found several positions. Please specify:",
                            Items = cardListItems
                        }
                    };
				
                    reply.Attachments.Add(attachment);
                }
            }
            else
            {
                reply.Text = "You don't have open position with such ID.";
            }
            
            await context.PostAsync(reply);
            context.Done(string.Empty);
        }
    }
}
