namespace FakeA2SServer;

[UsedImplicitly]
public class A2SServerService : ITransientDependency
{
	public required IAbpLazyServiceProvider LazyServiceProvider { get; [UsedImplicitly] init; }

	private ILogger<A2SServerService> Logger => LazyServiceProvider.LazyGetRequiredService<ILogger<A2SServerService>>();

	private IConfiguration Configuration => LazyServiceProvider.LazyGetRequiredService<IConfiguration>();

	private readonly CancellationTokenSource _cts = new();

	private A2SServer? _server;

	public async ValueTask StartAsync()
	{
		IPEndPoint serverAddress = IPEndPoint.Parse(Configuration.GetValue(@"A2SListenEndpoint", @"[::]:27015")!);

		_server = new A2SServer(serverAddress);
		if (Equals(serverAddress.Address, IPAddress.IPv6Any))
		{
			_server.Server.Client.DualMode = true;
		}

		const string prefix = @"A2S";

		_server.A2SInfo = new A2SInfo
		{
			Protocol = Configuration.GetValue<byte>(prefix + nameof(A2SInfo.Protocol)),
			Name = Configuration.GetValue<string?>(prefix + nameof(A2SInfo.Name)),
			Map = Configuration.GetValue<string?>(prefix + nameof(A2SInfo.Map)),
			Folder = Configuration.GetValue<string?>(prefix + nameof(A2SInfo.Folder)),
			Game = Configuration.GetValue<string?>(prefix + nameof(A2SInfo.Game)),
			ID = (short)Configuration.GetValue<ushort>(prefix + nameof(A2SInfo.ID)),
			Players = Configuration.GetValue<byte>(prefix + nameof(A2SInfo.Players)),
			MaxPlayers = Configuration.GetValue<byte>(prefix + nameof(A2SInfo.MaxPlayers)),
			Bots = Configuration.GetValue<byte>(prefix + nameof(A2SInfo.Bots)),
			ServerType = Configuration.GetValue(prefix + nameof(A2SInfo.ServerType), A2SType.Dedicated),
			Environment = Configuration.GetValue(prefix + nameof(A2SInfo.Environment), _server.A2SInfo.Environment),
			Visibility = Configuration.GetValue<A2SVisibility>(prefix + nameof(A2SInfo.Visibility)),
			Vac = Configuration.GetValue<A2SVacStatus>(prefix + nameof(A2SInfo.Vac)),
			Version = Configuration.GetValue<string?>(prefix + nameof(A2SInfo.Version)),
			Port = (short)Configuration.GetValue(prefix + nameof(A2SInfo.Port), (ushort)serverAddress.Port),
			SteamID = (long)Configuration.GetValue<ulong>(prefix + nameof(A2SInfo.SteamID)),
			SourceTvPort = (short)Configuration.GetValue<ushort>(prefix + nameof(A2SInfo.SourceTvPort)),
			SourceTvName = Configuration.GetValue<string?>(prefix + nameof(A2SInfo.SourceTvName)),
			Keywords = Configuration.GetValue<string?>(prefix + nameof(A2SInfo.Keywords)),
			GameID = (long)Configuration.GetValue<ulong>(prefix + nameof(A2SInfo.GameID))
		};

		_cts.Token.Register(() => _server.Dispose());

		ValueTask _ = _server.StartAsync(_cts.Token);

		Logger.LogInformation(@"A2S Server listen on {endpoint}: {info}", serverAddress, _server.A2SInfo);

		await ValueTask.CompletedTask;
	}

	public async ValueTask StopAsync()
	{
		await _cts.CancelAsync();
	}
}
