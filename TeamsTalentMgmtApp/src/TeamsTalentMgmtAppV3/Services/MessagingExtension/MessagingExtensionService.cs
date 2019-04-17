using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using TeamsTalentMgmtAppV3.Constants;
using TeamsTalentMgmtAppV3.Extensions;
using TeamsTalentMgmtAppV3.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Services.MessagingExtension
{
	public class MessagingExtensionService : IMessagingExtensionService
	{
		private readonly IMapper _mapper;
		private readonly IMessagingExtensionActionsService _messagingExtensionActionsService;
		private readonly ICandidateService _candidateService;
		private readonly IPositionService _positionService;

		public MessagingExtensionService(IMessagingExtensionActionsService messagingExtensionActionsService,
			ICandidateService candidateService,
			IPositionService positionService,
			IMapper mapper)
		{
			_messagingExtensionActionsService = messagingExtensionActionsService;
			_candidateService = candidateService;
			_positionService = positionService;
			_mapper = mapper;
		}

		public async Task<HttpResponseMessage> HandleInvokeRequest(HttpRequestMessage request, Activity activity, CancellationToken cancellationToken)
		{
			var extensionQueryData = activity.GetComposeExtensionQueryData();
			if (extensionQueryData?.CommandId == null)
			{
				return request.CreateResponse(HttpStatusCode.NotAcceptable);
			}

			// Show task module from messaging extension
			if (activity.IsFetchTask())
			{
				return _messagingExtensionActionsService.HandleFetchTaskAction(request, extensionQueryData.CommandId);
			}

			// User submits action from task module
			if (activity.IsSubmitAction())
			{
				return await _messagingExtensionActionsService.HandleSubmitAction(request, activity, cancellationToken);
			}
			
			// User clicks on messaging extension card
			if (activity.IsCardButtonClickEvent())
			{
				return await _messagingExtensionActionsService.HandleButtonClickEvent(request, extensionQueryData.CommandId, activity, cancellationToken);
			}

			return await ProcessQueryAction(request, extensionQueryData, cancellationToken);
		}

		private async Task<HttpResponseMessage> ProcessQueryAction(HttpRequestMessage request, ComposeExtensionQuery extensionQueryData, CancellationToken cancellationToken)
		{
			if (extensionQueryData.Parameters == null)
			{
				return request.CreateResponse(HttpStatusCode.NoContent);
			}

			var isInitialRun = false;
			var initialRunParameter = GetQueryParameterByName(extensionQueryData, "initialRun");

			// situation where the incoming payload was received from the config popup
			if (extensionQueryData.State.HasValue())
			{
				initialRunParameter = "true";
			}

			if (string.Equals(initialRunParameter, "true", StringComparison.OrdinalIgnoreCase))
			{
				isInitialRun = true;
			}

			var maxResults = extensionQueryData.QueryOptions.Count ?? 25;
			if (isInitialRun)
			{
				maxResults = 5;
			}

			var attachments = new List<ComposeExtensionAttachment>();			

			var searchText = GetQueryParameterByName(extensionQueryData, MessagingExtensionCommands.SearchTextParameterName);
			
			switch (extensionQueryData.CommandId)
			{
				case MessagingExtensionCommands.SearchPositions:
					var positions = await _positionService.Search(searchText, maxResults, cancellationToken);
					attachments = _mapper.Map<List<ComposeExtensionAttachment>>(positions);
					break;

				case MessagingExtensionCommands.SearchCandidates:
					var candidates = await _candidateService.Search(searchText, maxResults, cancellationToken);
					attachments = _mapper.Map<List<ComposeExtensionAttachment>>(candidates);
					break;
			}

			var response = new ComposeExtensionResponse
			{
				ComposeExtension = new ComposeExtensionResult
				{
					Type = "result",
					Attachments = attachments,
					AttachmentLayout = AttachmentLayoutTypes.List
				}
			};

			return request.CreateResponse(HttpStatusCode.OK, response);
		}

		private static string GetQueryParameterByName(ComposeExtensionQuery query, string name)
		{
			if (query?.Parameters == null || query.Parameters.Count == 0)
			{
				return string.Empty;
			}

			var parameter = query.Parameters[0];
			if (!string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))
			{
				return string.Empty;
			}

			return parameter.Value != null ? parameter.Value.ToString() : string.Empty;
		}
	}
}