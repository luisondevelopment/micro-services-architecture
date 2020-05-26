using MicroRabbit.MVC.Models.DTO;
using Refit;
using System.Threading.Tasks;

namespace MicroRabbit.MVC.Services
{
    public interface IBankingService
    {
        [Post("/api/banking")]
        Task<ApiResponse<dynamic>> Banking(TransferDTO transferDTO);
    }
}
