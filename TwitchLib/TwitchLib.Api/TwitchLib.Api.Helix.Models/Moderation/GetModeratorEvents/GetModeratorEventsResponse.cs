﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Api.Helix.Models.Common;

namespace TwitchLib.Api.Helix.Models.Moderation.GetModeratorEvents
{
    public class GetModeratorEventsResponse
    {
        [JsonProperty(PropertyName = "data")]
        public ModeratorEvent[] Data { get; protected set; }
        [JsonProperty(PropertyName = "pagination")]
        public Pagination Pagination { get; protected set; }
    }
}
