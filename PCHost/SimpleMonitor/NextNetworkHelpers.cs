using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMonitor
{
    class NextNetworkHelpers
    {
        public static UInt16[] GetNextState(NetworkStream stream)
        {
            // grab registers
            stream.WriteByte(8);
            UInt16[] regs = new UInt16[14];     // AF BC DE HL SP PC IX IY AF' BC' DE' HL' IR IFF2
            for (int a = 0; a < 14; a++)
            {
                UInt16 t = 0;
                byte b = (byte)stream.ReadByte();
                t = (byte)stream.ReadByte();
                t <<= 8;
                t |= b;
                regs[a] = t;
            }
            return regs;
        }

        public static void SetNextState(NetworkStream stream, UInt16 [] regs)
        {
            stream.WriteByte(9);
            for (int a = 0; a < regs.Length; a++)
            {
                stream.WriteByte((byte)(regs[a] & 0xFF));
                stream.WriteByte((byte)((regs[a] >> 8) & 0xFF));
            }
        }

        public static byte[] GetCurrentBanks(NetworkStream stream)
        {
            // Get Current Memory Banks
            byte[] banks = new byte[8];
            for (int a = 0; a < 8; a++)
            {
                stream.WriteByte(4);
                stream.WriteByte((byte)(0x50 + a));
                banks[a] = (byte)stream.ReadByte();
            }
            return banks;
        }

        public static byte[] GetData(NetworkStream stream, byte bank, UInt16 offset, int length)
        {
            byte[] data = new byte[length];
            int position = 0;

            // Copes with any reasonable length
            while (length > 0)
            {
                int sLength = Math.Min(length, 8192-offset);
                Console.WriteLine($"{length}:{sLength}");

                stream.WriteByte(2);    // 2 recieve binary data
                stream.WriteByte(bank);    // Bank
                stream.WriteByte((byte)((offset) & 255));
                stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address

                stream.WriteByte((byte)((sLength) & 255));
                stream.WriteByte((byte)(((sLength) >> 8) & 255)); // size

                int bytesRead = 0;
                length -= sLength;
                while (sLength > 0)
                {
                    bytesRead = stream.Read(data, position, sLength);
                    Console.WriteLine(bytesRead);
                    sLength -= bytesRead;
                    position += bytesRead;
                }

                offset = 0;
            }

            return data;
        }

        public static void SetData(NetworkStream stream, byte bank, UInt16 offset, byte[] data)
        {
            int length = data.Length;
            int position = 0;

            // Copes with any reasonable length
            while (length>0)
            {
                int sLength = Math.Min(length, 8192 - offset);

                stream.WriteByte(1);    // 1 Sending binary data
                stream.WriteByte(bank);    // Bank
                stream.WriteByte((byte)((offset) & 255));
                stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address

                stream.WriteByte((byte)((length) & 255));
                stream.WriteByte((byte)(((length) >> 8) & 255)); // size
                stream.Write(data, position, sLength);

                length -= sLength;
                offset = 0;
                position += sLength;
            }
        }

        public static void SendResume(NetworkStream stream)
        {
            stream.WriteByte(7);       // resume
        }

        public static void SetBreakpoint(NetworkStream stream, int breakpointNumber, byte bank, UInt16 offset)
        {
            stream.WriteByte(5);
            stream.WriteByte(bank);   // bank
            stream.WriteByte((byte)((offset) & 255));
            stream.WriteByte((byte)(((offset) >> 8) & 255)); // Address
            stream.WriteByte((byte)(breakpointNumber & 63)); // bp num
        }

        public static void SetNextRegister(NetworkStream stream, byte register,byte value)
        {
            stream.WriteByte(3);
            stream.WriteByte(register);
            stream.WriteByte(value);
        }

        public static void SendExecute(NetworkStream stream, UInt16 address)
        {
            stream.WriteByte(6);
            stream.WriteByte((byte)((address) & 255));
            stream.WriteByte((byte)(((address) >> 8) & 255)); // Address
        }
    }
}
