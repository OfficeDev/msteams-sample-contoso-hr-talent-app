using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Constants;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Dialogs
{
    [Serializable]
    public class CandidateDetailsDialog : IDialog<object>
    {
        private readonly ICandidateService _candidateService;
        private readonly IMapper _mapper;

        public CandidateDetailsDialog(ICandidateService candidateService,
            IMapper mapper)
        {
            _candidateService = candidateService;
            _mapper = mapper;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            
            var text = context.Activity.GetTextWithoutCommand(BotCommands.CandidateDetailsDialogCommand);
            var candidates = await _candidateService.Search(text, 15, context.CancellationToken);
            if (candidates.Any())
            {
                reply.Attachments = new List<Attachment>();
                if (candidates.Count == 1)
                {
                    var card = _mapper.Map<AdaptiveCard>(candidates[0]);
                    reply.Attachments.Add(card.ToAttachment());
                }
                else
                {
                    var cardListItems = _mapper.Map<List<CardListItem>>(candidates);
				
                    var attachment = new Attachment
                    {
                        ContentType = ListCard.ContentType,
                        Content = new ListCard
                        {
                            Title = "I found following candidates:",
                            Items = cardListItems
                        }
                    };
				
                    reply.Attachments.Add(attachment);
                }
            }
            else
            {
                reply.Text = "You don't have such candidates.";
            }
            
            await context.PostAsync(reply);
            context.Done(string.Empty);
        }
    }
}