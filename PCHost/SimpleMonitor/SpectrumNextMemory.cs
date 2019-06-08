using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMonitor
{
    class SpectrumNextMemory : MachineInfo
    {
        ulong baseAddress;
        byte[] bytes;
        public void Init(byte[] data, ulong address)
        {
            bytes = data;
            baseAddress = address;
        }

        public bool FetchByte(ulong address, out byte _b)
        {
            _b = 0xFF;
            if ((address >= baseAddress) && ((address - baseAddress) < (UInt64)bytes.Length))
            {
                _b = bytes[address - baseAddress];
                return true;
            }
            return false;
        }

        public bool FetchWord(ulong address, out ushort _w)
        {
            byte l;
            byte h;
            _w = 0xFFFF;
            if (FetchByte(address, out l) && FetchByte(address + 1, out h))
            {
                _w = l;
                _w |= ((UInt16)(h << 8));
                return true;
            }
            return false;
        }
    }
}
