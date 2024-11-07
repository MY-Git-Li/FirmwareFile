using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmwareFile.Sources
{
    public class FirmwareLineBlock
    {
        public byte[]? Data;
        public uint StartAddress;
        public uint Crc32;
    }
}
