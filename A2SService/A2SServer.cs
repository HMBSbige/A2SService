using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;

namespace A2SService;

/// <summary>
/// https://developer.valvesoftware.com/wiki/Server_queries
/// </summary>
public class A2SServer(EndPoint local) : IDisposable
{
	public const int MaxSize = 1400;
	private const int ChallengeResponseSize = 4 + 1 + 4;

	public UdpClient Server { get; } = new(local.AddressFamily);

	public A2SInfo A2SInfo { get; set; } = new();

	public TimeSpan ChallengeUpdateInterval { get; init; } = TimeSpan.FromSeconds(30);

	private int _challenge = -1;

	private IDisposable? _updateChallengeTask;

	private void UpdateChallenge()
	{
		int random = -1;
		while (random is -1)
		{
			random = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue) + 1;
		}
		Interlocked.Exchange(ref _challenge, random);
	}

	protected async ValueTask SendChallengeResponseAsync(IPEndPoint target, CancellationToken cancellationToken = default)
	{
		using IMemoryOwner<byte> memoryOwner = MemoryPool<byte>.Shared.Rent(ChallengeResponseSize);
		Memory<byte> memory = memoryOwner.Memory;

		BinaryPrimitives.WriteInt32LittleEndian(memory.Span.Slice(0, sizeof(int)), -1);
		memory.Span[4] = (byte)'A';
		BinaryPrimitives.WriteInt32LittleEndian(memory.Span.Slice(5, sizeof(int)), _challenge);

		await Server.SendAsync(memory.Slice(0, ChallengeResponseSize), target, cancellationToken);
	}

	public async ValueTask StartAsync(CancellationToken cancellationToken)
	{
		Server.Client.Bind(local);

		if (_updateChallengeTask is null)
		{
			UpdateChallenge();
			_updateChallengeTask = Observable.Interval(ChallengeUpdateInterval).Subscribe(_ => UpdateChallenge());
		}

		while (true)
		{
			try
			{
				UdpReceiveResult message = await Server.ReceiveAsync(cancellationToken);

				ValueTask _ = HandleAsync(message, cancellationToken);
			}
			catch (Exception) when (!cancellationToken.IsCancellationRequested)
			{

			}
		}
		// ReSharper disable once FunctionNeverReturns
	}

	protected virtual async ValueTask HandleAsync(UdpReceiveResult result, CancellationToken cancellationToken = default)
	{
		if (result.Buffer.Length < ChallengeResponseSize)
		{
			return;
		}

		if (BinaryPrimitives.ReadInt32LittleEndian(result.Buffer) is not -1)
		{
			return;
		}

		switch (result.Buffer[4])
		{
			case (byte)'T': // A2S_INFO
			{
				const string requestPayload = @"Source Engine Query";
				if (result.Buffer.Length < 4 + 1 + requestPayload.Length + 1)
				{
					return;
				}

				if (Encoding.UTF8.GetString(result.Buffer.AsSpan(4 + 1, requestPayload.Length)) is not requestPayload || result.Buffer[4 + 1 + requestPayload.Length] is not 0)
				{
					return;
				}

				if (result.Buffer.Length < 4 + 1 + requestPayload.Length + 1 + 4
					|| BinaryPrimitives.ReadInt32LittleEndian(result.Buffer.AsSpan(4 + 1 + requestPayload.Length + 1, 4)) != _challenge)
				{
					await SendChallengeResponseAsync(result.RemoteEndPoint, cancellationToken);
					return;
				}

				using IMemoryOwner<byte> memoryOwner = MemoryPool<byte>.Shared.Rent(MaxSize);
				Memory<byte> memory = memoryOwner.Memory.Slice(0, MaxSize);

				if (A2SInfo.TryWriteToSimpleResponse(memory.Span, out int bytesWritten))
				{
					await Server.SendAsync(memory.Slice(0, bytesWritten), result.RemoteEndPoint, cancellationToken);
				}

				return;
			}
			default:
			{
				return;
			}
		}
	}

	public void Dispose()
	{
		_updateChallengeTask?.Dispose();
		Server.Dispose();

		GC.SuppressFinalize(this);
	}
}
