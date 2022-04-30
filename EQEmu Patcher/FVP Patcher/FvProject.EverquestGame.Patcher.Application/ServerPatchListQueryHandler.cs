using CSharpFunctionalExtensions;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Domain.Contracts;

namespace FvProject.EverquestGame.Patcher.Application
{
    public class ServerPatchListQueryHandler : IQueryHandler<ServerPatchListQuery, Result<ServerPatchFileList>>
    {
        public ServerPatchListQueryHandler(IGetRepository<ServerPatchListQuery, Result<ServerPatchFileList>> fileListRepository)
        {
            FileListRepository = fileListRepository;
        }

        private IGetRepository<ServerPatchListQuery, Result<ServerPatchFileList>> FileListRepository { get; }

        public async Task<Result<ServerPatchFileList>> ExecuteAsync(ServerPatchListQuery query)
        {
            return await FileListRepository.Get(query);
        }
    }
}
