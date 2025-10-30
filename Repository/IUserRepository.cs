using Backend.Entities;

namespace Backend.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task EnsureDefaultUserAsync(User user, CancellationToken cancellationToken);
    }
}
