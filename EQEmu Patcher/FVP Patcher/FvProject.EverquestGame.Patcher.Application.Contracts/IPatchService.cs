using System.Net;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Domain.Contracts;

namespace FvProject.EverquestGame.Patcher.Application.Contracts {
    public interface IPatchService {
        Task ExecuteAsync(IDeleteRepository<FileEntry> deleteRepository, ClientPatchFileList patchList, IProgressReporter progressReporter);
    }
}
