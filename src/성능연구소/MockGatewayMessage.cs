using System.IO.Compression;
using System.Text;

namespace 성능연구소;

public static class MockGatewayMessage
{
    public enum MessageType
    {
        GuildCreateLarge,
        GuildCreateSmall,
        GuildCreateLargeCompressed,
        GuildCreateSmallCompressed
    }

    private static readonly byte[][] GuildCreateLarge;
    private static readonly byte[][] GuildCreateSmall;
    private static readonly byte[][] GuildCreateLargeCompressed;
    private static readonly byte[][] GuildCreateSmallCompressed;

    private static MessageType _messageType;

    public static void SelectMessageType(MessageType messageType)
    {
        _messageType = messageType;
    }

    public static byte[][] Events
    {
        get
        {
            return _messageType switch
            {
                MessageType.GuildCreateLarge => GuildCreateLarge,
                MessageType.GuildCreateSmall => GuildCreateSmall,
                MessageType.GuildCreateLargeCompressed => GuildCreateLargeCompressed,
                MessageType.GuildCreateSmallCompressed => GuildCreateSmallCompressed,
                _ => throw new InvalidOperationException("Unsupported message type")
            };
        }
    }

    static MockGatewayMessage()
    {
        var hello = Encoding.UTF8.GetBytes(File.ReadAllText("hello.json"));
        var ready = Encoding.UTF8.GetBytes(File.ReadAllText("ready.json"));
        var guildCreateLarge = Encoding.UTF8.GetBytes(File.ReadAllText("guild_create.json"));
        var guildCreateSmall = Encoding.UTF8.GetBytes(File.ReadAllText("guild_create_small.json"));

        var ms = new MemoryStream();
        using var df1 = new DeflateStream(ms, CompressionMode.Compress);
        df1.Write(guildCreateLarge);
        var guildCreateLargeCompressed = ms.ToArray();

        ms = new MemoryStream();
        using var df2 = new DeflateStream(ms, CompressionMode.Compress);
        df2.Write(guildCreateSmall);
        var guildCreateSmallCompressed = ms.ToArray();

        GuildCreateLarge = new[] { hello, ready, guildCreateLarge };
        GuildCreateSmall = new[] { hello, ready, guildCreateSmall };
        
        AddZLibPadding(ref guildCreateLargeCompressed);
        AddZLibPadding(ref guildCreateSmallCompressed);
        
        GuildCreateLargeCompressed = new[] { hello, ready, guildCreateLargeCompressed };
        GuildCreateSmallCompressed = new[] { hello, ready, guildCreateSmallCompressed };
    }

    private static void AddZLibPadding(ref byte[] array)
    {
        Array.Resize(ref array, array.Length + 4);
        array[^1] = 0xFF;
        array[^2] = 0xFF;
        array[^3] = 0x0;
        array[^4] = 0x0;
    }
}