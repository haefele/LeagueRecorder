namespace LeagueRecorder.Shared.Abstractions
{
    public interface IConfig
    {
        string GameDataContainerName { get; }
        string RecordingTableName { get; }
        string RecordingQueueName { get; }
        int CountOfSummonersToCheckIfIngameAtOnce { get; }
        string RiotApiKey { get; }
        int IntervalToCheckIfOneSummonerIsIngame { get; }
        int IntervalToCheckIfSummonersAreIngame { get; }
        int RecordingMaxErrorCount { get; }
        string Url { get; }
        bool CompressResponses { get; }
    }
}