using System.Collections.Generic;
using FvProject.EverquestGame.Patcher.Domain;

namespace FvProject.EverquestGame.Patcher.Presentation.Client.Events {
    public class AvailableExpansionsEvent {
        public AvailableExpansionsEvent(IDictionary<ExpansionsEnum, ServerPatchFileList> expansionsFiles) {
            ExpansionsFiles = expansionsFiles;
        }

        public IDictionary<ExpansionsEnum, ServerPatchFileList> ExpansionsFiles { get; }
    }
}
