using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;
using System.Threading.Tasks;

namespace MicroRabbit.Transfer.Domain.EventHandlers
{
    public class TransferLogEventHandler : IEventHandler<TransferLogEvent>
    {
        private readonly ITransferLogNoSQLRepository _transferLogNoSQLRepository;
        public TransferLogEventHandler(ITransferLogNoSQLRepository transferLogNoSQLRepository)
        {
            _transferLogNoSQLRepository = transferLogNoSQLRepository;
        }

        public Task Handle(TransferLogEvent @event)
        {
            _transferLogNoSQLRepository.Add(new TransferLogNoSQL() { Log = @event.Log });
            return Task.CompletedTask;
        }
    }
}
