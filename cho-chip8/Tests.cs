using System.Diagnostics;
using NUnit.Framework;

namespace cho_chip8
{
    public class Tests
    {
        private Chip8 chip8;

        [SetUp]
        public void Setup()
        {
            chip8 = new Chip8();
        }

        [TestCase((byte)0xFF, (byte)0xAA, (ushort)0xFFAA)]
        [TestCase((byte)0xD9, (byte)0x2B, (ushort)0xD92B)]
        [TestCase((byte)0x1A, (byte)0x2B, (ushort)0x1A2B)]
        public void givenTwoHalves_WhenFetchOpcode_AssertOpcodeCorrect(byte p1, byte p2, ushort expected)
        {
            var result = chip8.MergeBytes(p1, p2);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((ushort)0xFFAA, (ushort)0xF000)]
        [TestCase((ushort)0xD92B, (ushort)0xD000)]
        [TestCase((ushort)0x1A2B, (ushort)0x1000)]
        public void givenOpcode_WhenDecodingFirstChar_AssertUShortIsExpected(ushort opcode, ushort expected)
        {
            var result = chip8.FirstChar(opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((ushort)0xFFAA, (ushort)0x0F00)]
        [TestCase((ushort)0xD92B, (ushort)0x0900)]
        [TestCase((ushort)0x1A2B, (ushort)0x0A00)]
        public void givenOpcode_WhenDecodingSecondChar_AssertUShortIsExpected(ushort opcode, ushort expected)
        {
            var result = chip8.SecondChar(opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((ushort)0xFFAA, (ushort)0x00A0)]
        [TestCase((ushort)0xD92B, (ushort)0x0020)]
        [TestCase((ushort)0x1A2C, (ushort)0x00C0)]
        public void givenOpcode_WhenDecodingThirdChar_AssertUShortIsExpected(ushort opcode, ushort expected)
        {
            var result = chip8.ThirdChar(opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((ushort)0xFFAA, (ushort)0x000A)]
        [TestCase((ushort)0xD92B, (ushort)0x000B)]
        [TestCase((ushort)0x1A2C, (ushort)0x000C)]
        public void givenOpcode_WhenDecodingLastChar_AssertUShortIsExpected(ushort opcode, ushort expected)
        {
            var result = chip8.FourthChar(opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((ushort)0xFFFF, (ushort)0x0FFF)]
        [TestCase((ushort)0xA05E, (ushort)0x005E)]
        [TestCase((ushort)0xD69A, (ushort)0x069A)]
        public void givenOpcode_WhenGetNnn_AssertCorrectValue(ushort opcode, ushort expected)
        {
            var result = chip8.Nnn(opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((ushort)0xFFFF, (ushort)0x00FF)]
        [TestCase((ushort)0xA05E, (ushort)0x005E)]
        [TestCase((ushort)0xD69A, (ushort)0x009A)]
        public void givenOpcode_WhenGetNn_AssertCorrectValue(ushort opcode, ushort expected)
        {
            var result = chip8.Nn(opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((ushort)0xF000, 3, (ushort)0x000F)]
        [TestCase((ushort)0x1F00, 2, (ushort)0x001F)]
        [TestCase((ushort)0x00F0, 1, (ushort)0x000F)]
        public void givenOpcode_WhenShiftRight_AssertCorrectValue(ushort opcode, int positions, ushort expected)
        {
            var result = chip8.ShiftRight(positions, opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((byte)0xF0, 0x0)]
        [TestCase((byte)0xF9, 0x9)]
        [TestCase((byte)0x1F, 0xF)]
        [TestCase((byte)0x0B, 0xB)]
        public void givenOpcode_WhenGetLSB_AssertCorrectValue(byte opcode, byte expected)
        {
            var result = chip8.GetLeastSignificantBit(opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((byte)0xF0, 0xF)]
        [TestCase((byte)0xF0, 0xF)]
        [TestCase((byte)0x01, 0x0)]
        [TestCase((byte)0xB0, 0xB)]
        public void givenOpcode_WhenGetMSB_AssertCorrectValue(byte opcode, byte expected)
        {
            var result = chip8.GetMostSignificantBit(opcode);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase((ushort)0xFFFF, "Unknown opcode: 0xFFFF")]
        public void givenOpcode_WhenGetError_AssertCorrectValue(ushort opcode, string expected)
        {
            var result = chip8.GetErrMessage(opcode);
            Assert.That(result, Is.EquivalentTo(expected));
        }
    }
}
