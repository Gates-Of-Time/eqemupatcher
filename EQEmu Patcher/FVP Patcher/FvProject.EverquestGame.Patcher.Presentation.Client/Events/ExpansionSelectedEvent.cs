using FvProject.EverquestGame.Patcher.Domain;

namespace FvProject.EverquestGame.Patcher.Presentation.Client.Events {
    public class ExpansionSelectedEvent {
        public ExpansionSelectedEvent(ExpansionsEnum expansionsEnum) {
            SelectedExpansion = expansionsEnum;
        }

        public ExpansionsEnum SelectedExpansion { get; }
    }
}
