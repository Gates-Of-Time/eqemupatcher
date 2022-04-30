using CSharpFunctionalExtensions;
using FvProject.EverquestGame.Patcher.Domain;

namespace FvProject.EverquestGame.Patcher.Application.Contracts {
    public interface IExternalApplicationService {
        Result<GameClientsEnum> CanExecute { get; }
        Result Start();
    }
}
