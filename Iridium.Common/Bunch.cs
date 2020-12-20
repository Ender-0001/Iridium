using Iridium.Common.IO;

namespace Iridium.Common
{
    public class Bunch
    {
        // Required for de/serializing bunches.
        // These values usually vary per-game.
        private const int MAX_CHANNELS = 10240;
        private const int MAX_CHSEQUENCE = 1024;
        private const int MAX_CHTYPE = 8;

        public int PacketId;

        public bool bControl;
        public bool bOpen;
        public bool bClose;
        public bool bDormant;

        public bool bIsReplicationPaused;

        public bool bReliable;

        public int ChIndex;

        public bool bHasPackageMapExports;
        public bool bHasMustBeMappedGUIDs;

        public bool bPartial;

        public int ChSequence;

        public bool bPartialInitial;
        public bool bPartialFinal;

        public int ChType;

        public Bunch(int packetId = 0)
        {
            PacketId = packetId;
        }

        public void Read(BitReader reader)
        {
            bControl = reader.ReadBit();
            bOpen = bControl ? reader.ReadBit() : false;
            bClose = bControl ? reader.ReadBit() : false;
            bDormant = bClose ? reader.ReadBit() : false;

            bIsReplicationPaused = reader.ReadBit();

            bReliable = reader.ReadBit();

            ChIndex = (int)reader.ReadSerialized(MAX_CHANNELS);

            bHasPackageMapExports = reader.ReadBit();
            bHasMustBeMappedGUIDs = reader.ReadBit();

            bPartial = reader.ReadBit();

            ChSequence
                = bReliable ? (int)reader.ReadSerialized(MAX_CHSEQUENCE)
                : bPartial ? PacketId : 0;

            bPartialInitial = bPartial ? reader.ReadBit() : false;
            bPartialFinal = bPartial ? reader.ReadBit() : false;

            ChType = (bReliable || bOpen) ? (int)reader.ReadSerialized(MAX_CHTYPE) : 0;
        }

        public void Write(BitWriter writer)
        {
            writer.Write(bControl);

            if (bControl)
            {
                writer.Write(bOpen);
                writer.Write(bClose);

                if (bClose) writer.Write(bDormant);
            }

            writer.Write(bIsReplicationPaused);

            writer.Write(bReliable);

            writer.Write((uint)ChIndex, MAX_CHANNELS);

            writer.Write(bHasPackageMapExports);
            writer.Write(bHasMustBeMappedGUIDs);

            if (bReliable) writer.Write((uint)ChSequence, MAX_CHSEQUENCE);

            if (bPartial)
            {
                writer.Write(bPartialInitial);
                writer.Write(bPartialFinal);
            }

            if (bReliable || bOpen) writer.Write((uint)ChType, MAX_CHTYPE);
        }
    }
}
