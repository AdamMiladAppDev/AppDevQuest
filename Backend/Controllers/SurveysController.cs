using Backend.Contracts.Requests;
using Backend.Services.Surveys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SurveysController : ControllerBase
    {
        private readonly ISurveyService _surveyService;
        private readonly ILogger<SurveysController> _logger;

        public SurveysController(ISurveyService surveyService, ILogger<SurveysController> logger)
        {
            _surveyService = surveyService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetSurveys(CancellationToken cancellationToken)
        {
            var surveys = await _surveyService.GetSurveysAsync(cancellationToken);
            return Ok(surveys);
        }

        [HttpGet("{surveyId:guid}")]
        public async Task<IActionResult> GetSurvey(Guid surveyId, CancellationToken cancellationToken)
        {
            var survey = await _surveyService.GetSurveyAsync(surveyId, cancellationToken);
            return survey is null ? NotFound() : Ok(survey);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSurvey(
            [FromBody] CreateSurveyRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var survey = await _surveyService.CreateSurveyAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetSurvey), new { surveyId = survey.Id }, survey);
        }

        [HttpPost("{surveyId:guid}/invitations")]
        public async Task<IActionResult> SendInvitations(
            Guid surveyId,
            [FromBody] SendInvitationsRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var previews = await _surveyService.SendInvitationsAsync(surveyId, request, cancellationToken);
                if (previews.Count > 0)
                {
                    return Ok(new { previews });
                }

                return Accepted();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error sending survey invitations.");
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
