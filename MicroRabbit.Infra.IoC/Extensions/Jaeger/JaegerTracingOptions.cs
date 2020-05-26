using Microsoft.Extensions.Logging;

namespace MicroRabbit.Infra.IoC.Extensions.Jaeger
{
    public class JaegerTracingOptions
    {
        public double SamplingRate { get; set; }
        public double LowerBound { get; set; }
        public ILoggerFactory LoggerFactory { get; set; }
        public string JaegerAgentHost { get; set; }
        public int JaegerAgentPort { get; set; }
        public string ServiceName { get; set; }

        public JaegerTracingOptions()
        {
            SamplingRate = 0.1d;
            LowerBound = 1d;
            LoggerFactory = new LoggerFactory();
            JaegerAgentHost = "192.168.99.100";
            JaegerAgentPort = 16686;
        }
    }
}
