using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;

namespace MicroRabbit.Transfer.Data.Repository
{
    public class TransferLogNoSQLRepository : ITransferLogNoSQLRepository
    {
        private TransferDbContext _ctx;

        public TransferLogNoSQLRepository(TransferDbContext ctx)
        {
            _ctx = ctx;
        }

        public void Add(TransferLogNoSQL transferLog)
        {
            _ctx.TransferLogsNoSQL.Add(transferLog);
            _ctx.SaveChanges();
        }
    }
}
