using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using var cts = new CancellationTokenSource();

var bot = new TelegramBotClient("7692431379:AAFKnLr7AHVrhXT4XxMSLGNEyvYEs0CpIm8", cancellationToken: cts.Token);
var me = await bot.GetMe();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();
cts.Cancel();

async Task OnError(Exception exception, HandleErrorSource source)
{
	Console.WriteLine(exception);
}

async Task OnMessage(Message msg, UpdateType type)
{

	if (msg.Chat.Type == ChatType.Supergroup || msg.Chat.Type == ChatType.Group)
	{
		if (msg.Text != null)
		{
			Console.WriteLine(msg.From.Username + " - \"" + msg?.Text + "\" - " + msg?.From?.Id);
			if (msg?.Text == "/ban")
			{
				try
				{
					var chatAdmins = await bot.GetChatAdministrators(msg.Chat.Id);

					var isAdmin = chatAdmins.Any(admin => admin.User.Id == msg?.From?.Id);

					if (!isAdmin)
					{
						await bot.SendMessage(msg.Chat.Id, "Ви повинні бути адміністратором, щоб використовувати цю команду.");
						return;
					}

					if (msg.ReplyToMessage != null)
					{
						var userIdToBan = msg.ReplyToMessage.From.Id;

						await bot.BanChatMember(msg.Chat.Id, userIdToBan);

						await bot.SendMessage(msg.Chat.Id, $"Користувач {msg?.ReplyToMessage?.From?.Username} був забанений.");
					}
					else
					{
						await bot.SendMessage(msg.Chat.Id, "Будь ласка, відповідайте на повідомлення користувача, якого ви хочете забанити.");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
					await bot.SendMessage(msg.Chat.Id, "Сталася помилка при спробі забанити користувача.");
				}
			}
			else if (msg?.Text == "/mute")
			{
				try
				{
					var chatAdmins = await bot.GetChatAdministrators(msg.Chat.Id);

					var isAdmin = chatAdmins.Any(admin => admin.User.Id == msg?.From?.Id);

					if (!isAdmin)
					{
						await bot.SendMessage(msg.Chat.Id, "Ви повинні бути адміністратором, щоб використовувати цю команду.");
						return;
					}

					if (msg.ReplyToMessage != null)
					{
						var userIdToBan = msg.ReplyToMessage.From.Id;

						await bot.RestrictChatMember(msg.Chat.Id, userIdToBan, new ChatPermissions {CanSendMessages = false});

						await bot.SendMessage(msg.Chat.Id, $"Користувач {msg?.ReplyToMessage?.From?.Username} був заткнутий.");
					}
					else
					{
						await bot.SendMessage(msg.Chat.Id, "Будь ласка, відповідайте на повідомлення користувача, якого ви хочете заткнути.");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
					await bot.SendMessage(msg.Chat.Id, "Сталася помилка при спробі заткнути користувача.");
				}
			}

			else if (msg?.Text == "/getAdmins")
			{
				try
				{
					ChatMember[] admins = await bot.GetChatAdministrators(msg.Chat.Id);
					foreach (var item in admins)
					{
						await bot.SendMessage(msg.Chat.Id, $"Admin: {item.User}");
					}
				}
				catch(Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

			else if (msg?.Text == "/warning")
			{
				try
				{
					var chatAdmins = await bot.GetChatAdministrators(msg.Chat.Id);

					var isAdmin = chatAdmins.Any(admin => admin.User.Id == msg?.From?.Id);

					if (!isAdmin)
					{
						await bot.SendMessage(msg.Chat.Id, "Ви повинні бути адміністратором, щоб використовувати цю команду.");
						return;
					}

					if (msg.ReplyToMessage != null)
					{
						var userIdToBan = msg.ReplyToMessage.From.Id;

						await bot.SendMessage(msg.Chat.Id, $"Попередження користувачу {msg?.ReplyToMessage?.From?.Username}.");
					}
					else
					{
						await bot.SendMessage(msg.Chat.Id, "Будь ласка, відповідайте на повідомлення користувача, якому ви хочете видати попередження.");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error: {ex.Message}");
					await bot.SendMessage(msg.Chat.Id, "Сталася помилка при спробі заткнути користувача.");
				}
			}
		}
		if (msg.NewChatMembers != null && msg.NewChatMembers.Length > 0)
		{
			foreach (var newMember in msg.NewChatMembers)
			{
				Random random = new Random();
				int num1 = random.Next(1, 9);
				int num2 = random.Next(1, 9);
				int correctAnswer = num1 + num2;

				if (newMember.IsBot)
					continue;

				string welcomeMessage = $"Привіт, {newMember.Username}! Ласкаво просимо до нашої групи! Будь ласка пройдіть капчу \"{num1} + {num2}\", у вас є 10 секунд!";
				var sentMessage = await bot.SendMessage(msg.Chat.Id, welcomeMessage);

				bool isCorrectAnswer = false;
				DateTime timeout = DateTime.UtcNow.AddSeconds(10);

				while (DateTime.UtcNow < timeout)
				{
					var updates = await bot.GetUpdates();
					var userMessages = updates.Where(u => u.Message?.From?.Id == newMember.Id && u.Message?.Chat?.Id == msg.Chat.Id).ToList();

					foreach (var update in userMessages)
					{
						if (update.Message?.Text != null && int.TryParse(update.Message.Text, out int userAnswer))
						{
							if (userAnswer == correctAnswer)
							{
								isCorrectAnswer = true;
								break;
							}
						}
					}

					if (isCorrectAnswer) break;

					await Task.Delay(1000);
				}

				if (isCorrectAnswer)
				{
					await bot.SendMessage(msg.Chat.Id, $"Капча пройдена успішно, {newMember.Username}!");
				}
				else
				{
					await bot.SendMessage(msg.Chat.Id, $"Час вийшов або відповідь неправильна, {newMember.Username}. Ви будете видалені.");
					await bot.BanChatMember(msg.Chat.Id, newMember.Id);
				}
			}
		}

	}
}

async Task OnUpdate(Update update)
{
	if (update is { CallbackQuery: { } query })
	{
		await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
		await bot.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
	}
}