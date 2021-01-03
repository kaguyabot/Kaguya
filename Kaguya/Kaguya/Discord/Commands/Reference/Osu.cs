using Discord.Commands;
using Kaguya.Discord.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Discord;
using Kaguya.Database.Model;
using Kaguya.Database.Repositories;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.External.Osu;
using OsuSharp;

namespace Kaguya.Discord.Commands.Reference
{
    [Module(CommandModule.Reference)]
    [Group("osu")]
    [Alias("o")]
    public class Osu : KaguyaBase<Osu>
    {
        private readonly ILogger<Osu> _logger;
        private readonly OsuClient _osuClient;
        private readonly KaguyaUserRepository _kaguyaUserRepository;
        private readonly KaguyaServerRepository _kaguyaServerRepository;
        private const string GAME_MODE_STRING = "**Valid Game Modes:** `std`, `mania`, `taiko`, `ctb`";

        public Osu(ILogger<Osu> logger, OsuClient osuClient, KaguyaUserRepository kaguyaUserRepository, 
            KaguyaServerRepository kaguyaServerRepository) : base(logger)
        {
            _logger = logger;
            _osuClient = osuClient;
            _kaguyaUserRepository = kaguyaUserRepository;
            _kaguyaServerRepository = kaguyaServerRepository;
        }

        [Priority(1)]
        [Command("-set")]
        [Summary("Give me an osu! username to remember you by! After setting a username, you will no longer have " +
                 "to specify a username when using osu commands. You must also configure your game mode.\n\n" + GAME_MODE_STRING)]
        [Remarks("<game mode> <username or id>")]
        [Example("std name with spaces")]
        [Example("mania 99999999")]
        [Example("ctb really good ctb player")]
        public async Task OsuSetCommand(string gameModeString, [Remainder] string username)
        {
            var data = new OsuData(username, gameModeString, _osuClient);
            GameMode gameMode = data.GameMode;
            
            User osuUser = await data.GetOsuUserAsync();
            if (osuUser == null)
            {
                await SendBasicErrorEmbedAsync(new OsuUserNotFoundException(username).Message);

                return;
            }
            
            KaguyaUser kaguyaUser = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
            kaguyaUser.OsuId = osuUser.UserId;
            kaguyaUser.OsuGameMode = gameMode;

            await _kaguyaUserRepository.UpdateAsync(kaguyaUser);
            
            await SendBasicEmbedAsync("Successfully set your osu! username: " + osuUser.Username.AsBold(), Color.Blue);
        }

        [Priority(2)]
        [Command("-recent", RunMode = RunMode.Async)]
        [Alias("-r")]
        [Summary("Displays the most recent play for a given username. The `gamemode` parameter is only " +
                 "optional if your username has been set via the `osu -set` command.\n\n" + GAME_MODE_STRING)]
        [Remarks("\n<gamemode> [username]")]
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public async Task OsuRecentCommand(string gameMode = null, [Remainder] string username = null)
        {
            KaguyaUser kaguyaUser = await _kaguyaUserRepository.GetOrCreateAsync(Context.User.Id);
            KaguyaServer server = await _kaguyaServerRepository.GetOrCreateAsync(Context.Guild.Id);

            OsuData data;
            
            if (gameMode == null && username == null)
            {
                if (!kaguyaUser.OsuId.HasValue || !kaguyaUser.OsuGameMode.HasValue)
                {
                    await SendBasicErrorEmbedAsync($"To use this command without parameters, you need to set your osu! username and game mode through the " +
                                                   $"{server.CommandPrefix}osu -set <game mode> <username>".AsCodeBlockSingleLine() + " command.");

                    return;
                }

                data = new OsuData(kaguyaUser.OsuId.Value, kaguyaUser.OsuGameMode.Value, _osuClient);
            }
            else if (gameMode != null && username == null)
            {
                if (!kaguyaUser.OsuId.HasValue)
                {
                    await SendBasicErrorEmbedAsync("You specified a gamemode but not a username. To specify a gamemode without a username, " +
                                                   "you need to set your osu! username and game mode through the " +
                                                   $"{server.CommandPrefix}osu -set <game mode> <username>".AsCodeBlockSingleLine() +
                                                   " command.");

                    return;
                }
                
                data = new OsuData(kaguyaUser.OsuId.Value, gameMode, _osuClient);
            }
            else
            {
                data = new OsuData(username, gameMode, _osuClient);
            }
            
            User osuUser = await data.GetOsuUserAsync();
            if (osuUser == null)
            {
                await SendBasicErrorEmbedAsync($"I couldn't find anyone with the username {data.OsuUsername.AsBold()}.");

                return;
            }

            var recent = new OsuRecent(data, Context);
            await SendEmbedAsync(await recent.GetMostRecentForUserAsync());
        }
    }
}