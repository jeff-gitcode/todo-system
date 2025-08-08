using System;
using System.Threading.Tasks;
using TodoSystem.Domain.Entities;

namespace TodoSystem.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<bool> EmailExistsAsync(string email);
    }
}