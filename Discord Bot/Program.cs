using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord_Bot.Core;
using System.IO;
using Newtonsoft.Json;

#pragma warning disable CS1998

namespace Discord_Bot
{
    class Program
    {
        DiscordSocketClient _client;
        CommandHandler _handler;
        public string version = Utilities.GetAlert("VERSION");
        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            string name = Environment.UserName; // Greets user in console
            string message = Utilities.GetFormattedAlert("WELCOME_&NAME_&VERSION", name, version);
            Console.WriteLine(message);
            EditableCommands.JsonInit();
            if (Config.bot.token == "" || Config.bot.token == null && Config.bot.cmdPrefix == "" || Config.bot.cmdPrefix == null) //default values in config.json when first launched, first time setup essentially.
            {
                Console.WriteLine("Bot token not found. Get your bot's token from the Discord Developer portal and paste it here: ");
                string token = Console.ReadLine();
                Console.Write("Command prefix not found. What would you like it to be?" +
                    "\n(This is typically one symbol, such as \"!, #, $, %, etc.\": ");
                string prefix = Console.ReadLine();
                BotConfig bot = new BotConfig();
                bot.token = token;
                bot.cmdPrefix = prefix;
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText("Resources" + "/" + "config.json", json);
                Console.WriteLine("Confirmed. Restarting.");
                Environment.Exit(0);
                System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);
            }
            try
            {
                _client = new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose
                });
                _client.Log += Log;
                _client.Ready += RepeatingTimer.StartTimer;
                await _client.LoginAsync(TokenType.Bot, Config.bot.token);
                await _client.StartAsync();
                Global.Client = _client;
                _handler = new CommandHandler();
                await _handler.InitializeAsync(_client);
                await Task.Delay(-1);
            }
            catch(Discord.Net.HttpException)
            {
                Console.WriteLine("You have an invalid bot token. Edit /Resources/config.json and supply the proper token.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
        }
    }
}
