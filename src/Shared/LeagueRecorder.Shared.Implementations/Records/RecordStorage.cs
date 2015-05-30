using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Records;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Implementations.GameData;
using LeagueRecorder.Shared.Implementations.Recordings;
using LeagueRecorder.Shared.Localization;
using LiteGuard;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using NHibernate;

namespace LeagueRecorder.Shared.Implementations.Records
{
    public class RecordStorage : IRecordStorage
    {
        private readonly ISessionFactory _sessionFactory;

        public RecordStorage([NotNull]ISessionFactory sessionFactory)
        {
            Guard.AgainstNullArgument("sessionFactory", sessionFactory);

            this._sessionFactory = sessionFactory;
        }

        public Task<Result> SaveRecordAsync(Record record)
        {
            return Result.CreateAsync(() => Task.Run(() =>
            {
                using (var session = this._sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    var entity = RecordEntity.FromRecord(record);
                    session.SaveOrUpdate(entity);

                    transaction.Commit();
                }
            }));
        }
        
        public Task<Result<Record>> GetRecordAsync(long gameId, Region region)
        {
            return Result.CreateAsync(() => Task.Run(() =>
            {
                using (var session = this._sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    string id = RecordEntity.ToId(gameId, region);
                    var record = session.Get<RecordEntity>(id);

                    if (record == null)
                        throw new ResultException(Messages.RecordDoesNotExist);

                    return record.AsRecord();
                }
            }));
        }
    }
}