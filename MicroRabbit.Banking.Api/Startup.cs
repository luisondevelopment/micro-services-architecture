using MediatR;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Infra.IoC;
using MicroRabbit.Infra.IoC.Extensions.Jaeger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using zipkin4net;
using zipkin4net.Middleware;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace MicroRabbit.Banking.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory _loggerFactory;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BankingDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("BankingDbConnection"));
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Banking Microservice", Version = "v1" });
            });

            //services.AddOpenTracing();

            services.AddMediatR(typeof(Startup));

            RegisterServices(services);

            services.AddJaegerTracing(options => {
                options.JaegerAgentHost = "192.168.99.100";
                options.JaegerAgentPort = 6831;
                options.ServiceName = "Banking";
                options.LoggerFactory = _loggerFactory;
            });
        }

        private void RegisterServices(IServiceCollection services)
        {
            DependencyContainer.RegisterServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking Microservice V1");
            });


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

            //app.UseTracing("MicroRabbit.Banking.Api");


            app.UseMvc();
        }
    }
}
