using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Transfer.Domain.Events
{
    public class TransferLogEvent : Event
    {
        public TransferLogEvent(string log)
        {
            Log = log;
        }

        public string Log { get; set; }
    }
}
