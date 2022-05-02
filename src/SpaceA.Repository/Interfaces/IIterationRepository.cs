using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Interfaces
{
    public interface IIterationRepository : IRepository<Iteration, uint>
    {
        Task<IList<Iteration>> GetIterationsAsync(uint projectId);

        Task<IList<Iteration>> GetIterationsOfTeamAsync(uint teamId);

        //Task<Dictionary<uint, List<Iteration>>> GetIterationMapAsync(params uint[] teamIds);

        Task<Dictionary<uint, List<uint>>> GetIterationIdMapAsync(params uint[] teamIds);

        Task TransferAsync(uint projectId, string fromPath, uint toId, uint changerId);

        Task RemoveWithSubsAsync(uint projectId, string rootPath);

        Task<uint[]> WorkItemIdsRootedAtAsync(uint projectId, string path);
    }
}