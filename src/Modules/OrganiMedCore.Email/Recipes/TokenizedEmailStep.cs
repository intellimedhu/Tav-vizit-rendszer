using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrganiMedCore.Email.Services;
using OrganiMedCore.Email.ViewModels;
using System;
using System.Threading.Tasks;

namespace OrganiMedCore.Email.Recipes
{
    public class TokenizedEmailStep : IRecipeStepHandler
    {
        private readonly IEmailTokenizerDataService _emailTokenizerDataService;
        private readonly ILogger _logger;


        public TokenizedEmailStep(IEmailTokenizerDataService emailTokenizerDataService, ILogger<TokenizedEmailStep> logger)
        {
            _emailTokenizerDataService = emailTokenizerDataService;
            _logger = logger;
        }


        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "tokenizedemail", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            try
            {
                var data = context.Step.ToObject<TokenizedEmailStepModel>();

                return _emailTokenizerDataService.SaveManyTokenizedEmailsAsync(data.Emails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during recipe step: tokenizedemail");
            }

            return Task.CompletedTask;
        }


        private class TokenizedEmailStepModel
        {
            [JsonProperty("emails")]
            public TokenizedEmailViewModel[] Emails { get; set; }
        }
    }
}
