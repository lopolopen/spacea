using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceA.Model.Entities;

namespace SpaceA.Repository.Interfaces
{
    public interface IFolderRepository : IRepository<Folder, uint>
    {
        Task<IList<Folder>> GetFoldersAsync(uint projectId);

        //Task<Dictionary<uint, List<Folder>>> GetFolderMapAsync(params uint[] teamIds);

        Task<Dictionary<uint, List<uint>>> GetFolderIdMapAsync(params uint[] teamIds);

        Task TransferAsync(uint projectId, string fromPath, uint toId, uint changerId);

        Task RemoveWithSubsAsync(uint projectId, string rootPath);

        Task<uint[]> WorkItemIdsRootedAtAsync(uint projectId, string path);
    }
}