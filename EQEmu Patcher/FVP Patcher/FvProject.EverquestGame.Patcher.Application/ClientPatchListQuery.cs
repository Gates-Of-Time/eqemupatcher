using System.Linq;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;

namespace FvProject.EverquestGame.Patcher.Application {
    public class ClientPatchListQuery : IQuery
    {
        public ClientPatchListQuery(ExpansionsEnum expansion, ServerPatchFileList serverFiles)
        {
            Expansion = expansion;
            ServerFiles = serverFiles;
        }

        public ExpansionsEnum Expansion { get; }
        public ServerPatchFileList ServerFiles { get; }
    }
}
