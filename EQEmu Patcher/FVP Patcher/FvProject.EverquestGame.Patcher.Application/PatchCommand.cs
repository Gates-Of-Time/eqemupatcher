using CSharpFunctionalExtensions;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Domain.Contracts;

namespace FvProject.EverquestGame.Patcher.Application {
    public class PatchCommand : ICommand{
        public PatchCommand(ClientPatchFileList patchList, IProgressReporter progressReporter) {
            PatchList = patchList;
            ProgressReporter = progressReporter;
        }

        public ClientPatchFileList PatchList { get; }
        public IProgressReporter ProgressReporter { get; }
    }

    public class PatchCommandHandler : ICommandHandler<PatchCommand> {
        public PatchCommandHandler(IDeleteRepository<FileEntry> deleteRepository, IGetRepository<string, Result<Stream>> getServerFileRepository, IUpsertRepository<PatchFile, Result> upsertRepository) {
            ClientFilesDeleter = deleteRepository;
            FileServer = getServerFileRepository;
            UpsertRepository = upsertRepository;
        }

        private IDeleteRepository<FileEntry> ClientFilesDeleter { get; }
        private IGetRepository<string, Result<Stream>> FileServer { get; }
        private IUpsertRepository<PatchFile, Result> UpsertRepository { get; }

        public async Task Execute(PatchCommand command) {
            var progressReporter = command.ProgressReporter;

            progressReporter.Report($"Patching from {command.PatchList.Downloadprefix}...");
            foreach (var deleteFile in command.PatchList.Deletes) {
                progressReporter.Report($"Deleting {deleteFile.name}...");
                await ClientFilesDeleter.Delete(deleteFile);
            }

            var totalDownloadSize = command.PatchList.Downloads.Sum(x => x.size < 1 ? 1 : x.size);
            var curBytes = 0L;
            progressReporter.Report($"Downloading {totalDownloadSize} bytes for {command.PatchList.Downloads.Count()} file(s)...");
            foreach (var downloadFile in command.PatchList.Downloads) {
                var url = command.PatchList.Downloadprefix + downloadFile.name.Replace("\\", "/");
                var downloadResult = await FileServer.Get(url);
                if (downloadResult.IsFailure) {
                    curBytes += downloadFile.size < 1 ? 1 : downloadFile.size;
                    progressReporter.Report($"Failed to download <{downloadFile.name}> due to {downloadResult.Error}");
                }
                else {
                    var fileBytes = 0L;
                    var relativeProgress = new Progress<long>(totalBytes => {
                        if ((totalBytes - fileBytes) * 100 / totalDownloadSize < 1) {
                            return;
                        }

                        curBytes += totalBytes - fileBytes;
                        fileBytes = totalBytes;
                        var progress = curBytes * 100 / totalDownloadSize;
                        progressReporter.Progress.Report(progress);
                    });
                    using var downloadStream = downloadResult.Value;
                    var result = await UpsertRepository.Upsert(new PatchFile(downloadStream, downloadFile, relativeProgress));
                    if (result.IsSuccess) {
                        curBytes += downloadFile.size - fileBytes;
                        progressReporter.Progress.Report(curBytes * 100 / totalDownloadSize);
                        progressReporter.Report($"{downloadFile.name}...");
                    }
                    else {
                        curBytes += downloadFile.size < 1 ? 1 : downloadFile.size;
                        progressReporter.Report($"Failed writing {downloadFile.name}...");
                        progressReporter.Report($"\t {result.Error}");
                    }
                }

                var progress = curBytes * 100 / totalDownloadSize;
                progressReporter.Progress.Report(progress);
            }
        }
    }
}
