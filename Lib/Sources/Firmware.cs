/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

#nullable enable

namespace FirmwareFile
{
    /**
     * Represent the firmware for a device a set of one or more blocks of memory.
     */
    public class Firmware
    {
        /*===========================================================================
         *                           PUBLIC PROPERTIES
         *===========================================================================*/

        /**
         * Indicates if the blocks have explicit addresses (e.g., when loaded from an HEX file),
         * or if it has implicit / unknown addresses (e.g., when loaded from a binary file).
         */
        public bool HasExplicitAddresses { get; private set; }

        /**
         * Array of FirmwareBlocks that conform the firmware.
         */
        public FirmwareBlock[] Blocks { get => m_blocks.ToArray(); }

        /*===========================================================================
         *                            PUBLIC METHODS
         *===========================================================================*/

        /// <summary>
        /// 从最早开始的块，没有数据会进行 filldata 填充，直到最后地址 => 从最低的地址开始线性增加
        /// </summary>
        /// <param name="filldata">需要填充的数据，对于非一个字节对齐的地址的块会按照一个地址进行重复</param>
        /// <returns>所有的块，块间不连续的地址用filldata填充</returns>
        public List<FirmwareBlock> GetFillData(byte filldata)
        {
            List<FirmwareBlock> ret = new List<FirmwareBlock>();
            for (int i = 0; i < Blocks.Length - 1; i++)
            {
                var CurentBlock = Blocks[i];
                var LastBlock = Blocks[i + 1];

                var CurentEndaddress = CurentBlock.StartAddress + CurentBlock.Size;
                var LastCuernBlocksEndaddress = LastBlock.StartAddress;

                var AddressDiff = LastCuernBlocksEndaddress - CurentEndaddress;

                if (AddressDiff != 0)
                {
                    byte[] NewData = new byte[CurentBlock.Data.Length + AddressDiff * BitWidth / 8];
                    for (int j = 0; j < NewData.Length; j++)
                    {
                        NewData[j] = filldata;
                    }

                    Array.Copy(CurentBlock.Data, NewData, CurentBlock.Data.Length);

                    ret.Add(new FirmwareBlock(CurentBlock.StartAddress, NewData,BitWidth));
                }

            }

            ret.Add(new FirmwareBlock(Blocks[Blocks.Length - 1].StartAddress, Blocks[Blocks.Length - 1].Data, BitWidth));
            return ret;
        }

        public struct FirmwareLineBlock
        {
            public uint StartAddress;
            public byte[] Data;
        }

        /// <summary>
        /// 得到固定隔间的线性块，每个块之间的大小固定，且从最低的地址开始线性增加
        /// </summary>
        /// <param name="Splitsize">每个块之间的大小,按照字节来计算</param>
        /// <param name="filldata">填充的数据</param>
        /// <returns></returns>
        public List<FirmwareLineBlock> GetSplitLineBlock(uint Splitsize, byte filldata)
        {
            List<FirmwareLineBlock> ret = new List<FirmwareLineBlock>();

            var Fillblocks = GetFillData(filldata);

            var AllCountByte = Fillblocks.Sum(s => s.Data.Length);
            byte[] LineBlockeData = new byte[AllCountByte];
            int currenLen = 0;
            foreach (var array in Fillblocks)
            {
                Array.Copy(array.Data, 0, LineBlockeData, currenLen, array.Data.Length);
                currenLen += array.Data.Length;
            }

            SplitData(Splitsize, filldata, ret, Fillblocks[0].StartAddress, LineBlockeData);

            return ret;
        }

