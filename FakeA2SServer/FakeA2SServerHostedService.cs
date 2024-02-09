namespace FakeA2SServer;

public class FakeA2SServerHostedService : IHostedService
{
	public required IAbpLazyServiceProvider LazyServiceProvider { get; [UsedImplicitly] init; }

	private A2SServerService Service => LazyServiceProvider.LazyGetRequiredService<A2SServerService>();

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await Service.StartAsync();
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		await Service.StopAsync();
	}
}
