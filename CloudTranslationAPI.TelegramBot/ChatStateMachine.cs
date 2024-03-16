namespace CloudTranslationAPI.TelegramBot;

public class ChatStateMachine
{
    private readonly Dictionary<long, State> _chatStates = new();

    public State ProcessInput(long chatId, string input)
    {
        if (!_chatStates.TryGetValue(chatId, out var chatState))
        {
            var state = new State()
            {
                StateCode = StateCode.SourceLanguage
            };
            
            _chatStates.Add(chatId, state);

            return state;
        }
        switch (chatState.StateCode)
        {
            case StateCode.SourceLanguage:
                chatState.StateCode = StateCode.Text;
                chatState.SourceLanguage = input;
                return chatState;
            
            case StateCode.Text:
                chatState.StateCode = StateCode.TargetLanguage;
                chatState.Text = input;
                return chatState;
            
            case StateCode.TargetLanguage when !LanguageCodes.IsValidLanguageCode(input):
                return chatState;
            
            case StateCode.TargetLanguage:
                chatState.StateCode = StateCode.Final;
                chatState.TargetLanguage = input;
                return chatState;
            
            default:
                throw new ArgumentException("Not valid state machine input");
        }
    }

    public void RemoveChat(long chatId)
    {
        _chatStates.Remove(chatId);
    }
}

public class State
{
    private string? _sourceLanguage;
    public StateCode StateCode { get; set; }

    public string? SourceLanguage
    {
        get => _sourceLanguage;
        set {
            if (value == null || !LanguageCodes.IsValidLanguageCode(value))
            {
                _sourceLanguage = null;
            }
            else
            {
                _sourceLanguage = value;
            }
        }
    }
    public string? Text { get; set; }
    public string? TargetLanguage { get; set; }

    public bool Validate()
    {
        if (Text == null || TargetLanguage == null)
        {
            return false;
        }

        return LanguageCodes.IsValidLanguageCode(TargetLanguage) &&
               (SourceLanguage == null || LanguageCodes.IsValidLanguageCode(SourceLanguage));
    }
}

public enum StateCode
{
    SourceLanguage,
    Text,
    TargetLanguage,
    Final
}