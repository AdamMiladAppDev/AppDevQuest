using Backend.Contracts.Requests;
using Backend.Services.Surveys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ResponsesController : ControllerBase
    {
        private readonly ISurveyService _surveyService;
        private readonly ILogger<ResponsesController> _logger;

        public ResponsesController(ISurveyService surveyService, ILogger<ResponsesController> logger)
        {
            _surveyService = surveyService;
            _logger = logger;
        }

        [HttpGet("{token}")]
        public async Task<IActionResult> GetSurveyForToken(string token, CancellationToken cancellationToken)
        {
            var survey = await _surveyService.GetSurveyForTokenAsync(token, cancellationToken);
            return survey is null ? NotFound() : Ok(survey);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitResponse(
            [FromBody] SubmitSurveyResponseRequest request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                await _surveyService.SubmitSurveyResponseAsync(request, cancellationToken);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to submit survey response.");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
