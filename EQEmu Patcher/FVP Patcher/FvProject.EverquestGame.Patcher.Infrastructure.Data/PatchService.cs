using System.Net;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Domain.Contracts;

namespace FvProject.EverquestGame.Patcher.Infrastructure.Data {
    public class PatchService : IPatchService {
        public PatchService(IApplicationConfig applicationConfig, WebClient webClient) {
            ApplicationConfig = applicationConfig;
            WebClient = webClient;
            webClient.Encoding = System.Text.Encoding.UTF8;
        }

        private IApplicationConfig ApplicationConfig { get; }
        private WebClient WebClient { get; }

        public async Task ExecuteAsync(IDeleteRepository<FileEntry> deleteRepository, ClientPatchFileList patchList, IProgressReporter progressReporter) {
            progressReporter.Report($"Patching from {patchList.Downloadprefix}...");
            foreach (var deleteFile in patchList.Deletes) {
                progressReporter.Report($"Deleting {deleteFile.name}...");
                await deleteRepository.Delete(deleteFile);
            }

            var totalDownloadSize = patchList.Downloads.Sum(x => x.size < 1 ? 1 : x.size);
            var curBytes = 0L;
            progressReporter.Report($"Downloading {totalDownloadSize} bytes for {patchList.Downloads.Count()} file(s)...");
            foreach (var downloadFile in patchList.Downloads) {
                var url = patchList.Downloadprefix + downloadFile.name.Replace("\\", "/");
                var filePath = downloadFile.name.Replace("/", @"\");
                if (filePath.Contains('\\')) { //Make directory if needed.
                    var dir = $@"{ApplicationConfig.GameDirectory}\{filePath.Substring(0, filePath.LastIndexOf(@"\"))}";
                    Directory.CreateDirectory(dir);
                }

                filePath = $@"{ApplicationConfig.GameDirectory}\{filePath}";
                WebClient.DownloadFileAsync(new Uri(url), filePath);

                curBytes += downloadFile.size;
                progressReporter.Progress.Report(curBytes * 100 / totalDownloadSize);
                progressReporter.Report($"{downloadFile.name}...");

                var progress = curBytes * 100 / totalDownloadSize;
                progressReporter.Progress.Report(progress);
            }
        }
    }
}
