using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams.Models;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamTalentMgmtApp.Shared.Constants;
using TeamTalentMgmtApp.Shared.Models.Bot;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Dialogs
{
    [Serializable]
    public class CandidateSummaryDialog : IDialog<object>
    {
        private readonly ICandidateService _candidateService;
        private readonly IMapper _mapper;

        public CandidateSummaryDialog(ICandidateService candidateService,
            IMapper mapper)
        {
            _candidateService = candidateService;
            _mapper = mapper;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();

            var text = context.Activity.GetTextWithoutCommand(BotCommands.CandidateSummaryDialog);
            var candidates = await _candidateService.Search(text, 15, context.CancellationToken);
            if (candidates.Any())
            {
                reply.Attachments = new List<Attachment>();
                if (candidates.Count == 1)
                {
                    var candidate = candidates[0];
                    // Create file consent card
                    var consentContext = new FileConsentContext
                    {
                        CandidateId = candidate.CandidateId
                    };
                    
                    var fileConsentCard = new FileConsentCard
                    {
                        Name = SanitizeFileName($"{candidate.Name} Summary.txt"),
                        Description =  $"Here is summary for {candidate.Name}",
                        SizeInBytes = Encoding.UTF8.GetBytes(candidate.Summary).Length,
                        AcceptContext = consentContext,
                        DeclineContext = consentContext
                    };

                    reply.Attachments.Add(fileConsentCard.ToAttachment());
                }
                else
                {
                    var cardListItems = _mapper.Map<List<CardListItem>>(candidates,
                        opt => opt.Items["botCommand"] = BotCommands.CandidateSummaryDialog);

                    var attachment = new Attachment
                    {
                        ContentType = ListCard.ContentType,
                        Content = new ListCard
                        {
                            Title = "Please select candidate:",
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
        
        private static string SanitizeFileName(string fileName)
        {
            foreach(var invalidChar in Path.GetInvalidFileNameChars())
            {
                if (fileName.Contains(invalidChar))
                {
                    fileName = fileName.Replace(invalidChar, '_');
                }
            }

            return fileName;
        }

    }
}