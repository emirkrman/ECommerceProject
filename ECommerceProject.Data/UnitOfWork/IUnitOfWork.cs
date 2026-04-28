namespace ECommerceProject.Data.UnitOfWork;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
