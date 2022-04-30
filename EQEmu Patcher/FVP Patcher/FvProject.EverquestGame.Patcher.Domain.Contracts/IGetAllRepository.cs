namespace FvProject.EverquestGame.Patcher.Domain.Contracts
{
    public interface IGetAllRepository<TResult>
    {
        Task<TResult> GetAll();
    }
}