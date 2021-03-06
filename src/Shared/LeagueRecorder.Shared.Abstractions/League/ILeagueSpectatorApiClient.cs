﻿using System;
using System.Threading.Tasks;
using LeagueRecorder.Shared.Abstractions.Results;

namespace LeagueRecorder.Shared.Abstractions.League
{
    public interface ILeagueSpectatorApiClient
    {
        Task<Result<Version>> GetSpectatorVersion(Region region);
        Task<Result<RiotGameMetaData>> GetGameMetaData(Region region, long gameId);
        Task<Result<RiotLastGameInfo>> GetLastGameInfo(Region region, long gameId);
        Task<Result<RiotChunk>> GetChunk(Region region, long gameId, int chunkId);
        Task<Result<RiotKeyFrame>> GetKeyFrame(Region region, long gameId, int keyFrameId);
    }
}