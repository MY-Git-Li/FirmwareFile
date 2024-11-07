/**
 * @file
 * @copyright  Copyright (c) 2020 Jesús González del Río
 * @license    See LICENSE.txt
 */

using System;
using System.Linq;
using Xunit;

namespace FirmwareFile.Test
{
    public class FirmwareTest
    {
        [Fact]
        public void SetData_Empty()
        {
            // Prepare

            var firmware = new Firmware( false );
            var data = new byte[] {};
            UInt32 address = 0x1000;

            // Execute

            firmware.SetData( address, data );

            // Check

            Assert.False( firmware.HasExplicitAddresses );
            Assert.Empty( firmware.Blocks );
        }

        [Fact]
        public void SetData_Empty_16()
        {
            // Prepare

            var firmware = new Firmware(false,16);
            var data = new byte[] { };
            UInt32 address = 0x1000;

            // Execute

            firmware.SetData(address, data);

            // Check

            Assert.False(firmware.HasExplicitAddresses);
            Assert.Empty(firmware.Blocks);
        }

        [Fact]
        public void SetData_Single()
        {
            // Prepare

            var firmware = new Firmware( false );
            var data = new byte[] { 1, 2, 45, 3 };
            UInt32 address = 0x1000;

            // Execute

            firmware.SetData( address, data );

            // Check

            Assert.False( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address, firmware.Blocks[0].StartAddress );
            Assert.Equal( data, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_fault_16width()
        {
            // Prepare

            var firmware = new Firmware(false,16);
            var data = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address = 0x1000;

            // Execute

            var ex = Assert.Throws<ArgumentException>(() => firmware.SetData(address, data));

            // Check

            Assert.Contains("Bit width and data are not aligned", ex.Message);
           
        }

        [Fact]
        public void SetData_Single_16witdh()
        {
            // Prepare

            var firmware = new Firmware(false,16);
            var data = new byte[] { 1, 2, 45, 3, 255,255 };
            UInt32 address = 0x1000;

            // Execute

            firmware.SetData(address, data);

            // Check

            Assert.False(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address, firmware.Blocks[0].StartAddress);
            Assert.Equal(data, firmware.Blocks[0].Data);
        }

        [Fact]
        public void SetData_Double_NonOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void SetData_Double_NonOverlap_16width()
        {
            // Prepare

            var firmware = new Firmware(true,16);

            var data1 = new byte[] { 1, 2, 45, 3, 255,255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            // Execute

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }

        [Fact]
        public void SetData_Double_TailOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1002;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3, 145, 32, 0, 99 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_TailOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);

            var data1 = new byte[] { 1, 2, 45, 3 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1001;

            // Execute

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 1, 2, 45, 3, 145, 32, 0, 99 }, firmware.Blocks[0].Data);
        }


        [Fact]
        public void SetData_Double_TailJoin()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1005;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3, 255, 45, 3, 145, 32, 0, 99 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_TailJoin_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);

