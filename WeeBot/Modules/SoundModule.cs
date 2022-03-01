using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses;
using WeeBot.Services;

namespace WeeBot.Modules
{
    public sealed class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly AudioService _audioService;

        public AudioModule(LavaNode lavaNode, AudioService audioService)
        {
            _lavaNode = lavaNode;
            _audioService = audioService;
        }

        [Command("Join")]
            public async Task JoinAsync()
            {
                if (_lavaNode.HasPlayer(Context.Guild))
                {
                    await ReplyAsync("I'm already connected to a voice channel!");
                    return;
                }

                var voiceState = Context.User as IVoiceState;
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }

                try
                {
                    await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                    await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
                }
                catch (Exception exception)
                {
                    await ReplyAsync(exception.Message);
                }
            }

        [Command("Play")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            var voiceState = Context.User as IVoiceState;

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }

                if (string.IsNullOrWhiteSpace(searchQuery))
                {
                    await ReplyAsync("Please provide search terms.");
                    return;
                }

                if (!_lavaNode.HasPlayer(Context.Guild))
                {
                    await ReplyAsync("I'm not connected to a voice channel.");
                    return;
                }

                var searchResponse = await _lavaNode.SearchAsync(Victoria.Responses.Search.SearchType.YouTube, searchQuery);
                //if (searchResponse.SearchStatus == "LoadFailed" ||
                //    searchResponse.SearchStatus == "NoMatches")
                //{
                //    await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                //    return;
                //}

                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        foreach (var track in searchResponse.Tracks)
                        {
                            player.Queue.Enqueue(track);
                        }

                        await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        var track = searchResponse.Tracks.ToList()[0];
                        player.Queue.Enqueue(track);
                        await ReplyAsync($"Enqueued: {track.Title}");
                    }
                }
                else
                {
                    var track = searchResponse.Tracks.ToList()[0];

                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        for (var i = 0; i < searchResponse.Tracks.Count; i++)
                        {
                            if (i == 0)
                            {
                                await player.PlayAsync(track);
                                await ReplyAsync($"Now Playing: {track.Title}");
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks.ToList()[i]);
                            }
                        }

                        await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                    }
                    else
                    {
                        await player.PlayAsync(track);
                        await ReplyAsync($"Now Playing: {track.Title}");
                    }
                }
        }

        public async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            Console.WriteLine("OnTrackEnded event");
            if (args.Reason != TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("Queue completed! Please add more tracks to rock n' roll!");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                $"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }

        [Command("Skip")]
        public async Task SkipAsync()
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            try
            {
                var skippedTracks = player.SkipAsync().Result;
                await ReplyAsync($"Skipped: {skippedTracks.Skipped.Title}");
                await ReplyAsync($"Now playing: {skippedTracks.Current.Title}");
            } catch
            {
                await ReplyAsync("No more tracks in queue.");
            }
        }

        [Command("Stop")]
        public async Task StopAsync()
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            player.StopAsync();
            await ReplyAsync("Stopped playing.");
        }

        [Command("Leave")]
        public async Task LeaveAsync()
        {
            await _lavaNode.LeaveAsync(Context.Guild.CurrentUser.VoiceChannel);
        }

    }
}
