

//using System;
//using System.Collections.Generic;
//using System.Net.Http; // Додано для використання HttpClient
//using System.Text; // Додано для використання Encoding
//using System.Threading;
//using System.Threading.Tasks;
//using Newtonsoft.Json; // Додано для використання JSON серіалізації
//using Telegram.Bot;
//using Telegram.Bot.Exceptions;
//using Telegram.Bot.Polling;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.Enums;
//using Telegram.Bot.Types.ReplyMarkups;

//namespace Bot
//{
//    public class GardenAssistantBot
//    {

//        private readonly CancellationTokenSource cts = new CancellationTokenSource();
//        private readonly HttpClient httpClient = new HttpClient(); // Додано HttpClient для роботи з API
//        private TelegramBotClient botClient = new TelegramBotClient(Constants.Token);
//        //        private CancellationTokenSource cts = new CancellationTokenSource();
//        private Database database = new Database();
//        private WeatherClients weatherClients = new WeatherClients();

//        public async Task Start()
//        {
//            var receiverOptions = new ReceiverOptions
//            {
//                AllowedUpdates = { } // receive all update types
//            };

//            botClient.StartReceiving(
//                HandleUpdateAsync,
//                HandleErrorAsync,
//                receiverOptions,
//                cancellationToken: cts.Token
//            );

//            var botMe = await botClient.GetMeAsync();
//            Console.WriteLine($"Бот {botMe.Username} почав працювати.");
//        }

//        private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
//        {
//            var errorMessage = exception switch
//            {
//                ApiRequestException apiRequestException => $"Помилка в Telegram API:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
//                _ => exception.ToString()
//            };

//            Console.WriteLine(errorMessage);
//            return Task.CompletedTask;
//        }

//        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
//        {
//            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
//            {
//                await HandleMessageAsync(botClient, update.Message);
//            }
//            else if (update.Type == UpdateType.CallbackQuery)
//            {
//                await HandleCallbackQueryAsync(botClient, update.CallbackQuery);
//            }
//        }

//        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
//        {
//            if (message.Text == "/start")
//            {
//                await botClient.SendTextMessageAsync(message.Chat.Id, "Привіт, я бот-помічник, буду радий тобі допомогти. Моя основна функція вберегти рослини від несприятливих умов. Використовуйте /keyboard для перегляду меню.");
//                return;
//            }
//            else if (message.Text == "Погода")
//            {
//                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
//                {
//                    new []
//                    {
//                        InlineKeyboardButton.WithCallbackData("Погода в Києві", "WeatherKiyv"),
//                        InlineKeyboardButton.WithCallbackData("Погода у Львові", "WeatherLviv"),
//                        InlineKeyboardButton.WithCallbackData("Ввести координати", "EnterCoordinates"),

//                    }
//                });
//                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть опцію:", replyMarkup: inlineKeyboard);
//                return;
//            }
//            else if (message.Text == "/keyboard")
//            {
//                ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new[]
//                {
//                    new KeyboardButton[] { "Поради по догляду за вашими рослинами", "Управління рослинами" },
//                     new KeyboardButton[] { "Погода", "Додати рослину" }
//                })
//                {
//                    ResizeKeyboard = true
//                };
//                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть опцію:", replyMarkup: replyKeyboard);
//                return;
//            }
//            else if (message.Text == "Додати рослину")
//            {
//                await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /addplant - Яблуня Білий налив - Любить калійні добрива");
//            }
//            else if (message.Text == "Управління рослинами")
//            {

//                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
//                {
//                    new []
//                    {

//                        //InlineKeyboardButton.WithCallbackData("Додати рослину", "AddPlant"),
//                        InlineKeyboardButton.WithCallbackData("Видалити рослину", "RemovePlant"),
//                        InlineKeyboardButton.WithCallbackData("Переглянути рослини", "ViewPlants")
//                    }
//                });
//                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть опцію:", replyMarkup: inlineKeyboard);
//                return;
//            }
//            else if (message.Text == "Поради по догляду за вашими рослинами")
//            {
//                var response = await httpClient.GetAsync($"https://localhost:7164/plants/{message.Chat.Id}");

