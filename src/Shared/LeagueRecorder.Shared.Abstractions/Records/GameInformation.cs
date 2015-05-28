using System;

namespace LeagueRecorder.Shared.Abstractions.Records
{
    public class GameInformation
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int InterestScore { get; set; }
        public TimeSpan GameLength { get; set; }
    }
}