using System.Text;
using OllamaSharp;
using TL;

namespace sample;

internal enum CommandType
{
    Send,
    Summerize
}

internal struct Command
{
    public CommandType CommandType { get; set; }
    public string UserName { get; set; }
    public string Message { get; set; }
}

internal struct CommandR
{
    public string CommandType { get; set; }
    public string UserName { get; set; }
    public string Message { get; set; }
}


class MainClass
{
    private static  WTelegram.Client _client;
    public static async Task Main()
    {
        WTelegram.Helpers.Log = (i, s) => { };
        _client =  new WTelegram.Client( int .Parse(Environment.GetEnvironmentVariable("APP_ID") ?? throw new InvalidOperationException()) ,
            Environment.GetEnvironmentVariable("API_HASH") ?? throw new InvalidOperationException(), "./SESSION.session");
        DoLogin(loginInfo: Environment.GetEnvironmentVariable("PHONE_NUMBER") ?? throw new InvalidOperationException()).Wait();
        Console.WriteLine("hello to your smart telegram client!");
        Console.WriteLine("how can i help you?");
        while (true)
        {
            var query = Console.ReadLine();
            if (query == "exit")
            {
                break;
            }
            var response = AiResponseGenerator.GenerateResponse(query).Result;
            var command = ParseCommandFromString(response);
            switch (command.CommandType)
            {
                case CommandType.Send:
                    await _client.SendMessageAsync(
                        _client.Contacts_ResolveUsername(command.UserName.Replace("@",string.Empty)).Result.User.ToInputPeer(),
                        command.Message);
                    break;
                case CommandType.Summerize:
                   var messages = await _client.Messages_GetHistory(_client
                        .Contacts_ResolveUsername(command.UserName.Replace("@", string.Empty)).Result.User
                        .ToInputPeer());
                   var finalBlob = new StringBuilder();
                   var limited = messages.Messages.Take(10);
                   foreach (var message in limited)
                   {
                       var from = messages.UserOrChat(message.From ?? message.Peer); 
                       if (message is Message msg)
                       {
                           finalBlob.AppendLine($"from : {from}> {msg.message}");
                       }
                   }
                   var result = await AiResponseGenerator.GenerateSummerizeResponse(finalBlob.ToString());
                   Console.WriteLine(result);
                   break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Console.WriteLine(response);
        }
        
    }
    private static async Task DoLogin(string? loginInfo) 
    {
        while (_client.User == null)
        {
            var r = await _client.Login(loginInfo);
            switch (r) 
            {
                case "verification_code": 
                    Console.Write("Code: ");
                    loginInfo =  Console.ReadLine();
                    break;
                case "password": 
                    Console.Write("TF Code: ");
                    loginInfo = Console.ReadLine();
                    break;
                default: loginInfo = null; break;
            }
        }
        Console.WriteLine($"We are logged-in as {_client.User} (id {_client.User.id})");
    }


    private static Command ParseCommandFromString(string command)
    {
        var processed = System.Text.Json.JsonSerializer.Deserialize<CommandR>(command);
        return new Command()
        {
            CommandType = processed.CommandType.ToUpper() switch
            {
                "SEND" => CommandType.Send,
                "SUMMERIZE" => CommandType.Summerize
            },
            UserName = processed.UserName,
            Message = processed.Message
        };
    }
}

public static class AiResponseGenerator
{

    private const string COMMAND_PREPEX = """
                                  Imagine you are a smart assistante in a an instante messenger.
                                  you can perform 2 tasks:
                                  1- you can send a message to a user
                                  2- you can summerize a chat
                                  you need to process the user input below and classify the command and the arguments
                                  note that if you classify the input as SEND, you may need to generate message yourself based on the deduction you made from the user input
                                  note that usernames have a @ at the begginging. do not remove any underline or hyphen from username
                                  you need to output a json like this :
                                  {
                                    CommandType: "SEND" (or SUMMERIZE),
                                    UserName : (the username from user input),
                                    UserName : (the actual message)
                                  }

                                  just output the json, nothing else
                                  
                                  Here is the userinput:
                                  
                                  """;

    private const string SUMMERIZE_PREPEX = """
                                            please summerize the conversion below and only return the summery and nothing from you side :
                                            
                                            """;
    public static async Task<string> GenerateResponse(string query)
    {
        ConversationContextWithResponse context = null;
        var ollama = new OllamaApiClient(new Uri(Environment.GetEnvironmentVariable("ollama_url") ?? throw new InvalidOperationException("ollama_url is not set")));
        Console.WriteLine("Getting models...");
        var models = (await ollama.ListLocalModels()).ToList();
        if (models.Count == 0)
        {
            throw new InvalidOperationException("No models found\n try to install one like `ollama pull llama2`");
        }
        var modelName = Environment.GetEnvironmentVariable("model_name") ?? models.First().Name;
        ollama.SelectedModel = modelName;
        Console.WriteLine("using model: " + modelName);
        context = await ollama.GetCompletion(COMMAND_PREPEX + query , context);
        return context.Response;
    }
    public static async Task<string> GenerateRawResponse(string query)
    {
        ConversationContextWithResponse context = null;
        var ollama = new OllamaApiClient(new Uri(Environment.GetEnvironmentVariable("ollama_url") ?? throw new InvalidOperationException("ollama_url is not set")));

        Console.WriteLine("Getting models...");
        var models = (await ollama.ListLocalModels()).ToList();
        if (!models.Any())
        {
            throw new InvalidOperationException("No models found\n try to install one like `ollama pull llama2`");
        }
        var modelName = Environment.GetEnvironmentVariable("model_name") ?? models.First().Name;
        ollama.SelectedModel = modelName;
        Console.WriteLine("using model: " + modelName);
        context = await ollama.GetCompletion( query , context);
        return context.Response;
    }
    
    public static async Task<string> GenerateSummerizeResponse(string query)
    {
        ConversationContextWithResponse context = null;
        var ollama = new OllamaApiClient(new Uri(Environment.GetEnvironmentVariable("ollama_url") ?? throw new InvalidOperationException("ollama_url is not set")));
        Console.WriteLine("Getting models...");
        var models = (await ollama.ListLocalModels()).ToList();
        if (!models.Any())
        {
            throw new InvalidOperationException("No models found\n try to install one like `ollama pull llama2`");
        }
        var modelName = Environment.GetEnvironmentVariable("model_name") ?? models.First().Name;
        ollama.SelectedModel = modelName;
        Console.WriteLine("using model: " + modelName);
        context = await ollama.GetCompletion( SUMMERIZE_PREPEX + query , context);
        return context.Response;
    }
}
