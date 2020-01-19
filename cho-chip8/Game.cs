namespace cho_chip8
{
    public class Game
    {
        private Chip8 chip8;

        // Stores current state of key press.


        public Game()
        {
            chip8 = new Chip8();
        }

        /// <summary>
        ///
        ///        The systems memory map:
        ///
        ///    0x000-0x1FF - Chip 8 interpreter (contains font set in emu)
        ///    0x050-0x0A0 - Used for the built in 4x5 pixel font set (0-F)
        ///    0x200-0xFFF - Program ROM and work RAM
        ///
        ///
        /// </summary>
        public void Run()
        {
            chip8.Cycle();
        }
    }
}
