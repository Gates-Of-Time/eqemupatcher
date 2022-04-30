using CSharpFunctionalExtensions;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Application.Extensions;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Domain.Contracts;

namespace FvProject.EverquestGame.Patcher.Infrastructure.Data {
    public class ClientFilesRepository : IGetAllRepository<IEnumerable<string>>, IDeleteRepository<FileEntry>, IUpsertRepository<PatchFile, Result>, IGetRepositorySync<FileEntry, Stream> {
        public ClientFilesRepository(IApplicationConfig applicationConfig) {
            ApplicationConfig = applicationConfig;
        }

        private IApplicationConfig ApplicationConfig { get; }

        public async Task Delete(FileEntry fileEntry) {
            var filePath = fileEntry.name.Replace("/", @"\");
            filePath = $@"{ApplicationConfig.GameDirectory}\{filePath}";
            await Task.Run(() => { 
                if (File.Exists(fileEntry.name)) {
                    File.Delete(fileEntry.name);
                }
            });
        }

        public async Task<Result> Upsert(PatchFile upsert) {
            try {
                var filePath = upsert.FileEntry.name.Replace("/", @"\");
                EnsureDirectory(filePath);
                filePath = $@"{ApplicationConfig.GameDirectory}\{filePath}";
                using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
                await upsert.DownloadStream.CopyToAsync(fileStream, 81920, upsert.ProgressReporter);

                //using var reader = new StreamReader(upsert.downloadStream, System.Text.Encoding.UTF8);
                //using var writer = new StreamWriter(filePath, append: false);
                //await writer.WriteAsync(await reader.ReadToEndAsync());
            }
            catch (Exception ex) {
                return Result.Failure(ex.Message);
            }

            return Result.Success();
        }

        private void EnsureDirectory(string filePath) {
            if (filePath.Contains('\\')) { //Make directory if needed.
                var dir = $@"{ApplicationConfig.GameDirectory}\{filePath.Substring(0, filePath.LastIndexOf(@"\"))}";
                Directory.CreateDirectory(dir);
            }
        }

        public async Task<IEnumerable<string>> GetAll()
        {
            return await Task.Run(() =>
                Directory.GetFiles(ApplicationConfig.GameDirectory, "*.*", SearchOption.AllDirectories)
                         .Select(x => x.Replace(ApplicationConfig.GameDirectory, "", StringComparison.InvariantCultureIgnoreCase).TrimStart('\\'))
            );
        }

        public Stream Get(FileEntry fileEntry) {
            var filePath = $@"{ApplicationConfig.GameDirectory}\{fileEntry.name.Replace("/", @"\")}";
            return File.OpenRead(filePath);
        }
    }
}
