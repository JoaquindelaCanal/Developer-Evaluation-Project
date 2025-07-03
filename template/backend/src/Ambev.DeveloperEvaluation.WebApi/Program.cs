using System.Net.NetworkInformation;

using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Common.HealthChecks;
using Ambev.DeveloperEvaluation.Common.Logging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.IoC;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi.Middleware;

using MediatR;

using Rebus.Config;
using Rebus.ServiceProvider;
using Rebus.Routing.TypeBased;
using Rebus.Auditing.Messages;
using Rebus.Retry.Simple;
using Rebus.Bus;
using Rebus.Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Rebus.DataBus.InMem;
using Rebus.Transport.InMem;

using Serilog;
using Serilog.Events;
using Ambev.DeveloperEvaluation.Application.Common.Contracts.Sales;

namespace Ambev.DeveloperEvaluation.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            builder.AddBasicHealthChecks();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<DefaultContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("ccc")
                )
            );

            builder.Services.AddRebus((configure, serviceProvider) =>
            {
                return configure
                    .Logging(l => l.MicrosoftExtensionsLogging(serviceProvider.GetRequiredService<ILoggerFactory>()))

                    .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "sales-events")) 

                    .Options(o => o.RetryStrategy(
                        maxDeliveryAttempts: 3,
                        secondLevelRetriesEnabled: false
                    ))

                    // Configure type-based routing
                    .Routing(r => r.TypeBased().Map<SaleCreatedIntegrationEvent>("sales-events"))
                    .Routing(r => r.TypeBased().Map<SaleModifiedIntegrationEvent>("sales-events"))
                    .Routing(r => r.TypeBased().Map<SaleCancelledIntegrationEvent>("sales-events"))
                    .Routing(r => r.TypeBased().Map<SaleItemCancelledIntegrationEvent>("sales-events"));
            });

            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.RegisterDependencies();

            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    typeof(ApplicationLayer).Assembly,
                    typeof(Program).Assembly
                );
            });

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            var app = builder.Build();
            app.UseMiddleware<ValidationExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseBasicHealthChecks();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
