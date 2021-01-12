using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Kaguya.Discord.DiscordExtensions;
using Kaguya.Internal.Attributes;
using Kaguya.Internal.Enums;
using Microsoft.Extensions.Logging;
using Victoria;

namespace Kaguya.Discord.Commands.Music
{
    [Module(CommandModule.Music)]
    [Group("join")]
    [RequireUserPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Connect)]
    [RequireBotPermission(GuildPermission.Speak)]
    public class Join : KaguyaBase<Join>
    {
        private readonly ILogger<Join> _logger;
        private readonly LavaNode _lavaNode;
        
        public Join(ILogger<Join> logger, LavaNode lavaNode) : base(logger)
        {
            _logger = logger;
            _lavaNode = lavaNode;
        }

        [Command]
        [Summary("Tells me to join the voice channel you are currently in. If I am already in " +
                 "another voice channel, I won't leave it to join yours.")]
        public async Task JoinCommand()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await SendBasicErrorEmbedAsync("I'm already connected to a voice channel.");

                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await SendBasicErrorEmbedAsync("You must be connected to a voice channel to use this command.");

                return;
            }
            
            try 
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await SendBasicSuccessEmbedAsync($"Joined {voiceState.VoiceChannel.Name.AsBold()}!");
            }
            catch (Exception exception) 
            {
                await SendBasicErrorEmbedAsync($"Failed to join {voiceState.VoiceChannel.Name}.\n\nError:\n" + exception.Message);
            }
        }
    }
}