using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FvProject.EverquestGame.Patcher.Application;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Infrastructure.Data;
using Stylet;

namespace FvProject.EverquestGame.Patcher.Presentation.Client.Pages {
    public class PatchViewModel : Screen, IProgressReporter {
        private readonly IEventAggregator _eventAggregator;
        private readonly IApplicationConfig _applicationConfig;
        private readonly HttpClient _httpClient;
        private readonly WebClient _webClient;
        private readonly StringBuilder _stringBuilder;

        public PatchViewModel(IEventAggregator eventAggregator, IApplicationConfig applicationConfig, HttpClient httpClient) {
            _eventAggregator = eventAggregator;
            _applicationConfig = applicationConfig;
            _httpClient = httpClient;
            _webClient = new WebClient();
            _stringBuilder = new StringBuilder();
            Progress = new Progress<double>(progress => {
                ProgressValue = progress;
            });
        }


        private double _progressValue = 0;
        public double ProgressValue {
            get => _progressValue;
            set => SetAndNotify(ref _progressValue, value);
        }

        private string _patchLog = "";
        public string PatchLog {
            get => _patchLog;
            set => SetAndNotify(ref _patchLog, value);
        }

        public async Task PatchClient(ClientPatchFileList clientPatchFileList) {
            PatchLog = "";
            ProgressValue = 0;
            _stringBuilder.Clear();           

            //var command = new PatchCommand(clientPatchFileList, this);
            //var commandHandler = new PatchCommandHandler(new ClientFilesRepository(_applicationConfig), new PatchServerRepository(_httpClient), new ClientFilesRepository(_applicationConfig));
            //await commandHandler.Execute(command);

            var patchService = new PatchService(_applicationConfig, _webClient);
            await patchService.ExecuteAsync(new ClientFilesRepository(_applicationConfig), clientPatchFileList, this);
        }

        #region IProgressReporter
        public void Report(string message) {
            _stringBuilder.AppendLine(message);
            PatchLog = _stringBuilder.ToString();
        }

        public IProgress<double> Progress { get; }
        #endregion IProgressReporter
    }

    public class PatchDesignViewModel : PatchViewModel {
        public PatchDesignViewModel() : base(null, new ApplicationData(), null) {
        }
    }
}
