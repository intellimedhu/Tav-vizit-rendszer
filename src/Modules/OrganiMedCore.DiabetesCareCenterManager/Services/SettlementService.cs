using OrganiMedCore.DiabetesCareCenter.Core.Models;
using OrganiMedCore.DiabetesCareCenterManager.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrganiMedCore.DiabetesCareCenterManager.Services
{
    public class SettlementService : ISettlementService
    {
        private readonly ISession _session;

        private const int PaginationLimit = 10;


        public SettlementService(ISession session)
        {
            _session = session;
        }


        public async Task<Settlement> GetSettlementAsync(int id)
            => await _session.GetAsync<Settlement>(id);

        public async Task<IEnumerable<Settlement>> GetSettlementsAsync(int territoryId, int page, string q = null)
            => await GetSettlementsQuery(territoryId, q)
                    .OrderBy(x => x.ZipCode)
                    .ThenBy(x => x.Name)
                    .Skip(page * PaginationLimit)
                    .Take(PaginationLimit)
                    .ListAsync();

        public async Task<int> GetSettlementsCountAsync(int territoryId, string q)
            => await GetSettlementsQuery(territoryId, q).CountAsync();

        public async Task SaveSettlementAsync(Settlement settlement)
        {
            if (settlement.Id != 0)
            {
                var existing = await GetSettlementAsync(settlement.Id);
                if (existing != null)
                {
                    var changed = false;
                    if (existing.Description != settlement.Description)
                    {
                        existing.Description = settlement.Description;
                        changed = true;
                    }

                    if (existing.Name != settlement.Name)
                    {
                        existing.Name = settlement.Name;
                        changed = true;
                    }

                    if (existing.ZipCode != settlement.ZipCode)
                    {
                        existing.ZipCode = settlement.ZipCode;
                        changed = true;
                    }

                    if (changed)
                    {
                        _session.Save(existing);
                    }
                }
            }
            else
            {
                _session.Save(settlement);
            }
        }

        public async Task DeleteSettlementAsync(int id)
        {
            var settlement = await GetSettlementAsync(id);
            if (settlement != null)
            {
                _session.Delete(settlement);
            }
        }


        private IQuery<Settlement, SettlementIndex> GetSettlementsQuery(int territoryId, string q)
        {
            var query = _session.Query<Settlement, SettlementIndex>(index => index.TerritoryId == territoryId);
            if (!string.IsNullOrEmpty(q))
            {
                q = q?.ToLower();

                query = query.Where(x =>
                    x.ZipCode.Contains(q) ||
                    x.Name.Contains(q));
            }

            return query;
        }
    }
}
