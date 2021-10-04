using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using TeamsTalentMgmtAppV4.Extensions;
using TeamsTalentMgmtAppV4.Models.TemplateModels;
using TeamsTalentMgmtAppV4.Services.Templates;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Bot.Dialogs
{
    public class PositionsDetailsDialog : Dialog
    {
        private readonly IPositionService _positionService;
        private readonly PositionsTemplate _positionsTemplate;

        public PositionsDetailsDialog(
            PositionsTemplate positionsTemplate,
            IPositionService positionService)
            : base(nameof(PositionsDetailsDialog))
        {
            _positionService = positionService;
            _positionsTemplate = positionsTemplate;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(
            DialogContext dc,
            object options = null,
            CancellationToken cancellationToken = default)
        {
            var text = dc.Context.Activity.GetTextWithoutCommand(BotCommands.PositionsDetailsDialogCommand);
            var positions = new List<Position>();
            if (!string.IsNullOrEmpty(text))
            {
                var position = int.TryParse(text, out var positionId)
                    ? await _positionService.GetById(positionId)
                    : await _positionService.GetByExternalId(text, cancellationToken);

                if (position != null)
                {
                    positions.Add(position);
                }
            }

            var positionsTemplate = new PositionTemplateModel
            {
                Items = positions,
                NoItemsLabel = "I couldn't find this position."
            };

            await _positionsTemplate.ReplyWith(dc.Context, TemplateConstants.PositionAsAdaptiveCardWithMultipleItems, positionsTemplate);

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