//                if (response.IsSuccessStatusCode)
//                {
//                    var plants = JsonConvert.DeserializeObject<List<Plant>>(await response.Content.ReadAsStringAsync());
//                    if (plants.Count > 0)
//                    {
//                        string messageText = "Ваші рослини:";
//                        foreach (var plant in plants)
//                        {
//                            messageText += $"\nРослина: {plant.PlantName},\n Рекомендація: {plant.Recommendation}\n";
//                        }
//                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
//                    }
//                    else
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ви ще не додали жодної рослини.");
//                    }
//                }
//                else
//                {
//                    await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при отриманні списку рослин.");
//                }
//            }
//            else if (message.Text.StartsWith("/coords"))
//            {
//                var parts = message.Text.Split(' ');
//                if (parts.Length == 3 && double.TryParse(parts[1], out double lat) && double.TryParse(parts[2], out double lon))
//                {
//                    var weather = await weatherClients.GetWeatherClients(lat, lon);
//                    double temperatureCelsius = weather.list[0].main.temp - 273.15;
//                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Температура в {weather.city.name}: {temperatureCelsius:F1}°C\n" +
//                        $"Швидкість вітру: {weather.list[0].wind.speed} м/с\n" +
//                        $"Ймовірність опадів: {weather.list[0].pop * 100}%");
//                    if (temperatureCelsius > 12)
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, $"На дворі сприятлива температура");
//                    }
//                    if (weather.list[0].wind.speed > 6)
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Є небезрека від сильного вітру");
//                    }
//                    else
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Не має сильного вітру");
//                    }
//                    if (weather.list[0].pop > 0.8)
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Іде дощ");
//                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Рослини не потрібно поливати");
//                    }
//                }
//                else
//                {
//                    await botClient.SendTextMessageAsync(message.Chat.Id, "Неправильний формат. Використовуйте /coords <lat> <lon> через пробіл");
//                }
//            }
//            else if (message.Text.StartsWith("/addplant"))
//            {
//                // Обробка додавання рослини
//                var parts = message.Text.Split('-'); // Розділяємо текст повідомлення на частини
//                string plantName = message.Text.Substring(10).Trim();
//                if (!string.IsNullOrEmpty(plantName) && parts.Length == 3)
//                {
//                    DateTime dateAdded = DateTime.Now;
//                    string recommendation = parts[2].Trim(); // Отримуємо рекомендацію
//                    string name = parts[1].Trim(); // Отримуємо назву рослини
//                    var plant = new Plant
//                    {
//                        ChatId = message.Chat.Id,
//                        PlantName = name,
//                        DateAdded = dateAdded,
//                        Recommendation = recommendation
//                    };

//                    // Надсилаємо запит до контролера для додавання рослини
//                    var content = new StringContent(JsonConvert.SerializeObject(plant), Encoding.UTF8, "application/json");
//                    var response = await httpClient.PostAsync($"https://localhost:7164/plants", content);

//                    if (response.IsSuccessStatusCode)
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Рослину '{name}' додано з рекомендацією: {recommendation}");
//                    }
//                    else
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при додаванні рослини.");
//                    }
//                }
//                else
//                {
//                    await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /addplant - Яблуня Білий налив - Любить калійні добрива");
//                }
//            }
//            else if (message.Text.StartsWith("/removeplant"))
//            {
//                // Обробка видалення рослини
//                string plantName = message.Text.Substring(12).Trim();
//                if (!string.IsNullOrEmpty(plantName))
//                {
//                    // Надсилаємо запит до контролера для видалення рослини
//                    var response = await httpClient.DeleteAsync($"https://localhost:7164/plants/{message.Chat.Id}/{plantName}");

//                    if (response.IsSuccessStatusCode)
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Рослину '{plantName}' видалено.");
//                    }
//                    else
//                    {
//                        await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при видаленні рослини.");
//                    }
//                }
//                else
//                {
//                    await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /removeplant");
//                }
//            }
//        }

//        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
//        {
//            if (callbackQuery.Message == null || callbackQuery.Message.Chat == null)
//                return;

//            if (callbackQuery.Data.StartsWith("Weather"))
//            {
//                double lat = 0, lon = 0;
//                string city = "";
//                if (callbackQuery.Data == "WeatherKiyv")
//                {
//                    lat = 50.4501;
//                    lon = 30.5234;
//                    city = "Київ";
//                }
//                else if (callbackQuery.Data == "WeatherLviv")
//                {
//                    lat = 49.8397;
//                    lon = 24.0297;
//                    city = "Львів";
//                }

