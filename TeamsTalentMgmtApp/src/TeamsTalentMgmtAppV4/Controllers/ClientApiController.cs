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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TeamsTalentMgmtAppV4.Models;
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

        public ClientApiController(
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            ICandidateService candidateService,
            IPositionService positionService)
        {
            _appSettings = appSettings.Value;
            _candidateService = candidateService;
            _positionService = positionService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("api/app")]
        public ActionResult Get() => Ok(new
        {
            appId = _appSettings.TeamsAppId,
            botId = _appSettings.MicrosoftAppId
        });

        [HttpGet]
        [Route("api/candidates/{id}")]
        public async Task<ActionResult<CandidateDto>> GetCandidateById(int id, CancellationToken cancellationToken)
        {
            var candidate = await _candidateService.GetById(id, cancellationToken);
            return Ok(_mapper.Map<CandidateDto>(candidate));
        }

        [HttpGet]
        [Route("api/candidates/{id}/profilePicture")]
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
        public async Task<ActionResult> UpdateCandidateStage([FromBody] Candidate candidate, CancellationToken cancellationToken)
        {
            await _candidateService.UpdateCandidateStage(candidate.CandidateId, candidate.Stage, cancellationToken);
            return Ok();
        }

        [HttpGet]
        [Route("api/positions")]
        public async Task<ActionResult> GetAllPositions(CancellationToken cancellationToken)
        {
            var positions = await _positionService.GetAllPositions(cancellationToken);
            return Ok(_mapper.Map<List<PositionDto>>(positions));
        }

        [HttpGet]
        [Route("api/positions/{id}")]
        public async Task<ActionResult<PositionDto>> GetPositionById(int id, CancellationToken cancellationToken)
        {
            var position = await _positionService.GetById(id, cancellationToken);
            return Ok(_mapper.Map<PositionDto>(position));
        }

        [HttpGet]
        [Route("api/recruiters/{alias}/positions")]
        public async Task<ActionResult<PositionDto>> GetPositionsByRecruiterAlias(string alias, CancellationToken cancellationToken)
        {
            var positions = await _positionService.GetOpenPositions(alias, cancellationToken);
            return Ok(_mapper.Map<List<PositionDto>>(positions));
        }

        [HttpGet]
        [Route("api/positions/open")]
        public async Task<ActionResult<PositionDto>> GetAllOpenPositions(CancellationToken cancellationToken)
        {
            var positions = await _positionService.GetOpenPositions(string.Empty, cancellationToken);
            return Ok(_mapper.Map<List<PositionDto>>(positions));
        }
    }
}
