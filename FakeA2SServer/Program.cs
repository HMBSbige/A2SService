Log.Logger = new LoggerConfiguration()
#if DEBUG
	.MinimumLevel.Debug()
#else
	.MinimumLevel.Information()
#endif
	.MinimumLevel.Override(@"Microsoft", LogEventLevel.Information)
	.MinimumLevel.Override(@"Volo.Abp", LogEventLevel.Warning)
	.Enrich.FromLogContext()
	.WriteTo.Async(c => c.Console(outputTemplate: @"[{Timestamp:O}] [{Level}] {Message:lj}{NewLine}{Exception}"))
	.CreateLogger();

try
{
	HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

	builder.Logging.ClearProviders().AddSerilog();

	builder.ConfigureContainer(builder.Services.AddAutofacServiceProviderFactory());

	builder.Services.AddHostedService<FakeA2SServerHostedService>();

	await builder.Services.AddApplicationAsync<FakeA2SServerModule>();

	using IHost host = builder.Build();

	await host.InitializeAsync();

	await host.RunAsync();

	return 0;
}
catch (HostAbortedException)
{
	throw;
}
catch (Exception ex)
{
	Log.Fatal(ex, @"Host terminated unexpectedly!");
	return 1;
}
finally
{
	Log.CloseAndFlush();
}
