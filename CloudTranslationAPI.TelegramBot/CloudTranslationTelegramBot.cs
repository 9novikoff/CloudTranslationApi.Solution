using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using CloudTranslationAPI.Client;
using Telegram.Bot.Types.ReplyMarkups;

namespace CloudTranslationAPI.TelegramBot;

internal class CloudTranslationTelegramBot(
    string telegramBotToken,
    ChatStateMachine chatStateMachine,
    ICloudTranslationClient cloudTranslationClient)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var botClient = new TelegramBotClient(telegramBotToken);
        
        botClient.StartReceiving(updateHandler: HandleUpdateAsync, pollingErrorHandler: HandlePollingErrorAsync, cancellationToken: cancellationToken);

        return Task.CompletedTask;
    }
    
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        if (message.Text is not { } text)
            return;

        var chatId = message.Chat.Id;

        var state = chatStateMachine.ProcessInput(chatId, text.ToLower());
        
        var sourceKeyboard = GetLanguagesOneTimeKeyboard(17);

        switch (state.StateCode)
        {
            case StateCode.SourceLanguage:
                await botClient.SendTextMessageAsync(chatId: chatId, text: TelegramBotResponses.EnterSourceLanguage, replyMarkup: sourceKeyboard,  cancellationToken: cancellationToken);
                break;
            
            case StateCode.Text:   
                await botClient.SendTextMessageAsync(chatId: chatId, text: TelegramBotResponses.EnterText, cancellationToken: cancellationToken);
                break;
            
            case StateCode.TargetLanguage:
                await botClient.SendTextMessageAsync(chatId: chatId, text: TelegramBotResponses.EnterTargetLanguage, replyMarkup: sourceKeyboard, cancellationToken: cancellationToken);
                break;
            
            case StateCode.Final:
                if (state.Validate())
                {
                    try
                    {
                        var response = await cloudTranslationClient.TranslateAsync(new TranslationRequest(state.Text!,
                            state.TargetLanguage!, null, state.SourceLanguage));
                        await botClient.SendTextMessageAsync(chatId: chatId, text: string.Join("\n", 
                            response.Data.Translations.Select(t => t.TranslatedText).ToArray()), cancellationToken: cancellationToken);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        await botClient.SendTextMessageAsync(chatId: chatId, text: TelegramBotResponses.ErrorMessage, cancellationToken: cancellationToken);
                    }
                }
                chatStateMachine.SetSourceLanguageState(chatId);
                await botClient.SendTextMessageAsync(chatId: chatId, text: TelegramBotResponses.EnterSourceLanguage, replyMarkup: sourceKeyboard,  cancellationToken: cancellationToken);
                break;
        }        

    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

    private ReplyKeyboardMarkup GetLanguagesOneTimeKeyboard(int buttonsInRowAmount)
    {
        var languages = LanguageCodes.GetLanguagesCodes();
        
        var buttonsInColumnAmount = (int)Math.Ceiling((float)languages.Count / buttonsInRowAmount);
        
        var buttons = new List<List<KeyboardButton>>();

        for (int i = 0; i < buttonsInColumnAmount; i++)
        {
            var row = new List<KeyboardButton>();
            for (int j = 0; j < buttonsInRowAmount; j++)
            {
                var languageIndex = i * buttonsInRowAmount + j;
                if(languageIndex < languages.Count)
                    row.Add(new KeyboardButton(languages[i * buttonsInRowAmount + j]));    
            }
            buttons.Add(row);
        }

        return new ReplyKeyboardMarkup(buttons)
        {
            OneTimeKeyboard = true
        };
    }
}