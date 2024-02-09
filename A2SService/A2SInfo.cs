using System.Buffers.Binary;
using System.Text;

namespace A2SService;

public record A2SInfo
{
	public const byte Header = (byte)'I';

	public byte Protocol { get; set; }

	public string? Name { get; set; }

	public string? Map { get; set; }

	public string? Folder { get; set; }

	public string? Game { get; set; }

	public short ID { get; set; }

	public byte Players { get; set; }

	public byte MaxPlayers { get; set; }

	public byte Bots { get; set; }

	public A2SType ServerType { get; set; } = A2SType.Dedicated;

	public A2SEnvironment Environment { get; set; }

	public A2SVisibility Visibility { get; set; }

	public A2SVacStatus Vac { get; set; }

	public string? Version { get; set; }

	#region Extra Data

	public short? Port { get; set; }

	public long? SteamID { get; set; }

	public short? SourceTvPort { get; set; }

	public string? SourceTvName { get; set; }

	public string? Keywords { get; set; }

	public long? GameID { get; set; }

	#endregion

	public A2SInfo()
	{
		if (OperatingSystem.IsWindows())
		{
			Environment = A2SEnvironment.Windows;
		}
		else if (OperatingSystem.IsMacCatalyst() || OperatingSystem.IsMacOS())
		{
			Environment = A2SEnvironment.MacNg;
		}
		else
		{
			Environment = A2SEnvironment.Linux;
		}
	}

	public bool TryWriteToSimpleResponse(in Span<byte> buffer, out int bytesWritten)
	{
		bytesWritten = 0;

		if (buffer.Length < 20)
		{
			return false;
		}

		BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(0, 4), -1);
		buffer[4] = Header;
		buffer[5] = Protocol;
		bytesWritten = 6;

		if (!TryWriteString(Name, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteString(Map, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteString(Folder, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteString(Game, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!BinaryPrimitives.TryWriteInt16LittleEndian(buffer.Slice(bytesWritten), ID))
		{
			return false;
		}
		bytesWritten += sizeof(short);

		if (!TryWriteByte(Players, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteByte(MaxPlayers, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteByte(Bots, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteByte((byte)ServerType, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteByte((byte)Environment, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteByte((byte)Visibility, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteByte((byte)Vac, buffer, ref bytesWritten))
		{
			return false;
		}

		if (!TryWriteString(Version, buffer, ref bytesWritten))
		{
			return false;
		}

		if (Port is null && SteamID is null && SourceTvPort is null && Keywords is null && GameID is null)
		{
			return true;
		}

		ExtraDataFlag flag = ExtraDataFlag.None;
		if (!TryWriteByte((byte)flag, buffer, ref bytesWritten))
		{
			return false;
		}

		ref byte flagBuffer = ref buffer.Slice(bytesWritten - 1)[0];

		if (Port.HasValue)
		{
			flag |= ExtraDataFlag.Port;

			if (!BinaryPrimitives.TryWriteInt16LittleEndian(buffer.Slice(bytesWritten), Port.Value))
			{
				return false;
			}
			bytesWritten += sizeof(short);
		}

		if (SteamID.HasValue)
		{
			flag |= ExtraDataFlag.SteamID;

			if (!BinaryPrimitives.TryWriteInt64LittleEndian(buffer.Slice(bytesWritten), SteamID.Value))
			{
				return false;
			}
			bytesWritten += sizeof(long);
		}

		if (SourceTvPort.HasValue)
		{
			flag |= ExtraDataFlag.SourceTv;

			if (!BinaryPrimitives.TryWriteInt16LittleEndian(buffer.Slice(bytesWritten), SourceTvPort.Value))
			{
				return false;
			}
			bytesWritten += sizeof(short);

			if (!TryWriteString(SourceTvName, buffer, ref bytesWritten))
			{
				return false;
			}
		}

		if (Keywords is not null)
		{
			flag |= ExtraDataFlag.Keywords;

			if (!TryWriteString(Keywords, buffer, ref bytesWritten))
			{
				return false;
			}
		}

		if (GameID.HasValue)
		{
			flag |= ExtraDataFlag.GameID;

			if (!BinaryPrimitives.TryWriteInt64LittleEndian(buffer.Slice(bytesWritten), GameID.Value))
			{
				return false;
			}
			bytesWritten += sizeof(long);
		}

		flagBuffer = (byte)flag;
		return true;

		bool TryWriteByte(in byte b, in Span<byte> buff, ref int bytesWritten)
		{
			Span<byte> span = buff.Slice(bytesWritten);
			if (span.IsEmpty)
			{
				return false;
			}

			span[0] = b;
			++bytesWritten;

			return true;
		}

		bool TryWriteString(in ReadOnlySpan<char> str, in Span<byte> buff, ref int bytesWritten)
		{
			Span<byte> span = buff.Slice(bytesWritten);
			if (!Encoding.UTF8.TryGetBytes(str, span, out int count))
			{
				return false;
			}

			bytesWritten += count;

			return TryWriteByte(0, buff, ref bytesWritten);
		}
	}
}
