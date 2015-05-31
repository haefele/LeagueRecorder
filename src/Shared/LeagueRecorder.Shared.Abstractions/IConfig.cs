namespace LeagueRecorder.Shared.Abstractions
{
    public interface IConfig
    {
        string GameDataContainerName { get; }
        string RecordingTableName { get; }
        string RecordingQueueName { get; }
        int CountOfSummonersToCheckIfIngameAtOnce { get; }
    }
}