using Discord;
using Discord.WebSocket;
using Kaguya.Core;
using Kaguya.Core.CommandHandler;
using Kaguya.Core.Server_Files;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;

namespace Kaguya.Modules.Music
{
    public class MusicService
    {
        readonly LavaSocketClient _lavaSocketClient = Global.lavaSocketClient;
        readonly LavaRestClient _lavaRestClient = Global.lavaRestClient;
        readonly Logger logger = new Logger();

        private readonly Lazy<ConcurrentDictionary<ulong, AudioOptions>> _lazyOptions
            = new Lazy<ConcurrentDictionary<ulong, AudioOptions>>();

        private ConcurrentDictionary<ulong, AudioOptions> Options
            => _lazyOptions.Value;

        public async Task<Embed> JoinOrPlayAsync(SocketGuildUser user, IMessageChannel textChannel, ulong guildId, string query = null)
        {
            //Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Join/Play", "You must first join a voice channel!");

            if(Options.TryGetValue(user.Guild.Id, out var options) && options.Summoner.Id != user.Id)
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music, Join/Play", $"I can't join another voice channel untill {options.Summoner} disconnects me.");

            //If The user hasn't provided a Search string from the !Play command, then they must have used the !Join command.
            //Join the voice channel the user is in.

            try
            {
                //Try get the player. If it returns null then the user has used the command !Play without using the command !Join.
                var player = _lavaSocketClient.GetPlayer(guildId);
                if (player == null)
                {
                    //User Used Command !Play before they used !Join
                    //So We Create a Connection To The Users Voice Channel.
                    await _lavaSocketClient.ConnectAsync(user.VoiceChannel);
                    Options.TryAdd(user.Guild.Id, new AudioOptions
                    {
                        Summoner = user
                    });
                    //Now we can set the player to out newly created player.
                    player = _lavaSocketClient.GetPlayer(guildId);
                }

                //Find The YouTube Track the User requested.
                LavaTrack track;
                var search = await _lavaRestClient.SearchYouTubeAsync(query);

                //If we couldn't find anything, tell the user.
                if (search.LoadType == LoadType.NoMatches && query != null)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
                if (search.LoadType == LoadType.LoadFailed && query != null)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music", $"I failed to load {query}.");
                
                //Get the first track from the search results.
                //TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)
                track = search.Tracks.FirstOrDefault();

                //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
                if (player.CurrentTrack != null && player.IsPlaying || player.IsPaused)
                {
                    player.Queue.Enqueue(track);
                    logger.ConsoleMusicLog($"{track.Title} has been added to the music queue.");
                    return await StaticMusicEmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.");
                }
                //Player was not playing anything, so lets play the requested track.
                await player.PlayAsync(track);
                logger.ConsoleMusicLog($"🎵 Now Playing: {track.Title}\nUrl: {track.Uri}");
                return await StaticMusicEmbedHandler.CreateMusicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Uri}");
            }
            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception e)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music, Join/Play", e.Message);
            }
        }

        public async Task<Embed> LeaveAsync(SocketGuildUser user, ulong guildId)
        {
            try
            {
                //Get The Player Via GuildID.
                var player = _lavaSocketClient.GetPlayer(guildId);

                //if The Player is playing, Stop it.
                if (player.IsPlaying)
                    await player.StopAsync();

                //Leave the voice channel.
                var channelName = player.VoiceChannel.Name;
                await _lavaSocketClient.DisconnectAsync(user.VoiceChannel);
                logger.ConsoleMusicLog($"Kaguya has disconnected from {channelName}.");
                return await StaticMusicEmbedHandler.CreateBasicEmbed("Music", $"Disconnected from {channelName}.");
            }
            //Tell the user about the error so they can report it back to us.
            catch (InvalidOperationException e)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Leaving Music Channel", e.Message);
            }
        }

        /// <summary>
        /// Kaguya's music queue.
        /// </summary>
        /// <param name="guildId">ID of the guild this command was executed in.</param>
        /// <returns></returns>
        public async Task<Embed> ListAsync(ulong guildId) 
        {
            try
            {
                /* Create a string builder we can use to format how we want our list to be displayed. */
                var descriptionBuilder = new StringBuilder();

                /* Get The Player and make sure it isn't null. */
                var player = _lavaSocketClient.GetPlayer(guildId);
                if (player == null)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Queue", $"Could not aquire music player.\nAre you using the music service right now? See `{Servers.GetServer(guildId).commandPrefix}h m` for proper usage.");

                if (player.IsPlaying)
                {
                    /*If the queue count is less than 1 and the current track IS NOT null then we won't have a list to reply with.
                        In this situation we simply return an embed that displays the current track instead. */
                    if (player.Queue.Count < 1 && player.CurrentTrack != null)
                    {
                        return await StaticMusicEmbedHandler.CreateBasicEmbed($"Now Playing: {player.CurrentTrack.Title}", "There are no other items in the queue.");
                    }
                    else
                    {
                        /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
                         *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                            This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                        var trackNum = 2;
                        foreach (LavaTrack track in player.Queue.Items)
                        {
                            if (trackNum == 2) { descriptionBuilder.Append($"Up Next: [{track.Title}]({track.Uri})\n"); trackNum++; }
                            else { descriptionBuilder.Append($"#{trackNum}: [{track.Title}]({track.Uri})\n"); trackNum++; }
                        }
                        return await StaticMusicEmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})\n{descriptionBuilder.ToString()}");
                    }
                }
                else
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Queue", "Player doesn't seem to be playing anything right now. If this is an error, Please contact Stage in the Kaguya support server.");
                }
            }
            catch (Exception ex)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }

        }

        public async Task<Embed> SkipTrackAsync(ulong guildId)
        {
            var cmdPrefix = Servers.GetServer(guildId).commandPrefix;

            try
            {
                var player = _lavaSocketClient.GetPlayer(guildId);
                /* Check if the player exists */
                if (player == null)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check{cmdPrefix}Help for info on how to use the bot.");
                /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
                     User is expected to use the Stop command if they're only wanting to skip the current song. */
                if (player.Queue.Count == 1)
                    return await StaticMusicEmbedHandler.CreateMusicEmbed("Music Skipping", "This is the last song in the queue, so I have stopped playing."); await player.StopAsync();
                if (player.Queue.Count == 0)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Skipping", "There are no songs to skip!");
                else
                {
                    try
                    {
                        /* Save the current song for use after we skip it. */
                        var currentTrack = player.CurrentTrack;
                        /* Skip the current song. */
                        await player.SkipAsync();
                        logger.ConsoleMusicLog($"Bot skipped: {currentTrack.Title}");
                        return await StaticMusicEmbedHandler.CreateBasicEmbed("Music Skip", $"Successfully skipped {currentTrack.Title}");
                    }
                    catch (Exception ex)
                    {
                        return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Skipping Exception:", ex.ToString());
                    }

                }
            }
            catch (Exception ex)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Skip", ex.ToString());
            }
        }

        public async Task<Embed> VolumeAsync(ulong guildId, int volume)
        {
            if (volume >= 150 || volume <= 0)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed($"Music Volume", $"Volume must be between 1 and 149.");
            }
            try
            {
                var player = _lavaSocketClient.GetPlayer(guildId);
                await player.SetVolumeAsync(volume);
                logger.ConsoleMusicLog($"Bot Volume set to: {volume}.");
                return await StaticMusicEmbedHandler.CreateBasicEmbed($"🔊 Music Volume", $"Volume has been set to {volume}.");
            }
            catch (InvalidOperationException ex)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Volume", $"{ex.Message}", "Please contact Stage in the support server if this is a recurring issue.");
            }
        }

        public async Task<Embed> Pause(ulong guildId)
        {
            try
            {
                var player = _lavaSocketClient.GetPlayer(guildId);
                if (player.IsPaused)
                {
                    await player.ResumeAsync();
                    return await StaticMusicEmbedHandler.CreateMusicEmbed("▶️ Music", $"**Resumed:** Now Playing {player.CurrentTrack.Title}");
                }
                else
                {
                    await player.PauseAsync();
                    return await StaticMusicEmbedHandler.CreateMusicEmbed("⏸️ Music", $"**Paused:** {player.CurrentTrack.Title}");
                }
            }
            catch (InvalidOperationException e)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Play/Pause", e.Message);
            }
        }
    }
}
