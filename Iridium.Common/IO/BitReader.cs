using System;
using System.Collections;
using System.Collections.Generic;

namespace Iridium.Common.IO
{
    public class BitReader
    {
        #region Field Region

        public int Position;

        #endregion

        #region Property Region

        public BitArray Bits { get; private set; }

        public int Left => Bits.Length - Position;

        public bool IsEnd => Position >= Bits.Length;

        #endregion

        #region Constructor Region

        public BitReader(bool[] values)
            => Bits = new BitArray(values);

        public BitReader(bool[] values, int count)
        {
            Bits        = new BitArray(values);
            Bits.Length = count;
        }

        public BitReader(byte[] bytes)
            => Bits = new BitArray(bytes);

        public BitReader(byte[] bytes, int count)
        {
            Bits        = new BitArray(bytes);
            Bits.Length = count;
        }

        #endregion

        #region Method Region

        public bool ReadBit()
        {
            if (IsEnd) return false;

            return Bits[Position++];
        }

        public bool PeekBit()
        {
            var result = ReadBit();

            Position -= 1;

            return result;
        }

        public IEnumerable<bool> ReadBits(int count)
        {
            var result = new bool[count];

            for (var i = 0; i < count; i++) result[i] = ReadBit();

            return result;
        }

        public byte ReadByte()
        {
            byte result = 0;

            for (var i = 0; i < 8; i++)
            {
                if (ReadBit()) result |= (byte)(1 << i);
            }

            return result;
        }

        public byte PeekByte()
        {
            var result = ReadByte();

            Position -= 8;

            return result;
        }

        public IEnumerable<byte> ReadBytes(int count)
        {
            var result = new byte[count];

            for (int i = 0; i < count; i++) result[i] = ReadByte();

            return result;
        }

        public short ReadInt16()
            => BitConverter.ToInt16((byte[])ReadBytes(2), 0);
        public ushort ReadUInt16()
            => BitConverter.ToUInt16((byte[])ReadBytes(2), 0);

        public int ReadInt32()
            => BitConverter.ToInt32((byte[])ReadBytes(4), 0);
        public uint ReadUInt32()
            => BitConverter.ToUInt32((byte[])ReadBytes(4), 0);

        public long ReadInt64()
            => BitConverter.ToInt64((byte[])ReadBytes(8), 0);
        public ulong ReadUInt64()
            => BitConverter.ToUInt64((byte[])ReadBytes(8), 0);

        public ulong ReadSerialized(ulong max)
        {
            var result = 0UL;

            for (var mask = 1UL; (result + mask) < max; mask *= 2)
            {
                if (ReadBit()) result |= mask;
            }

            return result;
        }

        public ulong ReadPacked()
        {
            var result = 0UL;

            var bitsUsed = Position % 8;
            var bitsLeft = 8 - bitsUsed;

            var sourceMask0 = (1 << bitsLeft) - 1;
            var sourceMask1 = (1 << bitsUsed) - 1;

            var oldPosition = Position;

            for (int i = 0, shift = 0; i < 5; i++)
            {
                var bytePosition    = Position / 8;
                var alignedPosition = bytePosition * 8;

                Position = alignedPosition;

                var currentByte = ReadByte();
                var nextByte    = currentByte;

                if (bitsUsed != 0) nextByte = (Position + 8 <= Bits.Length) ? PeekByte() : (byte)0;

                oldPosition += 8;

                var readByte = ((currentByte >> bitsUsed) & sourceMask0) | ((nextByte & sourceMask1) << (bitsLeft & 7));

                result = (uint)((readByte >> 1) << shift) | result;

                if ((readByte & 1) == 0) break;

                shift += 7;
            }

            Position = oldPosition;

            return result;
        }

        public float ReadFloat()
            => BitConverter.ToSingle((byte[])ReadBytes(4), 0);
        public double ReadDouble()
            => BitConverter.ToDouble((byte[])ReadBytes(8), 0);

        #endregion
    }
}
