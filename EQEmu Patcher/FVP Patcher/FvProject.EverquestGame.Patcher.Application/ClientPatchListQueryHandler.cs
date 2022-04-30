using CSharpFunctionalExtensions;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Domain.Contracts;

namespace FvProject.EverquestGame.Patcher.Application
{
    public class ClientPatchListQueryHandler : IQueryHandler<ClientPatchListQuery, ClientPatchFileList>
    {
        public ClientPatchListQueryHandler(IGetAllRepository<IEnumerable<string>> getClientFilesRepository, IGetRepositorySync<FileEntry, Stream> getClientFileRepository)
        {
            GetClientFilesRepository = getClientFilesRepository;
            GetClientFileRepository = getClientFileRepository;
            StringComparer = StringComparer.Create(System.Globalization.CultureInfo.CurrentCulture, true);
            Md5Hasher = new MD5Hasher();
        }

        private IGetAllRepository<IEnumerable<string>> GetClientFilesRepository { get; }
        private IGetRepositorySync<FileEntry, Stream> GetClientFileRepository { get; }
        private StringComparer StringComparer { get; }
        private MD5Hasher Md5Hasher { get; }

        public async Task<ClientPatchFileList> ExecuteAsync(ClientPatchListQuery query)
        {
            var clientFiles = await GetClientFilesRepository.GetAll();
            var deleteFiles = query.ServerFiles.deletes.Where(serverFile => clientFiles.Contains(serverFile.name)).ToArray();
            var missingFiles = query.ServerFiles.downloads.Where(ShouldUpdateFile(clientFiles)).ToArray();
            return new ClientPatchFileList() { Expansion = query.Expansion, Downloads = missingFiles, Deletes = deleteFiles, Downloadprefix = query.ServerFiles.downloadprefix };
        }

        Func<FileEntry, bool> ShouldUpdateFile(IEnumerable<string> clientFiles) {
            bool isFileUpdated(FileEntry serverFile) => clientFiles.Any(clientFile => clientFile == serverFile.name.Replace("/", @"\") && HasWrongMd5CheckSum(serverFile));
            bool isFileMissing(FileEntry serverFile) => !clientFiles.Any(clientFile => clientFile == serverFile.name.Replace("/", @"\"));
            return (FileEntry serverFile) => isFileMissing(serverFile) || isFileUpdated(serverFile);
        }

        private bool HasWrongMd5CheckSum(FileEntry serverFile) {
            using var fileStream = GetClientFileRepository.Get(serverFile);
            var clientMd5 = Md5Hasher.CreateHash(fileStream);            
            return StringComparer.Compare(clientMd5, serverFile.md5) != 0;
        }
    }
}
