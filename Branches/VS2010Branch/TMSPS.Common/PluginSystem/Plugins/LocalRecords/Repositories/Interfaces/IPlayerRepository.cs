using System;
using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces
{
    public interface IPlayerRepository : IRepositoryBase
    {
        PlayerEntity EnsurePlayerExistsAndUptodate(string login, string nickname);
        uint IncreaseWins(string login);
        void SetLastTimePlayedChanged(PlayerEntity player, DateTime dateTime);
        ulong UpdateTimePlayed(string login);
        List<PlayerEntity> GetList(uint top, PlayerSortOrder sorting, bool ascending);
        bool DeleteData(string login);
        PlayerEntity Get(string login);
        PagedList<PlayerEntity> GetListByWins(int? startIndex, int? endIndex);
        PlayerEntity Get(int id);
    }
}