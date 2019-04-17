using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Dialogs
{
    [Serializable]
    public class NewJobPostingDialog : IDialog<object>
    {
        private readonly ITemplateService _templateService;

        public NewJobPostingDialog(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            var card = _templateService.GetAdaptiveCardForNewJobPosting();
            reply.Attachments.Add(card.ToAttachment());

            await context.PostAsync(reply);
            context.Done(string.Empty);
        }
    }
}