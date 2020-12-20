using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Iridium.Common.IO
{
    public class BitWriter
    {
        #region Field Region

        public int Position;

        #endregion

        #region Property Region

        public BitArray Bits { get; private set; }

        #endregion

        #region Constructor Region

        public BitWriter(int length)
            => Bits = new BitArray(length);

        #endregion

        #region Method Region

        public void Write(bool value)
        {
            while (Position >= Bits.Length)
            {
                if (Bits.Length == 0) Bits.Length = 1024;
                else Bits.Length = Bits.Length * 2;
            }

            Bits[Position++] = value;
        }

        public void Write(byte value)
        {
            foreach (var b in new BitArray(new byte[] { value }).Cast<bool>()) Write(b);
        }

        public void Write(IEnumerable<byte> bytes)
        {
            foreach (var value in bytes) Write(value);
        }

        public void Write(short value)
            => Write(BitConverter.GetBytes(value));
        public void Write(ushort value)
            => Write(BitConverter.GetBytes(value));

        public void Write(int value)
            => Write(BitConverter.GetBytes(value));
        public void Write(uint value)
            => Write(BitConverter.GetBytes(value));

        public void Write(long value)
            => Write(BitConverter.GetBytes(value));
        public void Write(ulong value)
            => Write(BitConverter.GetBytes(value));

        public void Write(ulong value, ulong max)
        {
            var result = 0UL;

            for (var mask = 1UL; (result + mask) < max; mask *= 2)
            {
                if ((value & mask) != 0)
                {
                    Write(true);

                    result |= mask;
                }
                else Write(false);
            }
        }

        public void Write(float value)
            => Write(BitConverter.GetBytes(value));
        public void Write(double value)
            => Write(BitConverter.GetBytes(value));

        public byte[] ToArray()
        {
            var result = new byte[(Bits.Length - 1) / 8 + 1];

            Bits.CopyTo(result, 0);

            return result;
        }

        #endregion
    }
}
