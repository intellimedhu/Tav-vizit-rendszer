using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Email;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Email.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/email-templates", Name = "EmailsApi")]
    public class EmailsApiController : Controller
    {
        private readonly IEmailSettingsService _emailSettingsService;
        private readonly IEmailTokenizerDataService _emailTokenizerDataService;
        private readonly ISmtpService _smtpService;


        public EmailsApiController(
            IEmailSettingsService emailSettingsService,
            IEmailTokenizerDataService emailTokenizerDataService,
            ISmtpService smtpService)
        {
            _emailSettingsService = emailSettingsService;
            _emailTokenizerDataService = emailTokenizerDataService;
            _smtpService = smtpService;
        }


        [HttpGet]
        [Route("templates", Name = "EmailsApi.GetTemplates")]
        public async Task<IActionResult> Get()
        {
            var templates = await _emailTokenizerDataService.GetEmailTemplatesAsync();

            return Ok(new
            {
                templates = templates.Select(template => new EmailTemplateViewModel()
                {
                    Id = template.Id,
                    Name = template.Name,
                    Tokens = template.Tokens
                })
            });
        }

        [HttpGet]
        [Route("", Name = "EmailsApi.GetTemplateById")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var template = await _emailTokenizerDataService.FindEmailTemplateByIdAsync(id);
            if (template == null)
            {
                return NotFound();
            }

            var tokenizedEmail = await _emailTokenizerDataService.GetTokenizedEmailByTemplateIdAsync(id);
            var tokenizedEmailViewModel = new TokenizedEmailViewModel();
            tokenizedEmailViewModel.UpdateViewModel(tokenizedEmail);


            return Ok(new
            {
                template = new EmailTemplateViewModel()
                {
                    Id = template.Id,
                    Name = template.Name,
                    Tokens = template.Tokens
                },
                tokenizedEmail = tokenizedEmailViewModel
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Post([FromBody]TokenizedEmailViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var template = await _emailTokenizerDataService.FindEmailTemplateByIdAsync(viewModel.TemplateId);
            if (template == null)
            {
                return NotFound();
            }

            await _emailTokenizerDataService.SaveTokenizedEmailAsync(viewModel);

            return Ok();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route("test", Name = "EmailsApi.Test")]
        public async Task<IActionResult> Post([FromBody] PreparedDataViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.TemplateId) ||
                string.IsNullOrEmpty(viewModel.PreparedData))
            {
                return BadRequest();
            }

            var tokenizedEmail = await _emailTokenizerDataService.GetTokenizedEmailByTemplateIdAsync(viewModel.TemplateId);
            if (tokenizedEmail == null)
            {
                return NotFound("No saved template.");
            }

            var emailSettings = await _emailSettingsService.GetEmailSettingsAsync();
            if (!emailSettings.DebugEmailAddresses.Any())
            {
                return NotFound("No debug emails.");
            }

            var rows = viewModel.PreparedData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var body = tokenizedEmail.RawBody;
            foreach (var row in rows)
            {
                var pairs = row.Split(new[] { ':' }, 2);
                if (pairs.Length != 2)
                {
                    continue;
                }

                body = body.Replace($"-{pairs[0].Trim()}-", pairs[1].Trim());
            }

            body = body.Replace("\n", "<br />");
            if (!string.IsNullOrEmpty(emailSettings.EmailFooter))
            {
                body += "<br />" + emailSettings.EmailFooter;
            }

            var message = new MailMessage()
            {
                Body = body,
                IsBodyHtml = true,
                Subject = "Test - " + tokenizedEmail.Subject,
                To = string.Join(";", emailSettings.DebugEmailAddresses)
            };

            var result = await _smtpService.SendAsync(message);
            if (result.Succeeded)
            {
                return Ok();
            }

            return Conflict();
        }
    }
}
