namespace A2SService;

[Flags]
public enum ExtraDataFlag : byte
{
	None = 0x00,
	Port = 0x80,
	SteamID = 0x10,
	SourceTv = 0x40,
	Keywords = 0x20,
	GameID = 0x01
}
