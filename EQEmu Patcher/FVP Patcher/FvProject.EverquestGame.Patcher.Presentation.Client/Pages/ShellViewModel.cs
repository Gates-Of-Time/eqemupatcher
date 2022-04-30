using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using FvProject.EverquestGame.Patcher.Application;
using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;
using FvProject.EverquestGame.Patcher.Infrastructure.Data;
using FvProject.EverquestGame.Patcher.Presentation.Client.Events;
using Stylet;

namespace FvProject.EverquestGame.Patcher.Presentation.Client.Pages {
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IHandle<ExpansionSelectedEvent>, IHandle<GameDirectoryChangedEvent> {
        private readonly IEventAggregator _eventAggregator;
        private readonly IApplicationConfig _applicationConfig;
        private readonly IExternalApplicationService _eqGameApplicationService;
        private readonly HttpClient _httpClient;
        private readonly ExpansionSelectorViewModel _expansionSelectorViewModel;
        private readonly PatchViewModel _patchViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        public ShellViewModel(IEventAggregator eventAggregator, IApplicationConfig applicationConfig) {
            _eventAggregator = eventAggregator;
            _applicationConfig = applicationConfig;
            _eqGameApplicationService = new EqGameApplicationService(applicationConfig);
            _httpClient = new HttpClient();

            _title = "Firione Vie Project Patcher";

            _eventAggregator?.Subscribe(this);

            _expansionSelectorViewModel = new ExpansionSelectorViewModel(eventAggregator, _applicationConfig);
            _patchViewModel = new PatchViewModel(eventAggregator, _applicationConfig, _httpClient);
            _settingsViewModel = new SettingsViewModel(eventAggregator, _applicationConfig);
            Items.Add(_expansionSelectorViewModel);
            Items.Add(_patchViewModel);
            Items.Add(_settingsViewModel);

            ActiveItem = _expansionSelectorViewModel;

            ClientFilesRepository = new ClientFilesRepository(applicationConfig);
            StatusBarViewModel = new StatusBarViewModel(eventAggregator);
        }

        private ConcurrentDictionary<ExpansionsEnum, ServerPatchFileList> ServerFiles { get; } = new ConcurrentDictionary<ExpansionsEnum, ServerPatchFileList>();
        private ClientFilesRepository ClientFilesRepository { get; }
        private ClientPatchFileList CurrentClientPatchFileList { get; set; }
        public StatusBarViewModel StatusBarViewModel { get; }

        private ExpansionsEnum _selectedExpansion;
        public ExpansionsEnum SelectedExpansion {
            get => _selectedExpansion;
            set {
                _selectedExpansion = value;
                if (ActiveItem != _expansionSelectorViewModel) {
                    return;
                }

                var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                Task.Factory.StartNew(async () => {
                    await CheckExpansionPatch(value);
                }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
            }
        }

        private string _title;
        public string Title {
            get => _title;
            set => SetAndNotify(ref _title, value);
        }

        private GameClientsEnum _currentClient = GameClientsEnum.Unknown;
        public GameClientsEnum CurrentClient {
            get => _currentClient;
            set => SetAndNotify(ref _currentClient, value);
        }

        private bool _canPatchClient;
        public bool CanPatchClient {
            get => _canPatchClient;
            set => SetAndNotify(ref _canPatchClient, value);
        }

        private bool _canLaunchClient = false;
        public bool CanLaunchClient {
            get => _canLaunchClient;
            set => SetAndNotify(ref _canLaunchClient, value);
        }

        private bool _canOpenSettings = true;
        public bool CanOpenSettings {
            get => _canOpenSettings;
            set => SetAndNotify(ref _canOpenSettings, value);
        }

        private bool IsAppBusy {
            set {
                CanPatchClient = value;
                CanLaunchClient = value;
                CanOpenSettings = value;
                _expansionSelectorViewModel.CanLeft = value;
                _expansionSelectorViewModel.CanRight = value;
            }
        }

        protected override void OnInitialActivate() {
            base.OnInitialActivate();
            CheckClient();
            LoadPatchFiles();
        }

        public async Task PatchClient() {
            if (ActiveItem != _expansionSelectorViewModel) {
                ActiveItem = _expansionSelectorViewModel;
                if(CurrentClientPatchFileList == null) {
                    await CheckExpansionPatch(SelectedExpansion);
                }

                return;
            }

            if (CurrentClientPatchFileList == null) {
                PublishApplicationStateChangedEvent("No patch available.", Colors.DeepSkyBlue);
                return;
            }

            IsAppBusy = false;
            PublishApplicationStateChangedEvent($"Patching <{_applicationConfig.GameDirectory}>...", Colors.Gold);

            ActiveItem = _patchViewModel;
            await _patchViewModel.PatchClient(CurrentClientPatchFileList);

            PublishApplicationStateChangedEvent($"Patched <{_applicationConfig.GameDirectory}> and ready to launch game.", Colors.Green);
            IsAppBusy = true;
        }

        public void LaunchClient() {
            var launchResult = _eqGameApplicationService.Start();
            if (launchResult.IsFailure) {
                PublishApplicationStateChangedEvent(launchResult.Error, Colors.Red);
            }
            else {
                System.Environment.Exit(0);
            }
        }

        public void OpenSettings() {
            ActiveItem = _settingsViewModel;
        }

        private void CheckClient() {
            PublishApplicationStateChangedEvent("Checking for supported client.", Colors.Gold);
            var launchResult = _eqGameApplicationService.CanExecute;
            if (launchResult.IsFailure) {
                PublishApplicationStateChangedEvent(launchResult.Error, Colors.Red);
            }
            else {

                CurrentClient = launchResult.Value;
                PublishApplicationStateChangedEvent("Supported client found.", Colors.Green);
                CanLaunchClient = true;
            }
        }

        private void LoadPatchFiles() {
            if (CurrentClient == GameClientsEnum.Unknown) {
                return;
            }

            if (_applicationConfig.SupportedExpansions.Count() == 0) {
                PublishApplicationStateChangedEvent("Application config has no supported expansions.", Colors.Red);
                return;
            }

            PublishApplicationStateChangedEvent("Initializing available expansions.", Colors.Gold);
            var repo = new PatchServerRepository(_httpClient);
            var queryHandler = new ServerPatchListQueryHandler(repo);
            var tasks = new List<Task>();
            foreach (var expansion in _applicationConfig.SupportedExpansions) {
                var query = new ServerPatchListQuery(CurrentClient, expansion);
                tasks.Add(Task.Run(async () => {
                    var fileListResult = await queryHandler.ExecuteAsync(query);
                    if (fileListResult.IsSuccess) {
                        ServerFiles.TryAdd(expansion, fileListResult.Value);
                    }
                    else {
                        PublishApplicationStateChangedEvent(fileListResult.Error, Colors.Red);
                    }
                }));
            }

            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Factory.StartNew(async () => {
                await Task.WhenAll(tasks);
                if (ServerFiles.Count() > 0) {
                    PublishApplicationStateChangedEvent("Initialized available expansions.", Colors.Green);
                    CanPatchClient = true;
                    PublishAvailableExpansionsEvent(ServerFiles);
                }
                else {
                    PublishApplicationStateChangedEvent("No available expansions for given client..", Colors.Red);
                }
            }, CancellationToken.None, TaskCreationOptions.None, uiScheduler);
        }

        private async Task CheckExpansionPatch(ExpansionsEnum expansion) {
            IsAppBusy = false;
            PublishApplicationStateChangedEvent($"Checking patch data <{_applicationConfig.GameDirectory}>...", Colors.Gold);
            await Task.Delay(500); // must match animation timer for carousel to avoid animation stutter, should fix diffrently!
            var queryHandler = new ClientPatchListQueryHandler(ClientFilesRepository, ClientFilesRepository);
            var query = new ClientPatchListQuery(expansion, ServerFiles[expansion]);
            CurrentClientPatchFileList = await queryHandler.ExecuteAsync(query);
            CanPatchClient = CurrentClientPatchFileList.HasChanges;
            if (CanPatchClient) {
                var appState = $"Patch available <{_applicationConfig.GameDirectory}>: {CurrentClientPatchFileList.Downloads.Count()} file(s) <{CurrentClientPatchFileList.DownloadSize}>";
                PublishApplicationStateChangedEvent(appState, Colors.Orange);
            }
            else {
                PublishApplicationStateChangedEvent($"Patched <{_applicationConfig.GameDirectory}> and ready to launch game.", Colors.Green);
            }

            IsAppBusy = true;
        }

        #region Event aggregation
        private void PublishApplicationStateChangedEvent(string newState, Color newColor) {
            _eventAggregator.Publish(new ApplicationStateChangedEvent(newState, newColor));
        }

        private void PublishAvailableExpansionsEvent(IDictionary<ExpansionsEnum, ServerPatchFileList> expansionsFiles) {
            _eventAggregator.Publish(new AvailableExpansionsEvent(expansionsFiles));
        }

        public void Handle(ExpansionSelectedEvent message) {
            SelectedExpansion = message.SelectedExpansion;
        }

        public void Handle(GameDirectoryChangedEvent message) {
            CheckClient();
            LoadPatchFiles();
            CurrentClientPatchFileList = null;
        }
        #endregion Event aggregation
    }

    public class ShellDesignViewModel : ShellViewModel {
        public ShellDesignViewModel() : base(null, new ApplicationData()) {
        }
    }
}
