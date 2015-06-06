namespace LeagueRecorder.Worker.Recorder
{
    public enum GameRecorderState
    {
        Running,
        Finished,
        Error
    }

    public static class GameRecorderStateExtensions
    {
        public static bool HasEnded(this GameRecorderState state)
        {
            return state != GameRecorderState.Running;
        }
    }
}