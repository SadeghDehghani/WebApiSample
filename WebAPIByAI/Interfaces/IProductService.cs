using WebAPIByAI.Models;

namespace WebAPIByAI.Interfaces
{
    public interface IProductService
    {
        IEnumerable<Product> GetAll();
    }
}
