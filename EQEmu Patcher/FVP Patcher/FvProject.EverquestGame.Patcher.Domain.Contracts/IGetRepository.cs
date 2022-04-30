namespace FvProject.EverquestGame.Patcher.Domain.Contracts {
    public interface IGetRepository<TQuery, TResult>
    {
        Task<TResult> Get(TQuery query);
    }

    public interface IGetRepositorySync<TQuery, TResult> {
        TResult Get(TQuery query);
    }
}
