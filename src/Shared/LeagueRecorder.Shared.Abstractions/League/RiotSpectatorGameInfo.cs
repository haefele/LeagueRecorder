﻿using System;

namespace LeagueRecorder.Shared.Abstractions.League
{
    public class RiotSpectatorGameInfo
    {
        public long GameId { get; set; }
        public TimeSpan GameLength { get; set; }
        public string Region { get; set; }
        public string EncryptionKey { get; set; }
    }
}