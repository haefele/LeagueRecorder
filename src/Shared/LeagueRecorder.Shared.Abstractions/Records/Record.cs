using System;

namespace LeagueRecorder.Shared.Abstractions.Records
{
    public class Record
    {
        public long GameId { get; set; }
        public Region Region { get; set; }

        public Version LeagueVersion { get; set; }
        public Version SpectatorVersion { get; set; }

        public GameInformation GameInformation { get; set; }
        public ReplayInformation ReplayInformation { get; set; }
    }
}