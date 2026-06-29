using DevisHp.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        var useCsv = (Environment.GetEnvironmentVariable("USE_CSV_REPOSITORY") ?? "false")
            .Equals("true", StringComparison.OrdinalIgnoreCase);

        if (useCsv)
        {
            var seedPath = Environment.GetEnvironmentVariable("SEED_DATA_PATH") ?? "..\\seed-data";
            services.AddSingleton<IReferenceRepository>(_ => new CsvReferenceRepository(seedPath));
        }
        else
        {
            var conn = Environment.GetEnvironmentVariable("DataverseConnectionString")
                ?? throw new InvalidOperationException("DataverseConnectionString is not set.");
            services.AddSingleton<IReferenceRepository>(_ => new DataverseReferenceRepository(conn));
        }
    })
    .Build();

host.Run();