//                var weather = await weatherClients.GetWeatherClients(lat, lon);
//                double temperatureCelsius = weather.list[0].main.temp - 273.15;
//               await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Температура в {weather.city.name}: {temperatureCelsius:F1}°C\n" +
//                        $"Швидкість вітру: {weather.list[0].wind.speed} м/с\n" +
//                        $"Ймовірність опадів: {weather.list[0].pop*100}%");
//                    if(temperatureCelsius > 12)
//                    {
//                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"На дворі сприятлива температура");
//                    }
//                    if(weather.list[0].wind.speed>6)
//                    {
//                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Є небезрека від сильного вітру");
//                    }
//                    else
//                    {
//                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Не має сильного вітру");
//                    }
//                    if (weather.list[0].pop > 0.8)
//                    {
//                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Іде дощ");
//                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Рослини не потрібно поливати");
//                    }
//            }
//            else if (callbackQuery.Data == "EnterCoordinates")
//            {
//                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введіть координати у форматі /coords <lat> <lon>\n" +
//                    "Наприклад: /coords 50.345 30.543\n" +
//                    "обовязково через пробіл");
//            }
//            //else if (callbackQuery.Data == "AddPlant")
//            //{
//            //    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введіть назву рослини після команди /addplant:");
//            //}
//            else if (callbackQuery.Data == "RemovePlant")
//            {
//                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введіть назву рослини після команди /removeplant:");
//            }
//            else if (callbackQuery.Data == "ViewPlants")
//            {
//                // Отримання списку рослин з контролера
//                var response = await httpClient.GetAsync($"https://localhost:7164/plants/{callbackQuery.Message.Chat.Id}");

//                if (response.IsSuccessStatusCode)
//                {
//                    var plants = JsonConvert.DeserializeObject<List<Plant>>(await response.Content.ReadAsStringAsync());
//                    if (plants.Count > 0)
//                    {
//                        string messageText = "Ваші рослини:";
//                        foreach (var plant in plants)
//                        {
//                            messageText += $"\nРослина: {plant.PlantName},\n Додано: {plant.DateAdded.ToShortDateString()},\n ";
//                        }
//                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, messageText);
//                    }
//                    else
//                    {
//                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Ви ще не додали жодної рослини.");
//                    }
//                }
//                else
//                {
//                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Помилка при отриманні списку рослин.");
//                }
//            }
//        }


