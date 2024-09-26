using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using Newtonsoft.Json;
using Microsoft.BotBuilderSamples;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Data.SqlClient;

namespace ProfileBot.Bots
{
	public class UserProfileDialog : ComponentDialog
	{
		private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;


		public UserProfileDialog(UserState userState)
		: base(nameof(UserProfileDialog))
		{
			_userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

			var waterfallSteps = new WaterfallStep[]
			{
					NameStepAsync,
					AgeStepAsync,
					CityStepAsync,
					PhoneNoStepAsync,
					EmailStepAsync,
					OccStepAsync,
					SummaryStepAsync,
					ContinueOrEndStepAsync

			};

			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new TextPrompt("PhonePrompt", PhonePromptValidatorAsync));
			AddDialog(new TextPrompt("EmailPrompt", EmailPromptValidatorAsync));

			InitialDialogId = nameof(WaterfallDialog);
		}

		

		private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
		}

		private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			stepContext.Values["name"] = (string)stepContext.Result;
			return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = MessageFactory.Text("Please enter your age.") }, cancellationToken);
		}

		private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
		{
			// Ensure age is greater than 10 and less than 150
			return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 10 && promptContext.Recognized.Value < 60);
		}


		//private async Task<DialogTurnResult> CityStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		//{
		//	stepContext.Values["age"] = (int)stepContext.Result;
		//	var msg = (int)stepContext.Values["age"] == -1 ? "No age given." : $"I have your age as {stepContext.Values["age"]}.";
		//	return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("select your gender") }, cancellationToken);
		//}

		private async Task<DialogTurnResult> CityStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			stepContext.Values["age"] = (int)stepContext.Result;
			var choices = new List<Choice>
			{
				new Choice { Value = "Male" },
				new Choice { Value = "Female" }
			};

			return await stepContext.PromptAsync(nameof(ChoicePrompt),
				new PromptOptions
				{
					Prompt = MessageFactory.Text("Please select your gender."),
					Choices = choices
				},
				cancellationToken);
		}

		//private async Task<DialogTurnResult> PhoneNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		//{
		//	stepContext.Values["gender"] = (string)stepContext.Result;
		//	return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your phone number.") }, cancellationToken);
		//}

		private static Task<bool> PhonePromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
		{
			var phoneNumber = promptContext.Recognized.Value;

			// Validate if the phone number contains exactly 10 digits
			if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.All(char.IsDigit) && phoneNumber.Length == 10)
			{
				return Task.FromResult(true);
			}
			else
			{
				return Task.FromResult(false);
			}
		}

		private async Task<DialogTurnResult> PhoneNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			stepContext.Values["gender"] = ((FoundChoice)stepContext.Result).Value;

			return await stepContext.PromptAsync("PhonePrompt", // Using the new phone validator prompt
				new PromptOptions
				{
					Prompt = MessageFactory.Text("Please enter your phone number-"),
					RetryPrompt = MessageFactory.Text("The phone number must be exactly 10 digits. Please enter a valid phone number.")
				},
				cancellationToken);
		}

		//private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		//{
		//	stepContext.Values["phonenumber"] = (string)stepContext.Result;
		//	return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your email address.") }, cancellationToken);
		//}

		private static Task<bool> EmailPromptValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
		{
			var email = promptContext.Recognized.Value;

			// Validate if the email contains '@' and ends with '@fnf.com'
			if (!string.IsNullOrEmpty(email) && email.Contains("@") && email.EndsWith("@fnf.com"))
			{
				return Task.FromResult(true);
			}
			else
			{
				return Task.FromResult(false);
			}
		}

		private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			stepContext.Values["phonenumber"] = (string)stepContext.Result;

			return await stepContext.PromptAsync("EmailPrompt", // Using the new email validator prompt
				new PromptOptions
				{
					Prompt = MessageFactory.Text("Please enter your email address."),
					RetryPrompt = MessageFactory.Text("The email must end with '@fnf.com'. Please enter a valid email address.") // Custom error message
				},
				cancellationToken);
		}
		private async Task<DialogTurnResult> OccStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			stepContext.Values["email"] = (string)stepContext.Result;
			//return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your ocupation") }, cancellationToken);

			var choices = new List<Choice>
			{
				new Choice { Value = "Student" },
				new Choice { Value = "Working" },
				new Choice { Value = "Other"}
			};

			return await stepContext.PromptAsync(nameof(ChoicePrompt),
				new PromptOptions
				{
					Prompt = MessageFactory.Text("Please select your ocupation"),
					Choices = choices
				},
				cancellationToken);
		}

		private async Task<DialogTurnResult> ContinueOrEndStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			var choice = ((FoundChoice)stepContext.Result).Value;

			if (choice == "Yes")
			{
				// Restart the dialog from the beginning
				return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
			}
			else
			{
				// End the dialog with a thank you message
				await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you! Have a great day!"), cancellationToken);
				return await stepContext.EndDialogAsync(null, cancellationToken);
			}
		}

		private async Task<DialogTurnResult> ImageUrlStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
{
    stepContext.Values["ocupation"] = ((FoundChoice)stepContext.Result).Value;

    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
    {
        Prompt = MessageFactory.Text("Please enter the URL of your profile image."),
        RetryPrompt = MessageFactory.Text("The URL you entered is invalid. Please enter a valid image URL.")
    }, cancellationToken);
}

		private async Task SaveUserProfileToDatabase(UserProfile userProfile)
		{
			var connectionString = "Data Source=FNFIDVPRE20500;Initial Catalog=UserProfileDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";

			using (var connection = new SqlConnection(connectionString))
			{
				await connection.OpenAsync();

				var query = "INSERT INTO UserProfile (Name, Age, Gender, PhoneNumber, Email, Occupation) " +
							"VALUES (@Name, @Age, @Gender, @PhoneNumber, @Email, @Occupation)";

				using (var command = new SqlCommand(query, connection))
				{
					command.Parameters.AddWithValue("@Name", userProfile.Name);
					command.Parameters.AddWithValue("@Age", userProfile.Age);
					command.Parameters.AddWithValue("@Gender", userProfile.Gender);
					command.Parameters.AddWithValue("@PhoneNumber", userProfile.PhoneNumber);
					command.Parameters.AddWithValue("@Email", userProfile.Email);
					command.Parameters.AddWithValue("@Occupation", userProfile.Ocupation);

					await command.ExecuteNonQueryAsync();
				}
			}
		}


		private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			stepContext.Values["ocupation"] = ((FoundChoice)stepContext.Result).Value;

			var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

			userProfile.Name = (string)stepContext.Values["name"];
			userProfile.Age = (int)stepContext.Values["age"];
			userProfile.Gender = (string)stepContext.Values["gender"];
			userProfile.Email = (string)stepContext.Values["email"];
			userProfile.PhoneNumber = (string)stepContext.Values["phonenumber"];
			userProfile.Ocupation = (string)stepContext.Values["ocupation"];

			// Save the user profile to the database
			await SaveUserProfileToDatabase(userProfile);

			var card = CreateProfileAdaptiveCard(userProfile);
			var attachment = new Attachment
			{
				ContentType = AdaptiveCard.ContentType,
				Content = JsonConvert.DeserializeObject(card)
			};

			await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);

			// Ask if the user wants to continue
			var choices = new List<Choice>
	{
		new Choice { Value = "Yes" },
		new Choice { Value = "No" }
	};

			return await stepContext.PromptAsync(nameof(ChoicePrompt),
				new PromptOptions
				{
					Prompt = MessageFactory.Text("Do you want to continue?"),
					Choices = choices
				},
				cancellationToken);
		}


		//private async Task<DialogTurnResult> ShowCardStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		//{
		//	reply.Attachments.Add(Cards.GetVideoCard().ToAttachment());
		//	await stepContext.Context.SendActivityAsync(reply, cancellationToken);
		//	return await stepContext.EndDialogAsync();

		//}
		private string CreateProfileAdaptiveCard(UserProfile userProfile)
		{
			var card = $@"
            {{
                ""type"": ""AdaptiveCard"",
                ""version"": ""1.2"",
                ""body"": [
                    {{
                        ""type"": ""TextBlock"",
                        ""text"": ""User Profile"",
                        ""weight"": ""Bolder"",
                        ""size"": ""Medium""
                    }},
                    {{
                        ""type"": ""FactSet"",
                        ""facts"": [
                            {{
                                ""title"": ""Name:"",
                                ""value"": ""{userProfile.Name}""
                            }},
                            {{
                                ""title"": ""Age:"",
                                ""value"": ""{userProfile.Age}""
                            }},
                            {{
                                ""title"": ""Gender:"",
                                ""value"": ""{userProfile.Gender}""
                            }},
                            {{
                                ""title"": ""Phone Number:"",
                                ""value"": ""{userProfile.PhoneNumber}""
                            }},
                            {{
                                ""title"": ""Email:"",
                                ""value"": ""{userProfile.Email}""
                            }},
							{{
								""title"":""Ocupation"",
								""value"":""{userProfile.Ocupation}""
							}}
                        ]
                    }}
                ],
                
            }}";

			return card;
		}
	}
}