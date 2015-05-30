using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LeagueRecorder.Shared.Abstractions;
using LeagueRecorder.Shared.Abstractions.Results;
using LeagueRecorder.Shared.Abstractions.Summoners;
using NHibernate;
using NHibernate.Linq;

namespace LeagueRecorder.Shared.Implementations.Summoners
{
    public class SummonerStorage : ISummonerStorage
    {
        private readonly ISessionFactory _sessionFactory;
        private readonly IConfig _config;

        public SummonerStorage([NotNull]ISessionFactory sessionFactory, [NotNull]IConfig config)
        {
            this._sessionFactory = sessionFactory;
            this._config = config;
        }

        public Task<Result> SaveSummonerAsync(Summoner summoner)
        {
            return Result.CreateAsync(() => Task.Run(() =>
            {
                using (var session = this._sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    var entity = SummonerEntity.FromSummoner(summoner);
                    session.SaveOrUpdate(entity);

                    transaction.Commit();
                }
            }));
        }

        public Task<Result<IList<Summoner>>> GetSummonersForInGameCheckAsync(Region[] availableRegions)
        {
            return Result.CreateAsync(() => Task.Run(() =>
            {
                using (var session = this._sessionFactory.OpenSession())
                using (var transaction = session.BeginTransaction())
                {
                    var regionsAsString = availableRegions.Select(f => f.ToString()).ToArray();

                    IList<Summoner> summoners = session.Query<SummonerEntity>()
                        .Where(f => regionsAsString.Contains(f.Region) && f.NextDateToCheckIfSummonerIsIngame <= DateTimeOffset.UtcNow)
                        .OrderBy(f => f.NextDateToCheckIfSummonerIsIngame)
                        .Take(this._config.CountOfSummonersToCheckIfIngameAtOnce)
                        .ToList()
                        .Select(f => f.AsSummoner())
                        .ToList();

                    return summoners;
                }
            }));
        }
    }
}