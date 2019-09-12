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
using Kaguya.Core.Embed;
using Kaguya.Core.UserAccounts;

namespace Kaguya.Modules.Music
{
    public class MusicService
    {
        readonly LavaShardClient _lavaShardClient = Global.lavaShardClient;
        readonly LavaRestClient _lavaRestClient = Global.lavaRestClient;
        readonly Logger logger = new Logger();

        private readonly Lazy<ConcurrentDictionary<ulong, AudioOptions>> _lazyOptions
            = new Lazy<ConcurrentDictionary<ulong, AudioOptions>>();

        private ConcurrentDictionary<ulong, AudioOptions> Options
            => _lazyOptions.Value;

        static SocketGuildUser summoner { get; set; }

        public async Task<Embed> JoinOrPlayAsync(SocketGuildUser user, ISocketMessageChannel textChannel, ulong guildId, string query = null)
        {
            UserAccount userAccount = new UserAccount(user.Id);

            //Check If User Is Connected To Voice Cahnnel.
            if (user.VoiceChannel == null)
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Join/Play", "You must first join a voice channel!");

            if(Options.TryGetValue(user.Guild.Id, out var options) && options.Summoner.Id != user.Id)
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music, Join/Play", $"I can't join another voice channel untill {options.Summoner} disconnects me.");

            //If The user hasn't provided a Search string from the !Play command, then they must have used the !Join command.
            //Join the voice channel the user is in.

            try
            {
                var player = _lavaShardClient.GetPlayer(guildId);
                if (player == null || player.VoiceChannel == null)
                {
                    await _lavaShardClient.ConnectAsync(user.VoiceChannel);
                    Options.TryAdd(user.Guild.Id, new AudioOptions
                    {
                        Summoner = user
                    });
                    summoner = user; 
                    player = _lavaShardClient.GetPlayer(guildId);
                }

                //Find The YouTube Track the User requested.
                LavaTrack track;
                var search = await _lavaRestClient.SearchYouTubeAsync(query);

                //If we couldn't find anything, tell the user.
                if (search.LoadType == LoadType.NoMatches && query != null)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
                if (search.LoadType == LoadType.LoadFailed && query != null)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music", $"I failed to load {query}.");
                if (search.LoadType == LoadType.PlaylistLoaded && !userAccount.IsSupporter)
                    return await StaticMusicEmbedHandler.CreateMusicEmbed("🎵 Music", 
                        "You must be a supporter to load playlists!", 
                        "More information may be found through the `supporter` command.");
                
                //Get the first track from the search results.
                //TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)
                track = search.Tracks.FirstOrDefault();

                //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
                if (player.CurrentTrack != null && player.IsPlaying || player.IsPaused)
                {
                    player.Queue.Enqueue(track);
                    string thumbnailURL = await track.FetchThumbnailAsync();
                    return await StaticMusicEmbedHandler.CreateMusicEmbed("Music", $"🎵 {track.Title} has been added to queue.", thumbnailURL: thumbnailURL);
                }
                //Player was not playing anything, so lets play the requested track.
                await player.PlayAsync(track);
                if (player.CurrentTrack.Length.TotalSeconds > 600)
                {
                    await player.StopAsync();
                    if (player.Queue.Count > 0)
                        await player.SkipAsync();
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music", $"This song is longer than 10 minutes, therefore it cannot be played!");
                }
                return await StaticMusicEmbedHandler.CreateMusicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Uri}", thumbnailURL: await track.FetchThumbnailAsync());
            }
            //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
            catch (Exception e)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music, Join/Play", e.Message);
            }
        }

        public static async Task<Embed> TrackCompletedAsync(LavaPlayer player, LavaTrack track, TrackEndReason reason)
        {
            if (reason.Equals(TrackEndReason.LoadFailed))
            {
                await player.VoiceChannel.DisconnectAsync();
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Continuation", "I have failed to continue the queue! If you believe this is an error, " +
                    $"please contact `Stage#0001` in my support server! Use `{Servers.GetServer(player.TextChannel.Guild as SocketGuild).commandPrefix}hdm` for an invite!");
            }

            if (player.Queue.Count < 1 && !player.IsPlaying)
            {
                return await StaticMusicEmbedHandler.CreateMusicEmbed("🎵 Music", "There are no more items left in the queue, so I have stopped playing! ");
            }

            if (player.Queue.TryDequeue(out var item) && item is LavaTrack nextTrack)
            {
                if (player.VoiceChannel == null)
                    await Global.lavaShardClient.ConnectAsync(summoner.VoiceChannel);
                await player.PlayAsync(nextTrack);
                return await StaticMusicEmbedHandler.CreateMusicEmbed("🎵 Music", 
                    $"Finished playing: {track.Title}\nNow playing: {nextTrack.Title}", thumbnailURL: await nextTrack.FetchThumbnailAsync());
            }

            return await StaticMusicEmbedHandler.CreateErrorEmbed("Music", "I failed to finish playing the requested track for an unknown reason. " +
                "Please contact my creator (Stage) in the support server (this can be found through the `invite` command).");
        }

        public async Task<Embed> LeaveAsync(SocketGuildUser user, ulong guildId)
        {
            try
            {
                //Get The Player Via GuildID.
                var player = _lavaShardClient.GetPlayer(guildId);

                //if The Player is playing, Stop it.
                if (player.IsPlaying)
                    await player.StopAsync();

                //Leave the voice channel.
                var channelName = player.VoiceChannel.Name;
                await _lavaShardClient.DisconnectAsync(user.VoiceChannel);
                return await StaticMusicEmbedHandler.CreateBasicEmbed("Music", $"Disconnected from {channelName}.");
            }
            //Tell the user about the error so they can report it back to us.
            catch (InvalidOperationException e)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Leaving Music Channel", e.Message);
            }
        }

        public async Task<Embed> JoinAsync(SocketGuildUser user, ulong guildId)
        {
            var player = _lavaShardClient.GetPlayer(guildId);
            await _lavaShardClient.ConnectAsync(user.VoiceChannel);
            return await StaticMusicEmbedHandler.CreateMusicEmbed("Music", $"Connected to `{user.VoiceChannel.Name}`");
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
                var player = _lavaShardClient.GetPlayer(guildId);
                if (player == null)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("🎵 Music Queue", 
                        $"Could not aquire music player.\nAre you using the music service right now? See `{Servers.GetServer(guildId).commandPrefix}h m` for proper usage.");

                if (player.IsPlaying)
                {
                    /*If the queue count is less than 1 and the current track IS NOT null then we won't have a list to reply with.
                        In this situation we simply return an embed that displays the current track instead. */
                    if (player.Queue.Count < 1 && player.CurrentTrack != null)
                    {
                        return await StaticMusicEmbedHandler.CreateBasicEmbed($"🎵 Now Playing: {player.CurrentTrack.Title}", 
                            "There are no other items in the queue.");
                    }
                    else
                    {
                        /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
                         *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                            This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                        var trackNum = 2;
                        foreach (LavaTrack track in player.Queue.Items)
                        {
                            if (trackNum == 2) { descriptionBuilder.Append($"`#{trackNum}`: [{track.Title}]({track.Uri})\n"); trackNum++; }
                            else { descriptionBuilder.Append($"`#{trackNum}`: [{track.Title}]({track.Uri})\n"); trackNum++; }
                        }
                        return await StaticMusicEmbedHandler.CreateBasicEmbed("🎵 Music Queue", 
                            $"`Now Playing`: [{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})\n{descriptionBuilder.ToString()}");
                    }
                }
                else
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("🎵 Music Queue", 
                        "Player doesn't seem to be playing anything right now. If this is an error, Please contact Stage in the Kaguya support server.");
                }
            }
            catch (Exception ex)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
            }

        }

        public async Task<Embed> SkipTrackAsync(ulong guildId, string serverName)
        {
            var cmdPrefix = Servers.GetServer(guildId).commandPrefix;

            try
            {
                var player = _lavaShardClient.GetPlayer(guildId);
                /* Check if the player exists */
                if (player == null)
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("⏩ Music Skip", 
                        $"Could not aquire player.\nAre you using the bot right now? Check `{cmdPrefix}h m` for information on Kaguya's Music Service.");
                if (player.Queue.Count == 0 && player.IsPlaying == true)
                {
                    await player.StopAsync();
                    return await StaticMusicEmbedHandler.CreateMusicEmbed("⏩ Music Skip", "This is the last song in the queue, so I have stopped playing.");
                }

                else if (player.Queue.Count == 0 && player.IsPlaying == false)
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("⏩ Music Skip", "There are no songs to skip!");
                }

                else
                {
                    try
                    {
                        // Save the current song for use after we skip it. 
                        var lastTrack = player.CurrentTrack.Title;
                        // Skip the current song.
                        await player.SkipAsync();
                        logger.ConsoleMusicLogNoUser($"Music Player Skipped: {lastTrack}");
                        return await StaticMusicEmbedHandler.CreateBasicEmbed("⏩ Music Skip", $"Successfully skipped {lastTrack}");
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

        public async Task<Embed> VolumeAsync(ulong guildId, string result)
        {
            if(int.TryParse(result, out int volume))
            {
                if (volume > 200)
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed($"🔊 Music Volume", $"Volume must not be above 200.");
                }
                try
                {
                    if (volume < 0) //The volume is negative
                    {
                        var lavaPlayer = _lavaShardClient.GetPlayer(guildId);
                        var curVolume = lavaPlayer.CurrentVolume;

                        if(curVolume + volume < 0) //We add because volume is negative.
                        {
                            /*If the volume is less than zero, let's set it to zero
                            because the user may want to quickly mute the track.*/

                            return await StaticMusicEmbedHandler.CreateMusicEmbed($"🔊 Music Volume",
                                $"Player volume set to 0.");
                        }

                        await lavaPlayer.SetVolumeAsync(curVolume + volume);

                        //We don't add a negative sign below because the number does it for us.

                        logger.ConsoleMusicLogNoUser($"Player Volume adjusted by: {volume}.");
                        return await StaticMusicEmbedHandler.CreateBasicEmbed($"🔊 Music Volume", $"Volume has been adjusted by {volume}.",
                            $"New Volume: {lavaPlayer.CurrentVolume}");
                    }
                    else if (result.Contains('+'))
                    {
                        string newValue = result.Split('+').Last();
                        if (int.TryParse(newValue, out int newVolume))
                        {
                            var newPlayer = _lavaShardClient.GetPlayer(guildId);
                            int curVolume = newPlayer.CurrentVolume;

                            if (curVolume + newVolume > 200)
                            {
                                return await StaticMusicEmbedHandler.CreateErrorEmbed("Invalid Volume Adjustment",
                                    $"The requested adjustment would put the total volume above 200. Volume adjustments " +
                                    $"may not send the player's volume above 200.");
                            }

                            await newPlayer.SetVolumeAsync(curVolume + newVolume);
                            logger.ConsoleMusicLogNoUser($"Bot Volume adjusted by: +{newVolume}.");
                            return await StaticMusicEmbedHandler.CreateBasicEmbed($"🔊 Music Volume",
                                $"Volume has been adjusted by +{newVolume}.", $"New Volume: {newPlayer.CurrentVolume}");

                        }
                        else
                        {
                            return await StaticMusicEmbedHandler.CreateErrorEmbed($"Invalid Volume",
                            "The value specified is invalid.");
                        }
                    }
                    else
                    {
                        var player = _lavaShardClient.GetPlayer(guildId);
                        await player.SetVolumeAsync(volume);
                        logger.ConsoleMusicLogNoUser($"Bot Volume set to: {volume}.");
                        return await StaticMusicEmbedHandler.CreateBasicEmbed($"🔊 Music Volume", $"Volume has been set to {volume}.");
                    }
                }
                catch (InvalidOperationException ex)
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Volume",
                        $"{ex.Message}", "Please contact Stage in the support server if this is a recurring issue.");
                }
            }
            else
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Volume",
                    "The requested volume is invalid.");
            }
        }

        public async Task<Embed> Pause(ulong guildID)
        {
            try
            {
                var player = _lavaShardClient.GetPlayer(guildID);
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

        public async Task<Embed> Jump(ulong guildID, int jumpNum)
        {
            jumpNum -= 1;

            try
            {
                var player = _lavaShardClient.GetPlayer(guildID);
                if(player.Queue.Count < 1)
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Queue Jump", $"There aren't any available songs to jump to!");
                }

                if(jumpNum > player.Queue.Count)
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Queue Jump", $"You are attempting to jump to a spot in the queue that doesn't exist!" +
                        $"\nSee what is available to jump to with the `queue` command.");
                }

                else
                {
                    for(int i = 0; i < jumpNum; i++)
                    {
                       await player.SkipAsync();
                    }
                    return await StaticMusicEmbedHandler.CreateMusicEmbed("⏭️ Music Jump", $"Successfully jumped to position `{jumpNum + 1}` in the queue!");
                }
            }
            catch (Exception e)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Queue Jump", e.Message);
            }
        }

        public async Task<Embed> Lyrics(ulong guildID) //Experimental
        {
            try
            {
                var player = _lavaShardClient.GetPlayer(guildID);
                var track = player.CurrentTrack;

                if (player == null)
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Lyrics",
                        "There is no currently active player.", "Use the `m leave` command while in a voice channel. " +
                        "If the issue persists, please join my support server and ask for help.");
                }

                string lyrics = await track.FetchLyricsAsync();

                if(lyrics == null)
                {
                    return await StaticMusicEmbedHandler.CreateErrorEmbed("Music Lyrics <a:crabPls:588362913379516442>",
                        $"No lyrics are available for \n{track.Author} - {track.Title}");
                }

                return await StaticMusicEmbedHandler.CreateMusicEmbed($"Music Lyrics <a:Banger:588362912905822208>",
                    $"Here are the lyrics to {track.Title}: {lyrics}");
            }
            catch(Exception e)
            {
                return await StaticMusicEmbedHandler.CreateErrorEmbed("Exception", $"{e.Message}",
                    "If this is unexpected, please join my support server and ask for help.");
            }
        }
    }
}
