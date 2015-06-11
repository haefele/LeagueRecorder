using System.Linq;

namespace LeagueRecorder.Shared.Abstractions
{
    public abstract class Team
    {
        #region Values
        public static Team Blue = new BlueTeam();
        public static Team Red = new RedTeam();

        public static Team[] All = new[]
        {
            Blue,
            Red
        };
        #endregion

        #region Constructors
        /// <summary>
        /// Prevents a default instance of the <see cref="Team"/> class from being created.
        /// Only nested classes are valid.
        /// </summary>
        private Team()
        {
            
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Gets the team identifier.
        /// </summary>
        public abstract long TeamId { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return this.Name;
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Returns the team for the specified <paramref name="teamId"/>.
        /// </summary>
        /// <param name="teamId">The team identifier.</param>
        public static Team FromTeamId(long teamId)
        {
            return All.FirstOrDefault(f => f.TeamId == teamId);
        }
        #endregion

        #region Internal
        private class BlueTeam : Team
        {
            public override string Name
            {
                get { return "Blue"; }
            }
            public override long TeamId
            {
                get { return 100; }
            }
        }
        private class RedTeam : Team
        {
            public override string Name
            {
                get { return "Red"; }
            }
            public override long TeamId
            {
                get { return 200; }
            }
        }
        #endregion
    }
}