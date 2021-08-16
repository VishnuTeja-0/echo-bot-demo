using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EchoBotDemo.Dialogs
{
    public class CarPropertiesDialog : ComponentDialog
    {
        private static IStatePropertyAccessor<EchoBotDemo.UserProfile> _userStateAccessor;

        public CarPropertiesDialog(UserState userState) : base(nameof(CarPropertiesDialog))
        {
            _userStateAccessor = userState.CreateProperty<UserProfile>(nameof(UserProfile));

            var waterfallSteps = new WaterfallStep[]
            {
                TypeStepAsync,
                FuelTypeAsync,
                CompanyStepAsync,
                ColorStepAsync,
                SecondHandStepAsync,
                MaximumAgeStepAsync,
                SummaryStepAsync
            };

            AddDialog(new WaterfallDialog("start", waterfallSteps));
            AddDialog(new ChoicePrompt("type"));
            AddDialog(new ChoicePrompt("fuel"));
            AddDialog(new TextPrompt("company"));
            AddDialog(new TextPrompt("color"));
            AddDialog(new ConfirmPrompt("isUsed"));
            AddDialog(new NumberPrompt<int>("maxAge", MaximumAgeStepAsyncValidator));

            InitialDialogId = "start";
        }

        private static async Task<DialogTurnResult> TypeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("type",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter type of car you want to buy"),
                    RetryPrompt = MessageFactory.Text("Please enter one of the given choices for type of car"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Sedan", "Compact", "SUV", "Hatchback", "Sports", "Coupe", "Convertible" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> FuelTypeAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["type"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync("fuel",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Nice. Now select fuel type of this car"),
                    RetryPrompt = MessageFactory.Text("Please enter one of the given choices for fuel type"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Petrol", "Diesel", "CNG", "Electric", "Hybrid"}),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> CompanyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["fuel"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync("company",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter brand name of manufactoring company from which to choose car"),
                    RetryPrompt = MessageFactory.Text("Please enter a valid brand name")
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> ColorStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["company"] = (string)stepContext.Result;

            return await stepContext.PromptAsync("color",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Enter color of car"),
                    RetryPrompt = MessageFactory.Text("Please enter a valid color name")
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> SecondHandStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["color"] = (string)stepContext.Result;

            return await stepContext.PromptAsync("isUsed",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Do you want the car to be a used one?"),
                    RetryPrompt = MessageFactory.Text("I don't understand. Please confirm.")
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> MaximumAgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var isUsed = (bool)stepContext.Result;
            stepContext.Values["used"] = isUsed;

            if (isUsed)
            {
                return await stepContext.PromptAsync("maxAge",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please the maximum age limit of car"),
                    RetryPrompt = MessageFactory.Text("Please enter a valid age. It should be less than 10 years and more than 0 year(s)")
                }, cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(-1, cancellationToken);
            }  
        }

        private static Task<bool> MaximumAgeStepAsyncValidator(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 10);
        }

        private static async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;
            var userProfile = await _userStateAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            userProfile.CarProperties.Type = (string)stepContext.Values["type"];
            userProfile.CarProperties.FuelType = (string)stepContext.Values["fuel"];
            userProfile.CarProperties.ManufacturingCompany = (string)stepContext.Values["company"];
            userProfile.CarProperties.Color = (string)stepContext.Values["color"];
            userProfile.CarProperties.IsSecondHand = (bool)stepContext.Values["used"];
            userProfile.CarProperties.MaximumAge = (int)stepContext.Values["age"];

            var msg = $"Here are the filters you have set for the search of your car:\n" +
                $"*  Car Type: {userProfile.CarProperties.Type}\n" +
                $"*  Car Fuel Type: {userProfile.CarProperties.FuelType}\n" +
                $"*  Car Manufacturer: {userProfile.CarProperties.ManufacturingCompany}\n" +
                $"*  Car Color: {userProfile.CarProperties.Color}\n";
            if (userProfile.CarProperties.IsSecondHand)
            {
                msg += $"Car Second Hand: Yes (Max. {userProfile.CarProperties.MaximumAge} years old";
            }
            else
            {
                msg += $"Car Second Hand: No";
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
