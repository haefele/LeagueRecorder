using System;
using System.Collections;
using System.Collections.Generic;

namespace LeagueRecorder.Shared.Abstractions.League
{
    public class RiotSpectatorGameInfo
    {
        public long GameId { get; set; }
        public TimeSpan GameLength { get; set; }
        public string Region { get; set; }
        public string EncryptionKey { get; set; }
        public IList<RiotGameParticipant> Participants { get; set; }
    }
}