            var data1 = new byte[] { 1, 2, 45, 3 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x1002;

            // Execute

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 1, 2, 45, 3, 45, 3, 145, 32, 0, 99 }, firmware.Blocks[0].Data);
        }

        [Fact]
        public void SetData_Double_HeadOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x0FFC;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address2, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 45, 3, 145, 32, 0, 99, 45, 3, 255 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_HeadOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);

            var data1 = new byte[] { 1, 2, 45, 3 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x0FFE;

            // Execute

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address2, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 45, 3, 145, 32, 0, 99, 45, 3 }, firmware.Blocks[0].Data);
        }

        [Fact]
        public void SetData_Double_HeadJoin()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x0FFA;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address2, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 45, 3, 145, 32, 0, 99, 1, 2, 45, 3, 255 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void SetData_Double_HeadJoin_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);

            var data1 = new byte[] { 1, 2, 45, 3 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99 };
            UInt32 address2 = 0x0FFD;

            // Execute

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address2, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 45, 3, 145, 32, 0, 99, 1, 2, 45, 3}, firmware.Blocks[0].Data);
        }

        [Fact]
        public void SetData_Double_FullOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1002;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99, 88, 12 };
            UInt32 address2 = 0x1000;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address2, firmware.Blocks[0].StartAddress );
            Assert.Equal( data2, firmware.Blocks[0].Data );
        }


        [Fact]
        public void SetData_Double_FullOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);

            var data1 = new byte[] { 1, 2, 45, 3 };
            UInt32 address1 = 0x1002;

            var data2 = new byte[] { 45, 3, 145, 32, 0, 99, 88, 12 };
            UInt32 address2 = 0x1000;

            // Execute

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address2, firmware.Blocks[0].StartAddress);
            Assert.Equal(data2, firmware.Blocks[0].Data);
        }

        [Fact]
        public void SetData_Double_NestedOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255, 0, 99, 88, 12 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32  };
            UInt32 address2 = 0x1003;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 45, 3, 145, 32, 88, 12 }, firmware.Blocks[0].Data );
        }


        [Fact]
        public void SetData_Double_NestedOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);

            var data1 = new byte[] { 1, 2, 45, 3, 255, 0, 99, 88 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 45, 3, 145, 32 };
            UInt32 address2 = 0x1001;

            // Execute

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 1, 2, 45, 3, 145, 32, 99, 88 }, firmware.Blocks[0].Data);
        }

        [Fact]
        public void SetData_Triple_Overlap()
        {
            // Prepare

            var firmware = new Firmware( true );

            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1008;

            var data3 = new byte[] { 23, 34, 1, 44, 88, 12, 77 };
            UInt32 address3 = 0x1004;

            // Execute

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );
            firmware.SetData( address3, data3 );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3, 23, 34, 1, 44, 88, 12, 77, 32, 0, 99 }, firmware.Blocks[0].Data );
        }


        [Fact]
        public void SetData_Triple_Overlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);

            var data1 = new byte[] { 1, 2, 45, 3 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1004;

            var data3 = new byte[] { 23, 34, 1, 44, 88, 12, 55,77};
            UInt32 address3 = 0x1001;

            // Execute

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);
            firmware.SetData(address3, data3);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 1, 2, 23, 34,  1, 44, 88, 12, 55, 77, 148, 32, 0, 99 }, firmware.Blocks[0].Data);
        }

        [Fact]
        public void EraseData_Empty()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1002;
            uint removeSize = 0;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_Empty_16width()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3};
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x1002;
            uint removeSize = 0;

            firmware.EraseData(address3, removeSize);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }


        [Fact]
        public void EraseData_NoOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255};
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1006;
            uint removeSize = 5;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }


        [Fact]
        public void EraseData_NoOverlap_16width()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x1006;
            uint removeSize = 3;

            firmware.EraseData(address3, removeSize);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }

        [Fact]
        public void EraseData_TailOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1003;
            uint removeSize = 5;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45 }, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_TailOverlap_16width()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3 ,56, 78};
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x1002;
            uint removeSize = 6;

            firmware.EraseData(address3, removeSize);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 1, 2 ,45, 3}, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }

        [Fact]
        public void EraseData_HeadOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x0FFE;
            uint removeSize = 5;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address3 + removeSize, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 3, 255 }, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_HeadOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3, 255,11 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x0FFE;
            uint removeSize = 3;

            firmware.EraseData(address3, removeSize);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address3 + removeSize, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 45, 3, 255, 11 }, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }

        [Fact]
        public void EraseData_DoubleOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1002;
            uint removeSize = 0x11;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2 }, firmware.Blocks[0].Data );
            Assert.Equal( address3 + removeSize, firmware.Blocks[1].StartAddress );
            Assert.Equal( new byte[] { 32, 0, 99 }, firmware.Blocks[1].Data );
        }

        [Fact]
        public void EraseData_DoubleOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3, 255, 77 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x1001;
            uint removeSize = 0x11;

            firmware.EraseData(address3, removeSize);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 1, 2 }, firmware.Blocks[0].Data);
            Assert.Equal(address3 + removeSize, firmware.Blocks[1].StartAddress);
            Assert.Equal(new byte[] { 0, 99 }, firmware.Blocks[1].Data);
        }

        [Fact]
        public void EraseData_FullOverlap_Single()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x0FFD;
            uint removeSize = 0x10;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address2, firmware.Blocks[0].StartAddress );
            Assert.Equal( data2, firmware.Blocks[0].Data );
        }


        [Fact]
        public void EraseData_FullOverlap_Double()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x0FFD;
            uint removeSize = 0x20;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Empty( firmware.Blocks );
        }

       
        [Fact]
        public void EraseData_PartialAndFullOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1004;
            uint removeSize = 0x20;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Single( firmware.Blocks );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( new byte[] { 1, 2, 45, 3 }, firmware.Blocks[0].Data );
        }

        [Fact]
        public void EraseData_PartialAndFullOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3, 255,77 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x1002;
            uint removeSize = 0x12;

            firmware.EraseData(address3, removeSize);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Single(firmware.Blocks);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(new byte[] { 1, 2, 45, 3 }, firmware.Blocks[0].Data);
        }


        [Fact]
        public void EraseData_MiddleOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1012;
            uint removeSize = 2;

            firmware.EraseData( address3, removeSize );

            // Check

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 3, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( new byte[] { 179, 7 }, firmware.Blocks[1].Data );
            Assert.Equal( address3 + removeSize, firmware.Blocks[2].StartAddress );
            Assert.Equal( new byte[] { 0, 99 }, firmware.Blocks[2].Data );
        }

        [Fact]
        public void EraseData_MiddleOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true, 16);
            var data1 = new byte[] { 1, 2, 45, 3, 255 ,77};
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x1011;
            uint removeSize = 1;

            firmware.EraseData(address3, removeSize);

            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(3, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(new byte[] { 179, 7 }, firmware.Blocks[1].Data);
            Assert.Equal(address3 + removeSize, firmware.Blocks[2].StartAddress);
            Assert.Equal(new byte[] { 0, 99 }, firmware.Blocks[2].Data);
        }

        [Fact]
        public void GetData_MiddleOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x1001;
            uint getSize = 3;

            var data3 = firmware.GetData( address3, getSize );

            // Check

            Assert.Equal( new byte[] { 2, 45, 3 }, data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }
        [Fact]
        public void GetData_MiddleOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3, 255,77 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x1001;
            uint getSize = 2;

            var data3 = firmware.GetData(address3, getSize);

            // Check

            Assert.Equal(new byte[] { 45, 3, 255, 77 }, data3);

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }

        [Fact]
        public void GetData_FullOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            var data3 = firmware.GetData( address2, (uint) data2.Length );

            // Check

            Assert.Equal( data2, data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void GetData_FullOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3, 255,77 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            var data3 = firmware.GetData(address2, (uint)data2.Length/2);

            // Check

            Assert.Equal(data2, data3);

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }

        [Fact]
        public void GetData_PartialHeadOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 0x0FFE;
            uint getSize = 4;

            var data3 = firmware.GetData( address3, getSize );

            // Check

            Assert.Null( data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void GetData_PartialHeadOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3, 255,77 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 0x0FFE;
            uint getSize = 4;

            var data3 = firmware.GetData(address3, getSize);

            // Check

            Assert.Null(data3);

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }


        [Fact]
        public void GetData_PartialTailOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 1013;
            uint getSize = 4;

            var data3 = firmware.GetData( address3, getSize );

            // Check

            Assert.Null( data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }

        [Fact]
        public void GetData_PartialTailOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3, 255,77 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 1013;
            uint getSize = 4;

            var data3 = firmware.GetData(address3, getSize);

            // Check

            Assert.Null(data3);

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }

        [Fact]
        public void GetData_NoOverlap()
        {
            // Prepare

            var firmware = new Firmware( true );
            var data1 = new byte[] { 1, 2, 45, 3, 255 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData( address1, data1 );
            firmware.SetData( address2, data2 );

            // Execute

            UInt32 address3 = 1005;
            uint getSize = 11;

            var data3 = firmware.GetData( address3, getSize );

            // Check

            Assert.Null( data3 );

            Assert.True( firmware.HasExplicitAddresses );
            Assert.Equal( 2, firmware.Blocks.Length );
            Assert.Equal( address1, firmware.Blocks[0].StartAddress );
            Assert.Equal( data1, firmware.Blocks[0].Data );
            Assert.Equal( address2, firmware.Blocks[1].StartAddress );
            Assert.Equal( data2, firmware.Blocks[1].Data );
        }


        [Fact]
        public void GetData_NoOverlap_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 45, 3, 255 ,16};
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            UInt32 address3 = 1005;
            uint getSize = 11;

            var data3 = firmware.GetData(address3, getSize);

            // Check

            Assert.Null(data3);

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(2, firmware.Blocks.Length);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);
        }

        [Fact]
        public void GetFillData()
        {
            // Prepare

            var firmware = new Firmware(true);
            var data1 = new byte[] { 1, 2, 0x45, 3, 0x1, 0x16 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1010;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

         
            var data3 = firmware.GetFillData(0xff);


            var CheckData = new byte[] { 
                1, 2, 0x45, 3, 0x1, 0x16 ,0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            };

            var CheckData2 = new byte[] {
               179, 7, 148, 32, 0, 99
            };
            // Check

            Assert.True(firmware.HasExplicitAddresses);
          
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);

            Assert.Equal(CheckData, data3[0].Data);
            Assert.Equal(CheckData2, data3[1].Data);
        }

        [Fact]
        public void GetFillData_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 0x45, 3, 0x1, 0x16 };
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1005;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute


            var data3 = firmware.GetFillData(0xff);


            var CheckData = new byte[] {
                1, 2, 0x45, 3, 0x1, 0x16 ,0xff, 0xff, 0xff, 0xff,
            };

            var CheckData2 = new byte[] {
               179, 7, 148, 32, 0, 99
            };
            // Check

            Assert.True(firmware.HasExplicitAddresses);
          
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);

            Assert.Equal(CheckData, data3[0].Data);
            Assert.Equal(CheckData2, data3[1].Data);
        }


        [Fact]
        public void GetSplitLineBlock()
        {
            // Prepare

            var firmware = new Firmware(true);
            var data1 = new byte[] { 1, 2, 0x45, 3, 0x1, 0x16 }; 
            UInt32 address1 = 0x1000;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1008;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute


            var data = firmware.GetSplitLineBlock(4,0xff);


            var CheckData = new byte[] {
                1, 2, 0x45, 3
            };
            var CheckData2 = new byte[] {
                0x1, 0x16 ,0xff, 0xff
            };

            var CheckData4 = new byte[] {
                 179, 7,148,32
            };
            var CheckData5 = new byte[] {
                0, 99, 0xff, 0xff
            };
            // Check

            Assert.True(firmware.HasExplicitAddresses);
         
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);

            Assert.Equal(CheckData, data[0].Data);
            Assert.Equal(CheckData2, data[1].Data);
            Assert.Equal(CheckData4, data[2].Data);
            Assert.Equal(CheckData5, data[3].Data);
        }


        [Fact]
        public void GetSplitLineBlock_16()
        {
            // Prepare

            var firmware = new Firmware(true,16);
            var data1 = new byte[] { 1, 2, 0x45, 3, 0x1, 0x16}; 
            UInt32 address1 = 0x1001;

            var data2 = new byte[] { 179, 7, 148, 32, 0, 99 };
            UInt32 address2 = 0x1005;

            firmware.SetData(address1, data1);
            firmware.SetData(address2, data2);

            // Execute

            var data = firmware.GetSplitLineBlock(4, 0xff);


            var CheckData = new byte[] {
                1, 2, 0x45, 3
            };
            var CheckData2 = new byte[] {
                0x1, 0x16 ,0xff, 0xff
            };

            var CheckData4 = new byte[] {
                 179, 7,148,32
            };
            var CheckData5 = new byte[] {
                0, 99, 0xff, 0xff
            };
            // Check

            Assert.True(firmware.HasExplicitAddresses);
            Assert.Equal(address1, firmware.Blocks[0].StartAddress);
            Assert.Equal(data1, firmware.Blocks[0].Data);
            Assert.Equal(address2, firmware.Blocks[1].StartAddress);
            Assert.Equal(data2, firmware.Blocks[1].Data);

            Assert.Equal(CheckData, data[0].Data);
            Assert.Equal(CheckData2, data[1].Data);
            Assert.Equal(CheckData4, data[2].Data);
            Assert.Equal(CheckData5, data[3].Data);
        }
    }
}
