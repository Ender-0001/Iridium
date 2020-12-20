using Iridium.Common.IO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Iridium
{
    public class Program
    {
        private const int HANDSHAKE_SIZE = 194;
        private const int COOKIE_SIZE = 20;

        private static Random _random;
        private static byte[] _cookie;

        private static UdpClient _server;

        private static void Main(string[] args)
        {
            _random = new Random();
            _cookie = new byte[COOKIE_SIZE];

            _random.NextBytes(_cookie);

            var local = new IPEndPoint(IPAddress.Any, 7777);

            _server = new UdpClient(local);

            var sender = new IPEndPoint(IPAddress.Any, 0);

            while (true)
            {
                var bytes    = _server.Receive(ref sender);
                var lastByte = bytes[^1];

                if (lastByte != 0)
                {
                    var bitSize = (bytes.Length * 8) - 1;

                    // Bit streaming, starts at the Least Significant Bit, and ends at the MSB.
                    while (!((lastByte & 0x80) >= 1))
                    {
                        lastByte *= 2;
                        bitSize--;
                    }

                    var reader = new BitReader(bytes, bitSize);

                    Received(reader, bytes, sender);
                }
            }
        }

        private static void Received(BitReader reader, byte[] bytes, IPEndPoint sender)
        {
            if (reader.ReadBit())
            {
                var bHandshakePacket = ParseHandshake(reader, out bool secretId, out float timeStamp, out byte[] cookie);
                if (bHandshakePacket)
                {
                    var bInitialConnect = timeStamp == 0f;
                    if (bInitialConnect) SendChallenge(sender);
                    else SendAck(sender, cookie); // Required because Fortnite (1.8) uses a mix of UE 4.15/16/17, just end my suffering already.
                }
            }
            else
            {
                // WriteBytes(bytes);

                var packetId = (int)reader.ReadSerialized(16384);

                while (!reader.IsEnd)
                {
                    var isAck = reader.ReadBit();
                    if (isAck)
                    {
                        var ackPacketId = (int)reader.ReadSerialized(16384);
                        if (ackPacketId == -1) return;

                        var bHasServerFrameTime = reader.ReadBit();

                        var remoteInKBytesPerSecond = (int)reader.ReadPacked();

                        continue;
                    }

                    // TODO(Cyuubi): Migrate this to the new Bunch class, when possible.
                    var bControl = reader.ReadBit();
                    var bOpen    = bControl ? reader.ReadBit() : false;
                    var bClose   = bControl ? reader.ReadBit() : false;
                    var bDormant = bClose ? reader.ReadBit() : false;

                    var bIsReplicationPaused = reader.ReadBit();

                    var bReliable = reader.ReadBit();

                    var chIndex = (int)reader.ReadSerialized(10240);
                    if (chIndex < 0 || chIndex >= 10240) continue;

                    var bHasPackageMapExports = reader.ReadBit();
                    var bHasMustBeMappedGUIDs = reader.ReadBit();

                    var bPartial = reader.ReadBit();

                    var chSequence
                        = bReliable ? (int)reader.ReadSerialized(1024)
                        : bPartial ? packetId : 0;

                    var bPartialInitial = bPartial ? reader.ReadBit() : false;
                    var bPartialFinal   = bPartial ? reader.ReadBit() : false;

                    var chType = (bReliable || bOpen) ? (int)reader.ReadSerialized(8) : 0;

                    var bunchDataBits = (int)reader.ReadSerialized(1024 * 8);
                    if (bunchDataBits > reader.Left) return;

                    switch (chType)
                    {
                        case 1:
                            {
                                Console.WriteLine("Received Control message.");

                                var type = reader.ReadByte();

                                switch (type)
                                {
                                    case 0:
                                        {
                                            var endian  = reader.ReadByte();
                                            var version = reader.ReadUInt32();

                                            Console.WriteLine("NMT_Hello received.");
                                            Console.WriteLine($"Endian = {endian}");
                                            Console.WriteLine($"Version = {version:X8}");
                                        }
                                        break;

                                    default: Console.WriteLine($"Unknown type ({type}) specified."); break;
                                }
                            }
                            break;

                        default: Console.WriteLine($"Unknown channel type ({chType}) specified."); break;
                    }
                }
            }
        }

        private static void WriteBytes(byte[] bytes)
        {
            var builder = new StringBuilder("byte[] { ");

            foreach (var b in bytes)
            {
                builder.Append($"0x{b:X2}, ");
            }

            builder.Append("};");

            Console.WriteLine(builder.ToString());
        }

        private static bool ParseHandshake(BitReader reader, out bool secretId, out float timeStamp, out byte[] cookie)
        {
            secretId  = false;
            timeStamp = 1f;
            cookie    = new byte[COOKIE_SIZE];

            if (reader.Left == HANDSHAKE_SIZE - 1)
            {
                secretId  = reader.ReadBit();
                timeStamp = reader.ReadFloat();
                cookie    = (byte[])reader.ReadBytes(COOKIE_SIZE);

                return true; // FIXME(Cyuubi): Doesn't confirm if it's 100% valid.
            }

            return false;
        }

        private static void SendChallenge(IPEndPoint sender)
        {
            var writer = new BitWriter(HANDSHAKE_SIZE + 1);

            writer.Write(true);
            writer.Write(false);

            writer.Write(1f);
            writer.Write(_cookie);

            CapHandshake(writer);

            var bytes = writer.ToArray();

            _server.Send(bytes, bytes.Length, sender);
        }

        private static void SendAck(IPEndPoint sender, byte[] cookie)
        {
            var writer = new BitWriter(HANDSHAKE_SIZE + 1);

            writer.Write(true);
            writer.Write(false);

            writer.Write(-1f);
            writer.Write(cookie);

            CapHandshake(writer);

            var bytes = writer.ToArray();

            _server.Send(bytes, bytes.Length, sender);
        }

        private static void CapHandshake(BitWriter writer)
        {
            writer.Write(true);
        }
    }
}
