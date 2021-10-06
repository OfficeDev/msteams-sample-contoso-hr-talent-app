using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtAppV4.Models;
using TeamsTalentMgmtAppV4.Services.Interfaces;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Models.Dto;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV4.Controllers
{
    [ApiController]
    public class ClientApiController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;
        private readonly ICandidateService _candidateService;
        private readonly IPositionService _positionService;
        private readonly IGraphApiService _graphApiService;
        private readonly IBotFrameworkHttpAdapter _botAdapter;
        private readonly INotificationService _notificationService;

        public ClientApiController(
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            ICandidateService candidateService,
            IPositionService positionService,
            IGraphApiService graphApiService,
            IBotFrameworkHttpAdapter botAdapter,
            INotificationService notificationService)
        {
            _appSettings = appSettings.Value;
            _candidateService = candidateService;
            _positionService = positionService;
            _graphApiService = graphApiService;
            _botAdapter = botAdapter;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        // todoscott:add controller access token logic....
        [HttpGet]
        [Route("api/app")]
        [Authorize]
        public ActionResult Get() => Ok(new
        {
            appId = _appSettings.TeamsAppId,
            botId = _appSettings.MicrosoftAppId
        });

        [HttpGet]
        [Route("api/candidates/{id}")]
        [Authorize]
        public async Task<ActionResult<CandidateDto>> GetCandidateById(int id, CancellationToken cancellationToken)
        {
            var candidate = await _candidateService.GetById(id, cancellationToken);
            return Ok(_mapper.Map<CandidateDto>(candidate));
        }

        [HttpGet]
        [Route("api/candidates/{id}/profilePicture")]
        [Authorize]
        public async Task<HttpResponseMessage> GetCandidateImageById(int id, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<CandidateDto>(await _candidateService.GetById(id, cancellationToken));
            using (var ms = new MemoryStream(Convert.FromBase64String(user.ProfilePictureDataOnly)))
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(ms.ToArray())
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Image.Jpeg);
                return result;
            }
        }

        [HttpPut]
        [Route("api/candidates/")]
        [Authorize]
        public async Task<ActionResult> UpdateCandidateStage([FromBody] Candidate candidate, CancellationToken cancellationToken)
        {
            await _candidateService.UpdateCandidateStage(candidate.CandidateId, candidate.Stage, cancellationToken);
            return Ok();
        }

        [HttpGet]
        [Route("api/positions")]
        [Authorize]
        public async Task<ActionResult> GetAllPositions(CancellationToken cancellationToken)
        {
            var positions = await _positionService.GetAllPositions(cancellationToken);
            return Ok(_mapper.Map<List<PositionDto>>(positions));
        }

        [HttpGet]
        [Route("api/positions/{id}")]
        [Authorize]
        public async Task<ActionResult<PositionDto>> GetPositionById(int id, CancellationToken cancellationToken)
        {
            var position = await _positionService.GetById(id, cancellationToken);
            return Ok(_mapper.Map<PositionDto>(position));
        }

        [HttpGet]
        [Route("api/recruiters/{alias}/positions")]
        [Authorize]
        public async Task<ActionResult<PositionDto>> GetPositionsByRecruiterAlias(string alias, CancellationToken cancellationToken)
        {
            var positions = await _positionService.GetOpenPositions(alias, cancellationToken);
            return Ok(_mapper.Map<List<PositionDto>>(positions));
        }

        [HttpGet]
        [Route("api/positions/open")]
        [Authorize]
        public async Task<ActionResult<PositionDto>> GetAllOpenPositions(CancellationToken cancellationToken)
        {
            var positions = await _positionService.GetOpenPositions(string.Empty, cancellationToken);
            return Ok(_mapper.Map<List<PositionDto>>(positions));
        }

        [HttpPut]
        [Route("api/candidates/{id}/feedback")]
        [Authorize]
        public async Task<ActionResult> AddFeedbackToCandidate(int id, [FromBody] CandidateFeedback candidateFeedback, CancellationToken cancellationToken)
        {
            await _candidateService.AddComment(
                new TeamTalentMgmtApp.Shared.Models.Commands.LeaveCommentCommand
                {
                    CandidateId = id,
                    Comment = candidateFeedback.Feedback
                },
                candidateFeedback.Name,
                cancellationToken);

            await _notificationService.SendToConversation(candidateFeedback.Feedback, null, new ConversationData
            {
                AccountId = candidateFeedback.Notify,
                TenantId = candidateFeedback.TenantId,
                ServiceUrl = "https://smba.trafficmanager.net/apis"
            }, cancellationToken);

            //if (!Request.Headers.TryGetValue("Authorization", out var values))
            //{
            //    return Unauthorized();
            //}

            //var token = values[0].Substring(values[0].IndexOf(' ')).Trim();

            //var chatId = await _graphApiService.GetProactiveChatIdForUser(token, candidateFeedback.TenantId, candidateFeedback.Notify, candidateFeedback.Feedback, cancellationToken);

            //var conversationReference = new ConversationReference
            //{
            //    User = new ChannelAccount
            //    {
            //        Id = candidateFeedback.Notify
            //    },
            //    Conversation = new ConversationAccount
            //    {
            //        Id = chatId,
            //        TenantId = candidateFeedback.TenantId
            //    },
            //    ServiceUrl = "https://smba.trafficmanager.net/apis"
            //};

            //await ((BotAdapter)_botAdapter).ContinueConversationAsync(_appSettings.MicrosoftAppId, conversationReference, ProactiveCallback, cancellationToken);

            return Ok();
        }

        private async Task ProactiveCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            await turnContext.SendActivityAsync("proactive hello");
        }
    }

    public class CandidateFeedback
    {
        public string Feedback { get; set; }
        public string Name { get; set; }
        public string Notify { get; set; }
        public string TenantId { get; set; }
    }
}
