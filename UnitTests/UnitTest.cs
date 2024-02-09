using A2SService;
using SteamQuery;
using SteamQuery.Enums;
using SteamQuery.Models;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace UnitTests;

[TestClass]
public class UnitTest
{
	[TestMethod]
	public async Task TestMethodAsync()
	{
		IPEndPoint serverAddress = IPEndPoint.Parse(@"[::]:27015");
		using A2SServer server = new(serverAddress);
		if (serverAddress.AddressFamily is AddressFamily.InterNetworkV6)
		{
			server.Server.Client.DualMode = true;
		}

		server.A2SInfo = new A2SInfo
		{
			Protocol = RandomNumberGenerator.GetBytes(1).First(),
			Name = @"Genshin Impact",
			Map = @"いいよ，こいよ",
			Folder = @"Palworld",
			Game = @"Palworld",
			Players = RandomNumberGenerator.GetBytes(1).First(),
			MaxPlayers = RandomNumberGenerator.GetBytes(1).First(),
			Bots = RandomNumberGenerator.GetBytes(1).First(),
			ServerType = A2SType.Dedicated,
			Environment = A2SEnvironment.Linux,
			Visibility = A2SVisibility.Public,
			Vac = A2SVacStatus.Secured,
			Version = @"114.514.1919.810",
			Port = BinaryPrimitives.ReadInt16LittleEndian(RandomNumberGenerator.GetBytes(2)),
			SteamID = BinaryPrimitives.ReadInt64LittleEndian(RandomNumberGenerator.GetBytes(8)),
			SourceTvName = @"",
			SourceTvPort = BinaryPrimitives.ReadInt16LittleEndian(RandomNumberGenerator.GetBytes(2)),
			Keywords = @"Kwt",
			GameID = BinaryPrimitives.ReadInt64LittleEndian(RandomNumberGenerator.GetBytes(8)),
		};

		ValueTask _ = server.StartAsync(default);

		await Parallel.ForAsync(0, 10000, async (i, token) =>
		{
			using GameServer client = new(IPAddress.Loopback, serverAddress.Port);

			SteamQueryInformation information = await client.GetInformationAsync(token);

			Assert.AreEqual(server.A2SInfo.Protocol, information.ProtocolVersion);
			Assert.AreEqual(server.A2SInfo.Name, information.ServerName);
			Assert.AreEqual(server.A2SInfo.Map, information.Map);
			Assert.AreEqual(server.A2SInfo.Folder, information.Folder);
			Assert.AreEqual(server.A2SInfo.Game, information.GameName);
			Assert.AreEqual(server.A2SInfo.Players, information.OnlinePlayers);
			Assert.AreEqual(server.A2SInfo.MaxPlayers, information.MaxPlayers);
			Assert.AreEqual(server.A2SInfo.Bots, information.Bots);
			Assert.AreEqual(SteamQueryServerType.Dedicated, information.ServerType);
			Assert.AreEqual(SteamQueryEnvironment.Linux, information.Environment);
			Assert.AreEqual(server.A2SInfo.Visibility, (A2SVisibility)Convert.ToByte(!information.Visible));
			Assert.AreEqual(server.A2SInfo.Vac, (A2SVacStatus)Convert.ToByte(information.VacSecured));
			Assert.AreEqual(server.A2SInfo.Version, information.Version);
			Assert.AreEqual(server.A2SInfo.Port, information.Port);
			Assert.AreEqual(server.A2SInfo.SteamID, (long?)information.SteamId);
			Assert.AreEqual(server.A2SInfo.SourceTvName, information.SourceTvName);
			Assert.AreEqual(server.A2SInfo.SourceTvPort, information.SourceTvPort);
			Assert.AreEqual(server.A2SInfo.Keywords, information.Keywords);
			Assert.AreEqual(server.A2SInfo.GameID, (long?)information.GameId);
		});
	}
}
