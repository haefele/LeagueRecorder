using System.Threading.Tasks;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Replays;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Localization;
using LiteGuard;
using NHibernate;

namespace LeagueRecorder.Shared.Implementations.Replays
{
    public class ReplayStorage : IReplayStorage
    {
        private readonly ISessionFactory _sessionFactory;

        public ReplayStorage([NotNull]ISessionFactory sessionFactory)
        {
            Guard.AgainstNullArgument("sessionFactory", sessionFactory);

            this._sessionFactory = sessionFactory;
        }

        public Task<Result> SaveReplayAsync(Replay replay)
        {
            return Result.CreateAsync(() => Task.Run(() =>
            {
                using (var session = this._sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    var entity = ReplayEntity.FromReplay(replay);
                    session.SaveOrUpdate(entity);

                    transaction.Commit();
                }
            }));
        }
        
        public Task<Result<Replay>> GetReplayAsync(long gameId, Region region)
        {
            return Result.CreateAsync(() => Task.Run(() =>
            {
                using (var session = this._sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    string id = ReplayEntity.ToId(gameId, region);
                    var record = session.Get<ReplayEntity>(id);

                    if (record == null)
                        throw new ResultException(Messages.RecordDoesNotExist);

                    return record.AsReplay();
                }
            }));
        }
    }
}