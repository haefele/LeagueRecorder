namespace LeagueRecorder.Shared.Abstractions.Results
{
    public enum ResultState
    {
        /// <summary>
        /// The operation was successfull.
        /// </summary>
        Success,
        /// <summary>
        /// The operation failed and there is a error message.
        /// </summary>
        Error
    }
}