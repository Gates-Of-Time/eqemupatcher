using FvProject.EverquestGame.Patcher.Application.Contracts;
using FvProject.EverquestGame.Patcher.Domain;

namespace FvProject.EverquestGame.Patcher.Application
{
    public class ServerPatchListQuery: IQuery
    {
        public ServerPatchListQuery(GameClientsEnum gameClient, ExpansionsEnum expansion)
        {
            GameClient = gameClient;
            Expansion = expansion;
        }

        public GameClientsEnum GameClient { get; }
        public ExpansionsEnum Expansion { get; }
    }
}