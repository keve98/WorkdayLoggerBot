using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace WorkdayLoggerBot.LUIS
{
    public class LuisHelper : IRecognizer
    {
        private readonly LuisRecognizer _luisRecognizer;

        public LuisHelper()
        {

            var service = new LuisService()
            {
                AppId = "f2fb6c09-d1e3-466a-948a-fd506d02e655",
                SubscriptionKey = "b91653e7a64542da8d7010a7c0e38f20",
                Region = "westeurope",
                Version = "0.1"
            };

            var app = new LuisApplication(service);
            var regOptions = new LuisRecognizerOptionsV2(app)
            {
                IncludeAPIResults = true,
                PredictionOptions = new LuisPredictionOptions()
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true
                }
            };

            _luisRecognizer = new LuisRecognizer(regOptions);

        }


        public async Task<RecognizerResult> RecognizeAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            return await _luisRecognizer.RecognizeAsync(turnContext, cancellationToken);
        }

        public async Task<T> RecognizeAsync<T>(ITurnContext turnContext, CancellationToken cancellationToken) where T : IRecognizerConvert, new()
        {
            return await _luisRecognizer.RecognizeAsync<T>(turnContext, cancellationToken);
        }
    }
}
