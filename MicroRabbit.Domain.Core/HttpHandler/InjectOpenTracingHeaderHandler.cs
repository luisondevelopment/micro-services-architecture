using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MicroRabbit.Domain.Core.HttpHandler
{
    public class InjectOpenTracingHeaderHandler : DelegatingHandler
    {
        private readonly ITracer _tracer;

        public InjectOpenTracingHeaderHandler(ITracer tracer)
        {
            _tracer = tracer;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var span = _tracer.ScopeManager.Active.Span
                .SetTag(Tags.SpanKind, Tags.SpanKindClient)
                .SetTag(Tags.HttpMethod, request.Method.ToString())
                .SetTag(Tags.HttpUrl, request.RequestUri.ToString());

            var dictionary = new Dictionary<string, string>();
            _tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, new TextMapInjectAdapter(dictionary));

            foreach (var entry in dictionary)
                request.Headers.Add(entry.Key, entry.Value);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
