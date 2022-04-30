namespace FvProject.EverquestGame.Patcher.Domain.Contracts {
    public interface IUpsertRepository<TQuery, TResult> {
        Task<TResult> Upsert(TQuery query);
    }
}
