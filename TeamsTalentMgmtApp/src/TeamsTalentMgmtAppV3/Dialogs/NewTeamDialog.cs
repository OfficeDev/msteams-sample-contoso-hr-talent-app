using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
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
    public class NewTeamDialog : IDialog<object>
    {
        private readonly IPositionService _positionService;
        private readonly IGraphApiService _graphApiService;
        private readonly IMapper _mapper;
        private readonly string _connectionName;

        public NewTeamDialog(IPositionService positionService, 
            IGraphApiService graphApiService,
            IMapper mapper)
        {
            _positionService = positionService;
            _graphApiService = graphApiService;
            _mapper = mapper;
            _connectionName = ConfigurationManager.AppSettings["BotOAuthConnectionName"];
        }

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();
            
            var text = context.Activity.GetTextWithoutCommand(BotCommands.NewTeamDialog);
            var positions = await _positionService.Search(text, 15, context.CancellationToken);
            if (positions.Any())
            {
                if (positions.Count == 1)
                {
                    //await context.SignOutUserAsync(_connectionName);
                    var token = await context.GetUserTokenAsync(_connectionName);
                    if (string.IsNullOrEmpty(token?.Token))
                    {
                        reply = await context.Activity.CreateOAuthReplyAsync(_connectionName, "Please sign in to proceed.", "Sign In");
                    }
                    else
                    {
                       var team = await _graphApiService.CreateNewTeamForPosition(positions[0], token.Token);
                       reply.Text = $"[Team {team.DisplayName}]({team.WebUrl}) has been created.";
                    }
                }
                else
                {
                    var cardListItems = _mapper.Map<List<CardListItem>>(positions,
                        opt => opt.Items["botCommand"] = BotCommands.NewTeamDialog);

                    reply.Attachments.Add(new Attachment
                    {
                        ContentType = ListCard.ContentType,
                        Content = new ListCard
                        {
                            Title = "I found following positions:",
                            Items = cardListItems
                        }
                    });
                }
            }
            else
            {
                reply.Text = "You don't have such open positions.";
            }

            await context.PostAsync(reply);
            context.Done(string.Empty);
        }
    }
}