using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using AutoMapper;
using TeamTalentMgmtApp.Shared.Models.DatabaseContext;
using TeamTalentMgmtApp.Shared.Models.Dto;
using TeamTalentMgmtApp.Shared.Services.Interfaces;

namespace TeamsTalentMgmtAppV3.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")] // * (Allow all) is for demo purposes only. You should NOT allow CORS requests from any origin
    public class ClientApiController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly ICandidateService _candidateService;
        private readonly IPositionService _positionService;

        public ClientApiController(
            ICandidateService candidateService,
            IPositionService positionService,
            IMapper mapper)
        {
            _candidateService = candidateService;
            _positionService = positionService;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("api/app")]
        public IHttpActionResult Get()
        {
            return Ok(new
            {
                appId = ConfigurationManager.AppSettings["TeamsAppId"],
                botId = ConfigurationManager.AppSettings["MicrosoftAppId"]
            });
        }

        [HttpGet]
        [Route("api/candidates/{id}")]
        public async Task<IHttpActionResult> GetCandidateById(int id, CancellationToken cancellationToken)
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
        public async Task<IHttpActionResult> UpdateCandidateStage([FromBody] Candidate candidate, CancellationToken cancellationToken)
        {
            await _candidateService.UpdateCandidateStage(candidate.CandidateId, candidate.Stage, cancellationToken);
            return Ok();
        }

        [HttpGet]
        [Route("api/positions")]
        public async Task<IHttpActionResult> GetAllPositions(CancellationToken cancellationToken)
        {
            var positions = await _positionService.GetAllPositions(cancellationToken);
            return Ok(_mapper.Map<List<PositionDto>>(positions));
        }

        [HttpGet]
        [Route("api/positions/{id}")]
        public async Task<IHttpActionResult> GetPositionById(int id)
        {
            var position = await _positionService.GetById(id);
            return Ok(_mapper.Map<PositionDto>(position));
        }

        [HttpGet]
        [Route("api/recruiters/{alias}/positions")]
        public async Task<IHttpActionResult> GetPositionsByRecruiterAlias(string alias, CancellationToken cancellationToken)
        {
            var positions = await _positionService.GetOpenPositions(alias, cancellationToken);
            return Ok(_mapper.Map<List<PositionDto>>(positions));
        }
    }
}
