using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kaguya
{
    public class Program
    {
        static void Main(string[] args)
            => new Program().StartAsync().GetAwaiter().GetResult();

        DiscordSocketClient _client;
        CommandHandler _handler;
        public string version = Utilities.GetAlert("VERSION");

        public async Task StartAsync()
        {
            try
            {
                string name = Environment.UserName; // Greets user in console
                string message = Utilities.GetFormattedAlert("WELCOME_&NAME_&VERSION", name, version);
                Console.WriteLine(message);
                if (Config.bot.Token == "" || Config.bot.Token == null && Config.bot.CmdPrefix == "" || Config.bot.CmdPrefix == null) //default values in config.json when first launched, first time setup essentially.
                {
                    Console.WriteLine("Bot token not found. Get your bot's token from the Discord Developer portal and paste it here: ");
                    string token = Console.ReadLine();
                    Console.Write("Command prefix not found. What would you like it to be?" +
                        "\n(This is typically one symbol, such as \"!, #, $, %, etc.\": ");
                    string prefix = Console.ReadLine();
                    BotConfig bot = new BotConfig
                    {
                        Token = token,
                        CmdPrefix = prefix
                    };
                    string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                    File.WriteAllText("Resources" + "/" + "config.json", json);
                    Console.WriteLine("Confirmed. Restarting in 5 seconds...");
                    var filePath = Assembly.GetExecutingAssembly().Location;
                    await Task.Delay(5000);
                    Process.Start(filePath);
                    Environment.Exit(0);
                }
                try
                {
                    _client = new DiscordSocketClient(new DiscordSocketConfig
                    {
                        MessageCacheSize = 100
                    });
                    try
                    {
                        await _client.LoginAsync(TokenType.Bot, Config.bot.Token);
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        Console.WriteLine("Error: Could not successfully connect. Do you have a stable internet connection?");
                        await Task.Delay(10000);
                        Console.WriteLine("Exiting...");
                        await Task.Delay(2000);
                        Environment.Exit(0);
                        return;
                    }
                    await _client.StartAsync();
                    Global.Client = _client;

                    var serviceProvider = new ServiceCollection()
                        .AddSingleton(_client)
                        .AddSingleton(new InteractiveService(_client))
                        .AddSingleton(new CommandService(
                        new CommandServiceConfig { CaseSensitiveCommands = false, ThrowOnError = false })).BuildServiceProvider();

                    _handler = new CommandHandler(serviceProvider);
                    await _handler.InitializeAsync();

                    await Task.Delay(-1);
                }
                catch (Discord.Net.HttpException)
                {
                    Console.WriteLine("You have an invalid bot token. Edit /Resources/config.json and supply the proper token.");
                    Console.WriteLine("Press any key to exit...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            }
            catch (ReflectionTypeLoadException ex) //Finds missing packages and tells us what they are in the console
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                Console.WriteLine(errorMessage);
            }
        }
    }
}
