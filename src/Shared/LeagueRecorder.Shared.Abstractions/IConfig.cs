namespace LeagueRecorder.Shared.Abstractions
{
    public interface IConfig
    {
        string GameDataContainerName { get; }
        string RecordingTableName { get; }
        int CountOfSummonersToCheckIfIngameAtOnce { get; }
    }
}