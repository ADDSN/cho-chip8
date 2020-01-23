using System;
using System.Collections.Generic;
using System.Linq;

namespace cho_chip8
{
    public class Chip8
    {
        private ushort opcode;

        private readonly byte[] memory = new byte[4096];

        // The Chip 8 has 15 8-bit general purpose registers named V0,V1 up to VE.
        // The 16th register is used  for the ‘carry flag’. Eight bits is one byte so we can use an unsigned char for this purpose
        private readonly byte[] vRegisters = new byte[16];

        // Can have a value from 0x000 to 0xFFF.
        private ushort index;
        private ushort programCounter;

        // Stack is used to remember current location before a jump is performed.
        // When jumping, store program counter in stack.
        private readonly Stack<ushort> stack = new Stack<ushort>(16);

        private byte delayTimer;
        private byte soundTimer;

        private byte[] key = new byte[16];

        private readonly byte[] fontSet = {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        public void Init()
        {

        }

        public void Cycle()
        {
            // Fetch opcode
            opcode = MergeBytes(memory[programCounter], memory[programCounter + 1]);

            switch(FirstChar(opcode))
            {
                case 0x0000:
                    switch (FourthChar(opcode))
                    {
                        case 0x0000: // 00E0
                            // Clears the screen.
                            break;
                        case 0x000E: // 00EE
                            // Returns from subroutine.
                            // . Set the PC to the address at the top of the stack.
                            programCounter = stack.Peek();
                            break;
                        default:
                            Console.WriteLine(GetErrMessage);
                            break;
                    }
                    break;
                case 0x1000:
                    JumpToNnn();
                    break;
                case 0x2000:
                    ExecuteSubroutine();
                    break;
                case 0x3000:
                    SkipIfEqual();
                    break;
                case 0x4000:
                    SkipIfNotEqual();
                    break;
                case 0x5000:
                    SkipIfVxEqualVy();
                    break;
                case 0x6000:
                    VxToNn();
                    break;
                case 0x7000:
                    AddNnToVx();
                    break;
                case 0x8000:
                    switch (FourthChar(opcode))
                    {
                        case 0x0000:
                            VxToVy();
                            break;
                        case 0x0001:
                            VxToVxOrVy();
                            break;
                        case 0x0002:
                            VxToVxAndVy();
                            break;
                        case 0x0003:
                            VxToVxXorVy();
                            break;
                        case 0x0004:
                            AddVyToVxWithCarry();
                            break;
                        case 0x0005:
                            SubtractVyFromVxWithBorrow();
                            break;
                        case 0x0006:
                            StoreLsb();
                            break;
                        case 0x0007:
                            VxToVyMinusVx();
                            break;
                        case 0x000E:
                            // Stores the most significant bit of VX in VF and then shifts VX to the left by 1.
                            StoreMsb();
                            break;
                        default:
                            Console.WriteLine(GetErrMessage);
                            break;
                    }
                    break;

                case 0x9000:
                    SkipIfVxNotEqualVy();
                    break;

                case 0xA000:
                    SaveNnnToIndex();
                    break;

                case 0xB000:
                    JumpToNnnV0();
                    break;

                case 0xC000:
                    SetVxWithRand();
                    break;

                case 0xD000:
                    DrawSpriteVxVyN();
                    break;

                case 0xE000:
                    switch (FourthChar(opcode))
                    {
                        case 0x000E:
                            SkipIfKeyPress();
                            break;
                        case 0x0001:
                            SkipIfKeyNotPress();
                            break;
                        default:
                            Console.WriteLine(GetErrMessage);
                            break;
                    }
                    break;

                case 0xF000:
                    switch (FourthChar(opcode))
                    {
                        case 0x0007:
                            SetVxToDelay();
                            break;
                        case 0x000A:
                            GetKey();
                            break;
                        case 0x0005:
                            switch (ThirdChar(opcode))
                            {
                                case 0x0010: // FX15
                                    SetDelay();
                                    break;
                                case 0x0050: // FX55
                                    SaveVToMemory();
                                    break;
                                case 0x0060: // FX65
                                    DumpVFromMemory();
                                    break;
                            }
                            break;
                        case 0x0008: // FX18
                            SetSound();
                            break;
                        case 0x000E: // FX1E
                            AddVxToIndexWithOverflow();
                            break;
                        case 0x0009: // FX29
                            SetIndexToVxSprite();
                            break;
                        case 0x0003: // FX33
                            StoreBinDecVx();
                            break;
                        default:
                            Console.WriteLine(GetErrMessage);
                            break;
                    }
                    break;

                default:
                    Console.WriteLine(GetErrMessage);
                    break;
            }

            // Update timers
            if(delayTimer > 0)
                --delayTimer;

            if(soundTimer > 0)
            {
                if(soundTimer == 1)
                    Console.WriteLine("BEEP!\n");
                --soundTimer;
            }
        }

        private void DumpVFromMemory() // FX65
        {
            byte indexOffset = 0;
            var _index = index;
            // Fills V0 to VX (including VX) with values from memory starting at address I.
            // The offset from I is increased by 1 for each value written, but I itself is left unmodified
            for (int i = 0; i <= vRegisters[ShiftRight(2, SecondChar(opcode))]; i++)
            {
                vRegisters[i] = memory[_index];
                ++indexOffset;
                _index += indexOffset;
            }
            programCounter += 2;
        }

        private void SaveVToMemory() // FX55
        {
            byte indexOffset = 0;
            var _index = index;
            // Stores V0 to VX (including VX) in memory starting at address I.
            // The offset from I is increased by 1 for each value written, but I itself is left unmodified
            for (int i = 0; i <= vRegisters[ShiftRight(2, SecondChar(opcode))]; i++)
            {
                memory[_index] = vRegisters[i];
                ++indexOffset;
                _index += indexOffset;
            }
            programCounter += 2;
        }

        private void StoreBinDecVx() // FX33
        {
            memory[index]     = (byte) (vRegisters[ShiftRight(2, SecondChar(opcode))] / 100);
            memory[index + 1] = (byte) ((vRegisters[ShiftRight(2, SecondChar(opcode))] / 10) % 10);
            memory[index + 2] = (byte) ((vRegisters[ShiftRight(2, SecondChar(opcode))] % 100) % 10);
            programCounter += 2;
        }

        private void SetIndexToVxSprite() // FX29
        {
            // TODO Confirm getting font.
            //Sets I to the location of the sprite for the character in VX.
            //Characters 0-F (in hexadecimal) are represented by a 4x5 font.
            for (int i = 0; i < fontSet.Count(); i++)
            {
                if (fontSet[i] == vRegisters[ShiftRight(2, SecondChar(opcode))])
                {
                    index = (ushort) i;
                }
            }
            programCounter += 2;
        }

        private void AddVxToIndexWithOverflow() // FX1E
        {
            vRegisters[0xF] = (byte) (index + vRegisters[ShiftRight(2, SecondChar(opcode))] > 0xFFF ? 1 : 0);
            index += vRegisters[ShiftRight(2, SecondChar(opcode))];
            programCounter += 2;
        }

        private void GetKey() // FX0A
        {
            // TODO implement getting input.
            var keyPress = 0xff;
            vRegisters[ShiftRight(2, SecondChar(opcode))] = (byte) keyPress;
            programCounter += 2;
        }

        private void SetDelay() // FX15
        {
            delayTimer = vRegisters[ShiftRight(2, SecondChar(opcode))];
            programCounter += 2;
        }

        private void SetSound() // FX18
        {
            soundTimer = vRegisters[ShiftRight(2, SecondChar(opcode))];
            programCounter += 2;
        }

        private void SetVxToDelay() // FX07
        {
            vRegisters[ShiftRight(2, SecondChar(opcode))] = delayTimer;
            programCounter += 2;
        }

        private void SkipIfKeyPress()  // 0xEX9E  TODO Confirm: Assumption that key = 1 is pressed.
        {
            programCounter = key[vRegisters[ShiftRight(2, SecondChar(opcode))]] == 1 ? programCounter += 4 : programCounter += 2;
        }

        private void SkipIfKeyNotPress()  // 0xEXA1
        {
            programCounter = key[vRegisters[ShiftRight(2, SecondChar(opcode))]] != 1 ? programCounter += 4 : programCounter += 2;
        }

        /// <summary>
        /// Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels.
        /// Each row of 8 pixels is read as bit-coded starting from memory location I;
        /// I value doesn’t change after the execution of this instruction.
        /// As described above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0 if that doesn’t happen
        /// </summary>
        private void DrawSpriteVxVyN() // 0xDXYN
        {
            // TODO Implement draw function
            var coordX = vRegisters[ShiftRight(2, SecondChar(opcode))];
            var coordY = vRegisters[ShiftRight(1, ThirdChar(opcode))];
            var heightN = FourthChar(opcode);
            programCounter += 2;

            // Swap screen bits base on sprite

            // DrawSpriteCallback?.Invoke(screen bits);
        }

        // private event Action DrawSpriteCallback;
        //
        // public void setDrawCallback(Action callback)
        // {
        //     DrawSpriteCallback += callback;
        // }

        private void SetVxWithRand() // 0xCXNN : Sets Vx to the result of rand.(0 - 255) AND nn
        {
            vRegisters[ShiftRight(2, SecondChar(opcode))] = (byte) (Nn(opcode) & randomNum());
            programCounter += 2;
        }

        private void StoreMsb()  // 0x8XYE : Store msb of VX in VF, shift VX to left by 1.
        {
            vRegisters[0xF] = GetMostSignificantBit(vRegisters[ShiftRight(2, SecondChar(opcode))]);

            vRegisters[ShiftRight(2, SecondChar(opcode))] = (byte) (vRegisters[ShiftRight(2, SecondChar(opcode))] << 1);
            programCounter += 2;
        }

        private void VxToVyMinusVx() // 0x8XY7 : Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
        {
            vRegisters[0xF] = (byte) ( vRegisters[ShiftRight(1, ThirdChar(opcode))] - vRegisters[ShiftRight(2, SecondChar(opcode))] < 0x00 ?
                0 : 1);

            vRegisters[ShiftRight(2, SecondChar(opcode))] =
                (byte) (vRegisters[ShiftRight(1, ThirdChar(opcode))] - vRegisters[ShiftRight(2, SecondChar(opcode))]);
            programCounter += 2;
        }

        private void StoreLsb() // 0x8XY6 : Stores the least significant bit of VX in VF (carry flag). Shifts VX to the right by 1.
        {
            vRegisters[0xF] = GetLeastSignificantBit(vRegisters[ShiftRight(2, SecondChar(opcode))]);

            vRegisters[ShiftRight(2, SecondChar(opcode))] = (byte) (vRegisters[ShiftRight(2, SecondChar(opcode))] >> 1);
            programCounter += 2;
        }

        private void VxToVxOrVy() // 0x8XY1 : Sets VX to VX OR VY
        {
            vRegisters[ShiftRight(2, SecondChar(opcode))] =
                (byte) (vRegisters[ShiftRight(2, SecondChar(opcode))] | vRegisters[ShiftRight(1, ThirdChar(opcode))]);
            programCounter += 2;
        }

        private void VxToVxAndVy() // 0x8XY2 : Sets VX to VX AND VY
        {
            vRegisters[ShiftRight(2, SecondChar(opcode))] =
                (byte) (vRegisters[ShiftRight(2, SecondChar(opcode))] & vRegisters[ShiftRight(1, ThirdChar(opcode))]);
            programCounter += 2;
        }

        private void VxToVxXorVy() // 0x8XY2 : Sets VX to VX XOR VY
        {
            vRegisters[ShiftRight(2, SecondChar(opcode))] =
                (byte) (vRegisters[ShiftRight(2, SecondChar(opcode))] ^ vRegisters[ShiftRight(1, ThirdChar(opcode))]);
            programCounter += 2;
        }

        private void AddVyToVxWithCarry() // 0x8XY4
        {
            vRegisters[0xF] = (byte) (vRegisters[ShiftRight(2, SecondChar(opcode))] + vRegisters[ShiftRight(1, ThirdChar(opcode))] > 0xFF ?
                1 : 0);
            vRegisters[ShiftRight(2, SecondChar(opcode))] += vRegisters[ShiftRight(1, ThirdChar(opcode))];
            programCounter += 2;
        }

        private void SubtractVyFromVxWithBorrow() // 0x8XY5
        {
            vRegisters[0xF] = (byte) (vRegisters[ShiftRight(2, SecondChar(opcode))] - vRegisters[ShiftRight(1, ThirdChar(opcode))] < 0x00 ?
                0 : 1);
            vRegisters[ShiftRight(2, SecondChar(opcode))] -= vRegisters[ShiftRight(1, ThirdChar(opcode))];
            programCounter += 2;
        }

        private void VxToVy() // 0x8XY0 : Sets VX to the value of VY.
        {
            vRegisters[ShiftRight(2, SecondChar(opcode))] = vRegisters[ShiftRight(1, ThirdChar(opcode))];
            programCounter += 2;
        }

        private void AddNnToVx() // 0x7XNN
        {
            vRegisters[ShiftRight(2, SecondChar(opcode))] += Nn(opcode);
            programCounter += 2;
        }

        private void VxToNn() // 0x6XNN
        {
            vRegisters[ShiftRight(2, SecondChar(opcode))] = Nn(opcode);
            programCounter += 2;
        }

        private void SkipIfVxEqualVy() //0x5XY0
        {
            programCounter = vRegisters[ShiftRight(2, SecondChar(opcode))] == vRegisters[ShiftRight(1, ThirdChar(opcode))]
                ? programCounter +=4 :programCounter +=2;
        }

        private void SkipIfVxNotEqualVy() // 0x9XY0
        {
            programCounter = vRegisters[ShiftRight(2, SecondChar(opcode))] != vRegisters[ShiftRight(1, ThirdChar(opcode))]
                ? programCounter +=4 :programCounter +=2;
        }

        private void SaveNnnToIndex() // 0xANNN
        {
            index = Nnn(opcode);
            programCounter += 2;
        }

        private void JumpToNnnV0() // 0xBNNN
        {
            stack.Push(programCounter);
            programCounter = (byte) (Nnn(opcode) + vRegisters[0]);
        }

        private void JumpToNnn() // 0x1NNN
        {
//            stack.Push(programCounter);
            // Jumps to address NNN
            // TODO Confirm how jumping works.
            programCounter = Nnn(opcode);
        }

        private void ExecuteSubroutine() // 0x2NNN
        {
            stack.Push(programCounter);
            // TODO Confirm how calling subroutine works.
            programCounter = Nnn(opcode);
        }

        private void SkipIfEqual() // 0x3XNN
        {
            programCounter = vRegisters[Nn(opcode)] == Nn(opcode) ?
                programCounter +=4 :programCounter +=2;
        }

        private void SkipIfNotEqual() // 0x4XNN
        {
            programCounter = vRegisters[Nn(opcode)] != Nn(opcode) ?
                programCounter +=4 :programCounter +=2;
        }

        // Shifts character the num positions to right.
        public readonly Func<int,ushort, ushort> ShiftRight = (positions, opcode) => (ushort) (opcode >> (positions * 4));

        // To merge into a ushort, use bitwise OR.
        public readonly Func<byte, byte, ushort> MergeBytes = (p1, p2) => (ushort) ((p1 << 8) | p2);

        // Decode opcode - get first nibble.
        public readonly Func<ushort, ushort> FirstChar = (opcode) => (ushort) (opcode & 0xF000);

        // Decode opcode - get second nibble.
        public readonly Func<ushort, ushort> SecondChar = (opcode) => (ushort) (opcode & 0x0F00);

        // Decode opcode - get third nibble.
        public readonly Func<ushort, ushort> ThirdChar = (opcode) => (ushort) (opcode & 0x00F0);

        // Decode opcode - get last nibble.
        public readonly Func<ushort, ushort> FourthChar = (opcode) => (ushort) (opcode & 0x000F);

        // 0xA2F0 & 0x0FFF = 0x02F0 (takes last 3)
        public readonly Func<ushort, ushort> Nnn = (opcode) => (ushort) (opcode & 0x0FFF);

        // 0xA2F0 & 0x00FF = 0x00F0 (takes last 2)
        public readonly Func<ushort, byte> Nn = (opcode) => (byte) (opcode & 0x00FF);

        // TODO Confirm LSB / MSB
        // Returns 1 / 0
        public readonly Func<byte, byte> GetLeastSignificantBit = (vx) => (byte) (vx & 0x01);

        // Returns 1 / 0
        public readonly Func<byte, byte> GetMostSignificantBit = (vx) => (byte) (vx >> 7);

        private readonly Func<int> randomNum = () => new Random().Next(0, 255);

        public readonly Func<ushort, string> GetErrMessage = (opcode) => $"Unknown opcode: 0x{opcode:X}";

        public void LoadGame()
        {

        }
    }
}
