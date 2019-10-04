using System;
using System.Threading.Tasks;
using AdaptiveCards;
using AutoMapper;
using Microsoft.Bot.Builder.Dialogs;
using TeamsTalentMgmtAppV3.Extensions;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Dialogs
{
    [Serializable]
    public class PositionsDetailsDialog : IDialog<object>
    {
        private readonly IPositionService _positionService;
        private readonly IMapper _mapper;

        public PositionsDetailsDialog(IPositionService positionService, 
            IMapper mapper)
        {
            _positionService = positionService;
            _mapper = mapper;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            var text = context.Activity.GetTextWithoutCommand(BotCommands.PositionsDetailsDialogCommand);
            Position position = null;
            if (text.HasValue())
            {
                if (int.TryParse(text, out var positionId))
                {
                    position = await _positionService.GetById(positionId);
                }
                else
                {
                    position = await _positionService.GetByExternalId(text, context.CancellationToken);
                }
            }
            else
            {

                reply.Text = "Please specify Position ID.";
            }

            if (position is null)
            {
                reply.Text = "I couldn't find this position.";
            }
            else
            {
                var card = _mapper.Map<AdaptiveCard>(position);
                reply.Attachments.Add(card.ToAttachment());
            }

            await context.PostAsync(reply);
            context.Done(string.Empty);
        }
    }
}