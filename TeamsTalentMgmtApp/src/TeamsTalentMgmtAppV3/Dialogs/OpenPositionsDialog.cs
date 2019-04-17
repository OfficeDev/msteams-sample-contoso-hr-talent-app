using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using TeamsTalentMgmtAppV3.Constants;
using TeamsTalentMgmtAppV3.Models.Bot;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Dialogs
{
	[Serializable]
    public class OpenPositionsDialog : IDialog<object>
	{
		private readonly IPositionService _positionService;
		private readonly IMapper _mapper;

		public OpenPositionsDialog(IPositionService positionService, 
			IMapper mapper)
		{
			_positionService = positionService;
			_mapper = mapper;
		}

		public async Task StartAsync(IDialogContext context)
		{
			var reply = context.MakeMessage();
			
			var openPositions = await _positionService.GetOpenPositions(context.Activity.From.Name, context.CancellationToken);
			if (openPositions.Any())
			{
				var title = $"You have {openPositions.Count} active postings right now:";
			
				var cardListItems = _mapper.Map<List<CardListItem>>(openPositions);
				
				var attachment = new Attachment
				{
					ContentType = ListCard.ContentType,
					Content = new ListCard
					{
						Title = title,
						Items = cardListItems,
						Buttons = new List<CardAction>
						{
							new CardAction(ActionTypes.ImBack, "Add new job posting", value:$"{BotCommands.NewJobPostingDialog}")
						}
					}
				};
				
				reply.Attachments = new List<Attachment>
				{
					attachment
				};
			}
			else
			{
				reply.Text = "You have no open positions";
			}

			await context.PostAsync(reply);
			context.Done(string.Empty);
		}
	}
}