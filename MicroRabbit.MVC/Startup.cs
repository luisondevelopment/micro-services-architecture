using Jaeger;
using Jaeger.Samplers;
using MicroRabbit.Domain.Core.HttpHandler;
using MicroRabbit.Infra.IoC.Extensions.Jaeger;
using MicroRabbit.Infra.IoC.Extensions.Refit;
using MicroRabbit.MVC.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing.Util;
using zipkin4net;
using zipkin4net.Middleware;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace MicroRabbit.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddJaegerTracing(options => {
                options.JaegerAgentHost = "192.168.99.100";
                options.JaegerAgentPort = 6831;
                options.ServiceName = "Front";
                options.LoggerFactory = LoggerFactory;
            });

            //services.AddOpenTracing();

            //services.AddHttpClient("Tracer").AddHttpMessageHandler(provider =>
            //   TracingHandler.WithoutInnerHandler("MicroRabbit.MVC"));

            //services.AddTransient<InjectOpenTracingHeaderHandler>();

            //services.AddHttpClient("Jaeger").AddHttpMessageHandler<InjectOpenTracingHeaderHandler>();

            services.RegisterRefitClientServices<IBankingService>("https://localhost:5001");


            //services.AddSingleton<OpenTracing.ITracer>(serviceProvider =>
            //{
            //    var serviceName = serviceProvider
            //        .GetRequiredService<IHostingEnvironment>()
            //        .ApplicationName;

            //    var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            //    var tracer = new Tracer.Builder(serviceName)
            //        .WithSampler(new ConstSampler(true))
            //        .WithLoggerFactory(loggerFactory)
            //        .Build();

            //    // Allows code that can't use DI to also access the tracer.
            //    GlobalTracer.Register(tracer);

            //    return tracer;
            //});


            services.AddHttpClient<ITransferService, TransferService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            //var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();

            //lifetime.ApplicationStarted.Register(() => {
            //    TraceManager.SamplingRate = 1.0f;
            //    var logger = new TracingLogger(loggerFactory, "zipkin4net");
            //    var httpSender = new HttpZipkinSender("http://192.168.99.100:9411/", "application/json");
            //    var tracer = new ZipkinTracer(httpSender, new JSONSpanSerializer());
            //    TraceManager.RegisterTracer(tracer);
            //    TraceManager.Start(logger);
            //});

            //lifetime.ApplicationStopped.Register(() => TraceManager.Stop());

            //app.UseTracing("MicroRabbit.MVC");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
