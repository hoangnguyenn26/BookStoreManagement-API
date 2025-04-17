
using Bookstore.Application.Dtos.Dashboard;

namespace Bookstore.Application.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<HomeDashboardDto> GetHomeDashboardAsync(CancellationToken cancellationToken = default);
    }
}