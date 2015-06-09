using System;
using System.Collections;
using System.Collections.Generic;

namespace LeagueRecorder.Shared.Abstractions.Replays
{
    public class Replay
    {
        public long GameId { get; set; }
        public Region Region { get; set; }

        public Version LeagueVersion { get; set; }
        public Version SpectatorVersion { get; set; }

        public GameInformation GameInformation { get; set; }
        public ReplayInformation ReplayInformation { get; set; }

        public IList<ReplayGameParticipant> Participants { get; set; }
    }
}