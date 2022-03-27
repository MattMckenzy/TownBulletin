﻿using System.Collections.Generic;
using System.Linq;

using TwitchLib.Client.Models.Extractors;

namespace TwitchLib.Client.Models
{
    /// <summary>Object representing emote set from a chat message.</summary>
    public class EmoteSet
    {
        /// <summary>List containing all emotes in the message.</summary>
        public List<Emote> Emotes { get; set; }

        /// <summary>The raw emote set string obtained from Twitch, for legacy purposes.</summary>
        public string RawEmoteSetString { get; set; }

        /// <summary>Constructor for ChatEmoteSet object.</summary>
        /// <param name="rawEmoteSetString"></param>
        /// <param name="message"></param>
        public EmoteSet(string rawEmoteSetString, string message)
        {
            // this should be removed and used outside of object
            RawEmoteSetString = rawEmoteSetString;
            EmoteExtractor emoteExtractor = new EmoteExtractor();
            Emotes = emoteExtractor.Extract(rawEmoteSetString, message).ToList();
        }

        /// <summary>Constructor for ChatEmoteSet object.</summary>
        /// <param name="emotes">Collection of Emote instances</param>
        /// <param name="rawEmoteSetString">Original string from which emotes were created</param>
        public EmoteSet(IEnumerable<Emote> emotes, string emoteSetData)
        {
            RawEmoteSetString = emoteSetData;
            Emotes = emotes.ToList();
        }

        public EmoteSet()
        {
        }
    }
}
