using Application.Core.Products;

using Domain;

using Infrastructure.Persistence.Context;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Core
{
    public class ProductService : IProductService
    {
        private readonly ProductDbContext _dbContext;

        public ProductService(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _dbContext.Products.FindAsync(id);
        }

        public async Task<int> CreateAsync(Product product)
        {
            _dbContext.Products.Add(product);
            _dbContext.SaveChangesAsync();
            return product.Id;
        }

        public async Task UpdateAsync(Product product)
        {
            _dbContext.Entry(product).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