        /// <summary>
        /// 将数据以固定的长度分割开
        /// </summary>
        /// <param name="Splitsize">固定的长度</param>
        /// <param name="filldata">当数据长度不足时，默认填入的数据</param>
        /// <param name="ret">返回的集合</param>
        /// <param name="Startadress">输入的线性起始地址</param>
        /// <param name="LineBlockeData">输入的线性数据</param>
        private static void SplitData(uint Splitsize, byte filldata, List<FirmwareLineBlock> ret, uint Startadress, byte[] LineBlockeData)
        {
            uint groupSize = Splitsize; // 每组的大小  
            uint totalGroups = ((uint)LineBlockeData.Length + groupSize - 1) / groupSize; // 计算总组数  

            for (uint i = 0; i < totalGroups; i++)
            {
                uint currentGroupSize = Math.Min(groupSize, (uint)LineBlockeData.Length - i * groupSize);

                FirmwareLineBlock temp = new FirmwareLineBlock();

                byte[] group = new byte[groupSize]; //定义固定大小的组  
                temp.Data = group;
                temp.StartAddress = Startadress + i * groupSize;

                // 复制当前组中的数据  
                Array.Copy(LineBlockeData, i * groupSize, group, 0, currentGroupSize);

                // 如果当前组不足 1024 字节，填充 0xFF  
                if (currentGroupSize < groupSize)
                {
                    for (uint j = currentGroupSize; j < groupSize; j++)
                    {
                        group[j] = filldata;
                    }
                }
                ret.Add(temp);
            }
        }

        /**
* Writes a data block at the given address, overwriting any previously set data if necessary.
* 
* FirmwareBlocks are automatically created, modified or merged as necessary as a result of inserting the data block.
* 
* @param [in] startAddress Starting address for the data block
* @param [in] data Data for the data block
*/
        public void SetData( UInt32 startAddress, byte[] data )
        {
            if( data.Length == 0 )
            {
                return;
            }
            if (data.Length % ( BitWidth / 0x8) != 0)
            {
                throw new ArgumentException("Bit width and data are not aligned");
            }


            UInt32 endAddress = startAddress + (uint) data.Length / (uint)(BitWidth / 0x8);
            FirmwareBlock? startBlock = null;
            FirmwareBlock? endBlock = null;

            RemoveOverwrittenBlocks( startAddress, endAddress );

            foreach( var block in m_blocks )
            {
                if( ( block.StartAddress <= startAddress ) && ( ( block.StartAddress + block.Size ) >= startAddress ) )
                {
                    if( startBlock != null )
                    {
                        throw new Exception( "INTERNAL ERROR: Blocks are overlapping" );
                    }
                    startBlock = block;
                }

                if( ( block.StartAddress <= endAddress ) && ( ( block.StartAddress + block.Size ) >= endAddress ) )
                {
                    if( endBlock != null )
                    {
                        throw new Exception( "INTERNAL ERROR: Blocks are overlapping" );
                    }
                    endBlock = block;
                }
            }

            if( endBlock == startBlock )
            {
                endBlock = null;
            }

            if( ( startBlock != null ) && ( endBlock != null ) )
            {
                // Data overlaps partially 2 blocks => Expand start block on the tail and merge end block

                startBlock.SetDataAtOffset( (int) ( startAddress - startBlock.StartAddress ), data );

                int endBlockOffset = (int) ( endAddress - endBlock.StartAddress );
                int tailSize = (int) ( endBlock.Size - endBlockOffset) * BitWidth / 0x8;
                var tailData = new byte[tailSize];
                Array.Copy( endBlock.Data, endBlockOffset * BitWidth / 0x8, tailData, 0, tailSize );
                startBlock.AppendData( tailData );

                m_blocks.Remove( endBlock );
            }
            else if( ( startBlock != null ) && ( endBlock == null ) )
            {
                // Data overlaps partially a block on its middle or tail => Expand start block on the middle or tail

                startBlock.SetDataAtOffset( (int) ( startAddress - startBlock.StartAddress ), data );
            }
            else if( ( startBlock == null ) && ( endBlock != null ) )
            {
                // Data overlaps partially a block on its head => Expand end block on the head

                endBlock.SetDataAtOffset( - (int) ( endBlock.StartAddress - startAddress ), data );
            }
            else
            {
                // Data does not overlap any other block => Create new block

                m_blocks.Add( new FirmwareBlock( startAddress, data, BitWidth) );
            }
        }

