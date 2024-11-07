/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FirmwareFile
{
    /**
     * Represents a block of firmware corresponding to the data for a continuous block
     * of memory starting at a given address.
     */
    public class FirmwareBlock
    {
        /*===========================================================================
         *                           PUBLIC PROPERTIES
         *===========================================================================*/

        /**
         * Starting address of the firmware block.
         */
        public UInt32 StartAddress { get; private set; }

        /**
         * Data of the firmware block.
         */
        public byte[] Data { get => m_data.ToArray(); }

        /**
         * Size of the firmware block.
         */
        public UInt32 Size { get => (uint) m_data.Count /(uint)(BitWidth / 0x8); }

        /// <summary>
        /// Data bit width => how many bits of data does an address contain usually 8 bit (1 byte)
        /// </summary>
        public byte BitWidth { get; private set; }

        /*===========================================================================
         *                         INTERNAL CONSTRUCTORS
         *===========================================================================*/

        internal FirmwareBlock( UInt32 startAddress, byte[] data ,byte bitWidth = 8)
        {
            Debug.Assert(bitWidth % 8 == 0, "BitWidth: The data bit width can only be a multiple of 8 bits (1 byte)");
            StartAddress = startAddress;
            m_data = new List<byte>( data );
            BitWidth = bitWidth;
        }

        /*===========================================================================
         *                            INTERNAL METHODS
         *===========================================================================*/

        internal void SetDataAtAddress( UInt32 address, byte[] data )
        {
            int offset = ( ( (int) address ) - ( (int) StartAddress ) );

            SetDataAtOffset( offset, data );
        }

        internal void SetDataAtOffset( int offset, byte[] data )
        {
            int endOffset = offset + data.Length / (BitWidth / 0x8);

            if( ( offset >= 0 ) && ( offset <= Size ) )
            {
                if( endOffset < Size )
                {
                    m_data.RemoveRange( offset * (BitWidth / 0x8), endOffset == offset ? data.Length/ (BitWidth / 0x8): data.Length);
                }
                else
                {
                    m_data.RemoveRange( offset * (BitWidth / 0x8), (int) ( Size  - offset ) * (BitWidth / 0x8));
                }
                m_data.InsertRange( offset * (BitWidth / 0x8), data );
            }
            else if( ( offset < 0 ) && ( endOffset >= 0 ) )
            {
                StartAddress -= (uint) (-offset);

                if( endOffset >= Size )
                {
                    m_data.Clear();
                }
                else if( endOffset > 0 )
                {
                    m_data.RemoveRange( 0, endOffset * (BitWidth / 0x8));
                }
                m_data.InsertRange( 0, data );
            }
            else
            {
                throw new ArgumentException( "Inserted data region does not overlap the block data region" );
            }
        }

        internal void AppendData( byte[]? data )
        {
            if( data != null )
            {
                m_data.AddRange( data );
            }
        }

        internal void EraseDataRangeAfterAddress( UInt32 address )
        {
            uint offset = 0;
            if( address > StartAddress )
            {
                offset = ( address - StartAddress );
            }

            EraseDataRangeAfterOffset( offset );
        }

        internal void EraseDataRangeBeforeAddress( UInt32 address )
        {
            uint offset = 0;
            if( address > StartAddress )
            {
                offset = ( address - StartAddress );
            }

            EraseDataRangeBeforeOffset( offset );
        }

        internal void EraseDataRangeAfterOffset( uint offset )
        {
            if( offset < Size * BitWidth / 0x8)
            {
                m_data.RemoveRange( (int) offset , (int) ( Size * BitWidth / 0x8 - offset ) );
            }
        }

        internal void EraseDataRangeBeforeOffset( uint offset )
        {
            if( offset < Size )
            {
                m_data.RemoveRange( 0, (int) offset * BitWidth / 0x8);
                StartAddress += offset;
            }
            else
            {
                m_data.Clear();
            }
        }

        /*===========================================================================
         *                           PRIVATE ATTRIBUTES
         *===========================================================================*/

        private List<byte> m_data;
    }
}