//    }
//}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot
{
    public class GardenAssistantBot
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly HttpClient httpClient = new HttpClient();
        private TelegramBotClient botClient = new TelegramBotClient(Constants.Token);
        private Database database = new Database();
        private WeatherClients weatherClients = new WeatherClients();

        public async Task Start()
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            var botMe = await botClient.GetMeAsync();
            Console.WriteLine($"Бот {botMe.Username} почав працювати.");
        }

        private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Помилка в Telegram API:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                await HandleMessageAsync(botClient, update.Message);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await HandleCallbackQueryAsync(botClient, update.CallbackQuery);
            }
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message)
        {
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Привіт, я бот-помічник, буду радий тобі допомогти. Моя основна функція вберегти рослини від несприятливих умов. Використовуйте /keyboard для перегляду меню.");
                return;
            }
            else if (message.Text == "Погода")
            {
                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Погода в Києві", "WeatherKiyv"),
                        InlineKeyboardButton.WithCallbackData("Погода у Львові", "WeatherLviv"),
                        InlineKeyboardButton.WithCallbackData("Ввести координати", "EnterCoordinates"),
                    }
                });
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть опцію:", replyMarkup: inlineKeyboard);
                return;
            }
            else if (message.Text == "/keyboard")
            {
                ReplyKeyboardMarkup replyKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton[] { "Поради по догляду за вашими рослинами", "Управління рослинами" },
                    new KeyboardButton[] { "Погода", "Додати рослину" }
                })
                {
                    ResizeKeyboard = true
                };
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть опцію:", replyMarkup: replyKeyboard);
                return;
            }
            else if (message.Text == "Додати рослину")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /addplant - Яблуня Білий налив - Любить калійні добрива");
            }
            else if (message.Text == "Управління рослинами")
            {
                InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Видалити рослину", "RemovePlant"),
                        InlineKeyboardButton.WithCallbackData("Переглянути рослини", "ViewPlants"),
                        InlineKeyboardButton.WithCallbackData("Замінити рослину", "ReplacePlant")
                    }
                });
                await botClient.SendTextMessageAsync(message.Chat.Id, "Виберіть опцію:", replyMarkup: inlineKeyboard);
                return;
            }
            else if (message.Text == "Поради по догляду за вашими рослинами")
            {
                var response = await httpClient.GetAsync($"https://localhost:7164/plants/{message.Chat.Id}");

                if (response.IsSuccessStatusCode)
                {
                    var plants = JsonConvert.DeserializeObject<List<Plant>>(await response.Content.ReadAsStringAsync());
                    if (plants.Count > 0)
                    {
                        string messageText = "Ваші рослини:";
                        foreach (var plant in plants)
                        {
                            messageText += $"\nРослина: {plant.PlantName},\n Рекомендація: {plant.Recommendation}\n";
                        }
                        await botClient.SendTextMessageAsync(message.Chat.Id, messageText);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Ви ще не додали жодної рослини.");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при отриманні списку рослин.");
                }
            }
            else if (message.Text.StartsWith("/coords"))
            {
                var parts = message.Text.Split(' ');
                if (parts.Length == 3 && double.TryParse(parts[1], out double lat) && double.TryParse(parts[2], out double lon))
                {
                    var weather = await weatherClients.GetWeatherClients(lat, lon);
                    double temperatureCelsius = weather.list[0].main.temp - 273.15;
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"Температура в {weather.city.name}: {temperatureCelsius:F1}°C\n" +
                        $"Швидкість вітру: {weather.list[0].wind.speed} м/с\n" +
                        $"Ймовірність опадів: {weather.list[0].pop * 100}%");
                    if (temperatureCelsius > 12)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"На дворі сприятлива температура");
                    }
                    if (weather.list[0].wind.speed > 6)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Є небезрека від сильного вітру");
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Не має сильного вітру");
                    }
                    if (weather.list[0].pop > 0.8)
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Іде дощ");
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"Рослини не потрібно поливати");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Неправильний формат. Використовуйте /coords <lat> <lon> через пробіл");
                }
            }
            else if (message.Text.StartsWith("/addplant"))
            {
                var parts = message.Text.Split('-'); // Розділяємо текст повідомлення на частини
                if (message.Text.Length > 10) // Перевіряємо довжину тексту перед викликом Substring
                {
                    string plantName = message.Text.Substring(10).Trim();
                    if (!string.IsNullOrEmpty(plantName) && parts.Length == 3)
                    {
                        DateTime dateAdded = DateTime.Now;
                        string recommendation = parts[2].Trim(); // Отримуємо рекомендацію
                        string name = parts[1].Trim(); // Отримуємо назву рослини
                        var plant = new Plant
                        {
                            ChatId = message.Chat.Id,
                            PlantName = name,
                            DateAdded = dateAdded,
                            Recommendation = recommendation
                        };

                        // Надсилаємо запит до контролера для додавання рослини
                        var content = new StringContent(JsonConvert.SerializeObject(plant), Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync($"https://localhost:7164/plants", content);

                        if (response.IsSuccessStatusCode)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Рослину '{name}' додано з рекомендацією: {recommendation}");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при додаванні рослини.");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /addplant - Яблуня Білий налив - Любить калійні добрива");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /addplant - Яблуня Білий налив - Любить калійні добрива");
                }
            }
            else if (message.Text.StartsWith("/replacePlant"))
            {
                var parts = message.Text.Split('-'); // Розділяємо текст повідомлення на частини
                if (message.Text.Length > 13) // Перевіряємо довжину тексту перед викликом Substring
                {
                    string plantName = message.Text.Substring(13).Trim();
                    if (!string.IsNullOrEmpty(plantName) && parts.Length == 3)
                    {
                        DateTime dateAdded = DateTime.Now;
                        string recommendation = parts[2].Trim(); // Отримуємо рекомендацію
                        string name = parts[1].Trim(); // Отримуємо назву рослини

                        // Видаляємо рослину з бази даних, якщо вона вже існує
                        await database.RemovePlantAsync(message.Chat.Id, plantName);

                        // Додаємо нову рослину
                        var plant = new Plant
                        {
                            ChatId = message.Chat.Id,
                            PlantName = name,
                            DateAdded = dateAdded,
                            Recommendation = recommendation
                        };

                        // Надсилаємо запит до контролера для додавання рослини
                        var content = new StringContent(JsonConvert.SerializeObject(plant), Encoding.UTF8, "application/json");
                        var response = await httpClient.PostAsync($"https://localhost:7164/plants", content);

                        if (response.IsSuccessStatusCode)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Рослину '{name}' замінено на нову рослину з рекомендацією: {recommendation}");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при заміні рослини.");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /replacePlant - Яблуня Білий налив - Любить калійні добрива");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /replacePlant - Яблуня Білий налив - Любить калійні добрива");
                }
            }
            else if (message.Text.StartsWith("/removeplant"))
            {
                if (message.Text.Length > 12) // Перевіряємо довжину тексту перед викликом Substring
                {
                    string plantName = message.Text.Substring(12).Trim();
                    if (!string.IsNullOrEmpty(plantName))
                    {
                        // Надсилаємо запит до контролера для видалення рослини
                        var response = await httpClient.DeleteAsync($"https://localhost:7164/plants/{message.Chat.Id}/{plantName}");

                        if (response.IsSuccessStatusCode)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, $"Рослину '{plantName}' видалено.");
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Помилка при видаленні рослини.");
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /removeplant");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Будь ласка, введіть назву рослини після команди /removeplant");
                }
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            if (callbackQuery.Message == null || callbackQuery.Message.Chat == null)
                return;

            if (callbackQuery.Data.StartsWith("Weather"))
            {
                double lat = 0, lon = 0;
                string city = "";
                if (callbackQuery.Data == "WeatherKiyv")
                {
                    lat = 50.4501;
                    lon = 30.5234;
                    city = "Київ";
                }
                else if (callbackQuery.Data == "WeatherLviv")
                {
                    lat = 49.8397;
                    lon = 24.0297;
                    city = "Львів";
                }

                var weather = await weatherClients.GetWeatherClients(lat, lon);
                double temperatureCelsius = weather.list[0].main.temp - 273.15;
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Температура в {weather.city.name}: {temperatureCelsius:F1}°C\n" +
                        $"Швидкість вітру: {weather.list[0].wind.speed} м/с\n" +
                        $"Ймовірність опадів: {weather.list[0].pop * 100}%");
                if (temperatureCelsius > 12)
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"На дворі сприятлива температура");
                }
                if (weather.list[0].wind.speed > 6)
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Є небезрека від сильного вітру");
                }
                else
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Не має сильного вітру");
                }
                if (weather.list[0].pop > 0.8)
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Іде дощ");
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Рослини не потрібно поливати");
                }
            }
            else if (callbackQuery.Data == "EnterCoordinates")
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введіть координати у форматі /coords <lat> <lon>\n" +
                    "Наприклад: /coords 50.345 30.543\n" +
                    "обовязково через пробіл");
            }
            else if (callbackQuery.Data == "RemovePlant")
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введіть назву рослини після команди /removeplant:");
            }
            else if (callbackQuery.Data == "ReplacePlant")
            {
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Введіть назву рослини після команди /replacePlant:");
            }
            else if (callbackQuery.Data == "ViewPlants")
            {
                // Отримання списку рослин з контролера
                var response = await httpClient.GetAsync($"https://localhost:7164/plants/{callbackQuery.Message.Chat.Id}");

                if (response.IsSuccessStatusCode)
                {
                    var plants = JsonConvert.DeserializeObject<List<Plant>>(await response.Content.ReadAsStringAsync());
                    if (plants.Count > 0)
                    {
                        string messageText = "Ваші рослини:";
                        foreach (var plant in plants)
                        {
                            messageText += $"\nРослина: {plant.PlantName},\n Додано: {plant.DateAdded.ToShortDateString()},\n ";
                        }
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, messageText);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Ви ще не додали жодної рослини.");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "Помилка при отриманні списку рослин.");
                }
            }
        }
    }
}
