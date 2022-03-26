﻿using System;
using TwitchLib.EventSub.Webhooks.Core.Models.Predictions;

namespace TwitchLib.EventSub.Webhooks.Core.SubscriptionTypes.Channel
{
    /// <summary>
    /// Channel Prediction Lock subscription type model
    /// <para>Description:</para>
    /// <para>A Prediction was locked on a specified channel.</para>
    /// </summary>
    public class ChannelPredictionLock : ChannelPredictionBase
    {
        /// <summary>
        /// The time the Channel Points Prediction was locked.
        /// </summary>
        public DateTime LockedAt { get; set; } = DateTime.MinValue;
    }
}