﻿using ChatBeet.Services;
using ChatBeet.Utilities;
using GravyBot;
using GravyIrc.Messages;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ChatBeet.Rules
{
    public class TrackRule : MessageRuleBase<PrivateMessage>
    {
        private readonly ChatBeetConfiguration config;
        private readonly LastFmService lastFm;

        public TrackRule(LastFmService lastFm, IOptions<ChatBeetConfiguration> options)
        {
            this.lastFm = lastFm;
            config = options.Value;
        }

        public override async IAsyncEnumerable<IClientMessage> Respond(PrivateMessage incomingMessage)
        {
            var rgx = new Regex($"^{config.CommandPrefix}track (.*) by (.*)");
            var match = rgx.Match(incomingMessage.Message);
            if (match.Success)
            {
                var trackName = match.Groups[1].Value;
                var artistName = match.Groups[2].Value;

                var track = await lastFm.GetTrackInfo(trackName, artistName);

                if (track != null)
                {
                    var result = track.Name;
                    if (track.Duration.HasValue)
                    {
                        result += $" ({track.Duration})";
                    }

                    if (!string.IsNullOrEmpty(track.AlbumName) || !string.IsNullOrEmpty(track.ArtistName))
                    {
                        result += " |";
                    }

                    if (!string.IsNullOrEmpty(track.AlbumName))
                    {
                        result += $" from {track.AlbumName}";
                    }

                    if (!string.IsNullOrEmpty(track.ArtistName))
                    {
                        result += $" by {track.ArtistName}";
                    }

                    result += $" | {track.Url}";
                    yield return new PrivateMessage(incomingMessage.GetResponseTarget(), result);
                }
                else
                {
                    yield return new PrivateMessage(incomingMessage.GetResponseTarget(), "Sorry, couldn't find that track.");
                }
            }
        }
    }
}