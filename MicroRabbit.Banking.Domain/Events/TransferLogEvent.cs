using MicroRabbit.Domain.Core.Events;
using Newtonsoft.Json;

namespace MicroRabbit.Banking.Domain.Events
{
    public class TransferLogEvent : Event
    {
        public TransferLogEvent(int from, int to, decimal amount)
        {
            Log = JsonConvert.SerializeObject(new { from, to, amount });
        }

        public string Log { get; private set; }
    }
}