        /**
         * Erases a data block from the given address.
         * 
         * FirmwareBlocks are automatically deleted, modified or split as necessary as a result of erasing the data block.
         * 
         * @param [in] startAddress Starting address for the data block
         * @param [in] size Size of the data block
         */
        public void EraseData( UInt32 startAddress, uint size )
        {
            if( size == 0 )
            {
                return;
            }
            //if (size % (BitWidth / 0x8) != 0)
            //{
            //    throw new ArgumentException("Bit width and data are not aligned");
            //}

            UInt32 endAddress = startAddress + size;

            foreach( var block in m_blocks.ToArray() )
            {
                uint blockStartAddress = block.StartAddress;
                uint blockEndAddress = block.StartAddress + block.Size;

                if( ( blockStartAddress < startAddress ) && ( blockEndAddress > endAddress ) )
                {
                    // Region overlaps the middle of a block => Split block

                    int endBlockOffset = (int) ( endAddress - blockStartAddress );
                    int tailSize = (int) ( block.Size - endBlockOffset) * BitWidth / 0x8;
                    var tailData = new byte[tailSize];
                    Array.Copy( block.Data, endBlockOffset * BitWidth / 0x8, tailData, 0, tailSize );

                    m_blocks.Add( new FirmwareBlock( endAddress, tailData ) );

                    uint startBlockOffset = ( startAddress - blockStartAddress );

                    block.EraseDataRangeAfterOffset( startBlockOffset );
                }
                else if( ( blockStartAddress >= startAddress ) && ( blockEndAddress <= endAddress ) )
                {
                    // Region fully overlaps a block => Remove block

                    m_blocks.Remove( block );
                }
                else if( ( blockStartAddress >= startAddress ) && ( blockStartAddress < endAddress ) )
                {
                    // Region overlaps the head of a block => Remove from head

                    uint blockOffset = ( endAddress - blockStartAddress );

                    block.EraseDataRangeBeforeOffset( blockOffset );
                }
                else if( ( blockEndAddress > startAddress ) && ( blockEndAddress <= endAddress ) )
                {
                    // Region overlaps the tail of a block => Remove from tail

                    uint blockOffset = ( startAddress - blockStartAddress );

                    block.EraseDataRangeAfterOffset( blockOffset );
                }
            }
        }

        /**
         * Gets a data block from the given address.
         * 
         * @param [in] startAddress Starting address for the data block
         * @param [in] size Size of the data block
         * 
         * @return Array filled with the contents of the firmware at the requested data block memory region,
         *         or @c null if the requested data block memory region is not completely defined (i.e., if
         *         it doesn't fully overlap a single FirmwareBlock)
         */
        public byte[]? GetData( UInt32 startAddress, uint size )
        {
            UInt32 endAddress = startAddress + size;

            foreach( var block in m_blocks )
            {
                uint blockStartAddress = block.StartAddress;
                uint blockEndAddress = block.StartAddress + block.Size;

                if( ( blockStartAddress <= startAddress ) && ( blockEndAddress >= endAddress ) )
                {
                    var returnData = new byte[size * BitWidth / 0x8];
                    Array.Copy( block.Data, (int) ( startAddress - blockStartAddress ) * BitWidth / 0x8, returnData, 0, size * BitWidth / 0x8);
                    return returnData;
                }
            }

            return null;
        }

        /// <summary>
        /// Data bit width => how many bits of data does an address contain usually 8 bit (1 byte)
        /// </summary>
        public byte BitWidth { get; private set; }

        /*===========================================================================
         *                          INTERNAL CONSTRUCTORS
         *===========================================================================*/

        internal Firmware( bool hasExplicitAddresses , byte bitWidth = 8)
        {
            Debug.Assert(bitWidth % 8 == 0, "BitWidth: The data bit width can only be a multiple of 8 bits (1 byte)");
            HasExplicitAddresses = hasExplicitAddresses;
            BitWidth = bitWidth;
        }

        /*===========================================================================
         *                            PRIVATE METHODS
         *===========================================================================*/

        private void RemoveOverwrittenBlocks( UInt32 startAddress, UInt32 endAddress )
        {
            foreach( var block in m_blocks.ToArray() )
            {
                if( ( block.StartAddress >= startAddress ) && ( ( block.StartAddress + block.Size ) <= endAddress ) )
                {
                    m_blocks.Remove( block );
                }
            }
        }

        /*===========================================================================
         *                           PRIVATE ATTRIBUTES
         *===========================================================================*/

        private List<FirmwareBlock> m_blocks = new List<FirmwareBlock>();
    }
}
