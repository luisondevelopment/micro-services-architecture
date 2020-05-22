using MicroRabbit.Transfer.Domain.Models;

namespace MicroRabbit.Transfer.Domain.Interfaces
{
    public interface ITransferLogNoSQLRepository
    {
        void Add(TransferLogNoSQL transferLog);
    }
}
