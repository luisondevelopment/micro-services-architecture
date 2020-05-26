using Jaeger;
using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using OpenTracing;
using OpenTracing.Mock;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using OpenTracing.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.Infra.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        private readonly IMediator _mediator;
        private readonly Dictionary<string, List<Type>> _handlers;
        private readonly List<Type> _eventTypes;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public RabbitMQBus(IMediator mediator, IServiceScopeFactory serviceScopeFactory)
        {
            _mediator = mediator;
            _serviceScopeFactory = serviceScopeFactory; //Estudar
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            return _mediator.Send(command);
        }

        public void Publish<T>(T @event) where T : Event
        {
            var tracer = GlobalTracer.Instance;

            using (IScope scope = GlobalTracer.Instance.BuildSpan("send")
                    .WithTag(Tags.SpanKind.Key, Tags.SpanKindClient)
                    .WithTag(Tags.Component.Key, "example-client")
                    .StartActive(finishSpanOnDispose: true))
            {
                var headers = new Dictionary<string, string>();
                var adapter = new TextMapInjectAdapter(headers);

                GlobalTracer.Instance.Inject(scope.Span.Context, BuiltinFormats.TextMap, adapter);

                tracer.ActiveSpan?.Log(new Dictionary<string, object> {
                    { "event", nameof(@event) },
                    { "type", @event.GetType().ToString()},
                    { "customer_name", "teste" },
                    { "item_number", "teste" },
                    { "quantity", "teste" }
                });

                var factory = new ConnectionFactory() { HostName = "192.168.99.100" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();

                    properties.Headers = new Dictionary<string, object>();

                    foreach (var item in headers)
                    {
                        properties.Headers.Add(new KeyValuePair<string, object>(item.Key, item.Value));
                    }

                    var eventName = @event.GetType().Name;

                    channel.QueueDeclare(eventName, false, false, false, null);

                    var message = JsonConvert.SerializeObject(@event);
                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish("", eventName, properties, body);
                }
            }

        }

        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }

            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }

            if (_handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException($"Handler type {handlerType.Name} already is registerd for {eventName}", nameof(handlerType));
            }

            _handlers[eventName].Add(handlerType);

            StartBasicConsume<T>();
        }

        private void StartBasicConsume<T>() where T : Event
        {
            var factory = new ConnectionFactory() { HostName = "192.168.99.100", DispatchConsumersAsync = true };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            var eventName = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body);

            var headers = new Dictionary<string, string>();

            try
            {
                foreach(var header in e.BasicProperties.Headers)
                {
                    headers.Add(header.Key, Encoding.UTF8.GetString((byte[])header.Value));
                }

                GlobalTracer.Instance?.ActiveSpan?.Log(new Dictionary<string, object> {
                    { "event", message },
                    { "type", message},
                    { "customer_name", "teste" },
                    { "item_number", "teste" },
                    { "quantity", "teste" }
                });

                ISpanBuilder spanBuilder;

                IPropagator propagator = new BinaryPropagator();

                try
                {
                    var parentSpanCtx = GlobalTracer.Instance.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(headers));

                    spanBuilder = GlobalTracer.Instance.BuildSpan("a");
                    if (parentSpanCtx != null)
                    {
                        spanBuilder = spanBuilder.AsChildOf(parentSpanCtx);
                    }
                }
                catch (Exception)
                {
                    spanBuilder = GlobalTracer.Instance.BuildSpan(("a"));
                }

                spanBuilder
                    .WithTag("source-host-process-id", Process.GetCurrentProcess().Id);

                using (var scope = spanBuilder.StartActive(true))
                {
                    await ProcessEvent(eventName, message).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var subscriptions = _handlers[eventName];

                    foreach (var subscription in subscriptions)
                    {
                        //var handler = Activator.CreateInstance(subscription);

                        var handler = scope.ServiceProvider.GetService(subscription);

                        if (handler == null)
                            continue;

                        var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                        var @event = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                    }
                }
            }
        }
    }
}
