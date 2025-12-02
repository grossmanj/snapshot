using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Application.Services;
using CustomerSnapshot.Infrastructure.Repositories;
using CustomerSnapshot.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows.Forms;

namespace CustomerSnapshot.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        var initialCustomerId = TryParseCustomerId(args);

        var host = CreateHostBuilder(args).Build();

        ApplicationConfiguration.Initialize();

        var form = host.Services.GetRequiredService<MainForm>();
        if (initialCustomerId.HasValue)
        {
            form.InitialCustomerId = initialCustomerId.Value;
        }

        Application.Run(form);
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Repositories
                services.AddSingleton<ICustomerRepository, CustomerRepository>();
                services.AddSingleton<IOrderRepository, OrderRepository>();
                services.AddSingleton<IInvoiceRepository, InvoiceRepository>();
                services.AddSingleton<IQuoteRepository, QuoteRepository>();
                services.AddSingleton<IIssueRepository, IssueRepository>();
                services.AddSingleton<IInteractionRepository, InteractionRepository>();

                // Services
                services.AddSingleton<ICustomerKpiService, CustomerKpiService>();
                services.AddSingleton<IPersonalityProfileService, PersonalityProfileService>();
                services.AddSingleton<ITalkingPointsService, TalkingPointsService>();
                services.AddSingleton<INextBestActionService, NextBestActionService>();
                services.AddSingleton<ICustomerSnapshotService, CustomerSnapshotService>();

                // UI
                services.AddSingleton<MainForm>();
            });

    private static int? TryParseCustomerId(IEnumerable<string> args)
    {
        foreach (var arg in args)
        {
            if (int.TryParse(arg, out var id))
            {
                return id;
            }
        }

        return null;
    }
}
