﻿using Discord.Commands;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.Command_Handler;
using Kaguya.Core.Command_Handler.LogMethods;
using Kaguya.Core.CommandHandler;
using Kaguya.Core.LevelingSystem;
using Kaguya.Core.Server_Files;
using Kaguya.Core.UserAccounts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Kaguya.Core.Embed;
using Victoria;

#pragma warning disable

namespace Kaguya
{
    class CommandHandler
    {
        DiscordShardedClient _client;
        CommandService _commands;
        LavaShardClient _lavaShardClient;
        private IServiceProvider _services;
        readonly KaguyaLogMethods logger = new KaguyaLogMethods();
        readonly MusicLogMethods musicLogger = new MusicLogMethods();
        readonly Stopwatch stopWatch = new Stopwatch();
        readonly Timers timers = new Timers();
        readonly Logger consoleLogger = new Logger();

        public CommandHandler(IServiceProvider services)
        {
            _services = services;
            _client = services.GetRequiredService<DiscordShardedClient>();
            _commands = services.GetRequiredService<CommandService>();

            _client.MessageReceived += HandleCommandAsync;
        }

        public async Task InitializeAsync()
        {
            try
            {
                _client = Global.Client;
                _lavaShardClient = Global.lavaShardClient;

                _commands = new CommandService();
                _commands.AddTypeReader(typeof(List<SocketGuildUser>), new ListSocketGuildUserTR());
                await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            }
            catch (Exception e)
            {
                consoleLogger.ConsoleCriticalAdvisory(e, "InitializeAsync method threw an exception. CommandHandler.cs line 62.");
                return;
            }
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage;
            var user = msg.Author as SocketGuildUser;
            if (msg is null || user is null || user.IsBot) return;

            var guild = Servers.GetServer(user.Guild);
            var userAccount = UserAccounts.GetAccount(user);

            if (userAccount.Blacklisted == 1) { return; }
            if (guild.IsBlacklisted) { return; }


            var context = new ShardedCommandContext(_client, msg);

            foreach (string phrase in guild.FilteredWords)
            {
                if (msg.Content.Contains(phrase))
                {
                    await logger.UserSaysFilteredPhrase(msg);
                    consoleLogger.ConsoleCommandLog(context);
                }
            }

            Leveling.UserSentMessage(user, msg.Channel as SocketTextChannel);

            string oldUsername = userAccount.Username;
            string newUsername = context.User.Username;

            if ($"{oldUsername}" != $"{newUsername}#{user.Discriminator}")
                userAccount.Username = $"{newUsername}#{user.Discriminator}";

            int argPos = 0;

            if (!msg.HasStringPrefix(guild.commandPrefix, ref argPos) && !msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
                return;

            var embed = new KaguyaEmbedBuilder();

            stopWatch.Start();
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            stopWatch.Stop();

            if (result.IsSuccess)
            {
                consoleLogger.ConsoleCommandLog(context, stopWatch.ElapsedMilliseconds);
            }

            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case CommandError.UnknownCommand:
                        break;
                    case CommandError.BadArgCount:
                        var cmdPrefix = Servers.GetServer(context.Guild).commandPrefix;
                        embed.WithDescription("**Error: I need a different set of information than what you've given me!**");
                        embed.WithFooter($"Use {cmdPrefix}h <command> to see the proper usage.");
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.BadArgCount, "User attempted to use invalid parameters for a command.");
                        break;
                    case CommandError.ParseFailed:
                        embed.WithDescription("**Error: I failed to parse a specified value!**");
                        embed.WithFooter($"You may be using text instead of a number. Review {guild.commandPrefix}h <command> for the proper usage!");
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.BadArgCount, "Failed to parse given value specified in command.");
                        break;
                    case CommandError.UnmetPrecondition:
                        embed.WithDescription($"**Error: {result.ErrorReason}**");
                        embed.WithFooter("Review $h <command> for the proper usage!");
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.BadArgCount, $"{result.ErrorReason}");
                        break;
                    case CommandError.MultipleMatches:
                        embed.WithDescription("**Error: I found multiple matches for the task you were trying to execute!**");
                        embed.WithFooter($"Review {guild.commandPrefix}h <command> for the proper usage! I can only do one thing at a time!");
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.BadArgCount, "Multiple matches found.");
                        break;
                    default:
                        embed.WithDescription("**Error: I failed to execute this command for an unknown reason.**");
                        embed.WithFooter($"Error reason: {result.ErrorReason}");
                        await context.Channel.SendMessageAsync(embed: embed.Build());
                        consoleLogger.ConsoleCommandLog(context, CommandError.Unsuccessful, $"{result.ErrorReason}");
                        break;
                }

                string filePath = $"{Directory.GetCurrentDirectory()}/Logs/FailedCommandLogs/KaguyaLogger_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.txt";
                File.AppendAllText(filePath, $"{s.Content} - User: {s.Author} - Time: {DateTime.Now.ToLongTimeString()}\n");
            }

            if (result.IsSuccess)
            {
                string filePath = $"{Directory.GetCurrentDirectory()}/Logs/SuccessfulCommandLogs/KaguyaLogger_{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Year}.txt";
                File.AppendAllText(filePath, $"{s.Content} - User: {s.Author} - Time: {DateTime.Now.ToLongTimeString()}\n");
            }
        }
    }
}
