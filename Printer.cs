using System.Runtime.InteropServices;
using System.Text;

namespace GemSharp {
    internal class Printer {
        public enum Format {
            Normal    = 0,
            Bold      = 1,
            Underline = 4
        }
 
        public enum Color {
            Gray    = 29, // Not a "valid" color
            Black   = 30,
            Red     = 31,
            Green   = 32,
            Yellow  = 33,
            Blue    = 34,
            Purple  = 35,
            Cyan    = 36,
            White   = 37,
            Default = 39
        }

        private Stream stream;
        private static readonly string startSequence = "\x1b[";
        private static readonly string resetSequence = "\x1b[0m";

        // https://stackoverflow.com/a/77603420
        private static void EnableEscapeSequences() {
            // P/Invoke declarations
            const int STD_OUTPUT_HANDLE = -11;
            const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

            [DllImport("kernel32.dll", SetLastError = true)]
            static extern IntPtr GetStdHandle(int nStdHandle);

            [DllImport("kernel32.dll")]
            static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

            [DllImport("kernel32.dll")]
            static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

            // Get the handle to the standard output stream
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);

            // Get the current console mode
            uint mode;
            if (!GetConsoleMode(handle, out mode))
            {
                Console.Error.WriteLine("Failed to get console mode");
                return;
            }

            // Enable the virtual terminal processing mode
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            if (!SetConsoleMode(handle, mode))
            {
                Console.Error.WriteLine("Failed to set console mode");
                return;
            }
        }

        public Printer(Stream stream) {
            this.stream = stream;
            EnableEscapeSequences();
        }

        public void Write(string text, Format format = Format.Normal, Color color = Color.Default, Color backgroundColor = Color.Default) {
            int parsedColor = color == Color.Gray ? 90 : (int)color;
            string sequence = ((int)format).ToString() + ';'
                              + parsedColor.ToString() + ';'
                              + ((int)backgroundColor + 10).ToString() + 'm';
            // `text` has to be UTF-16, as it was converted from UTF-8 right after the bytes were read
            this.stream.Write(Encoding.Unicode.GetBytes(Printer.startSequence + sequence + text + Printer.resetSequence));
        }

        public void WriteLine(string text = "", Format format = Format.Normal, Color color = Color.Default, Color backgroundColor = Color.Default) {
            /* If we did the logical thing and just passed `text + "\n"` to
            `Write`, then the formatting would stretch all the way across the
            width of the line, which is a problem if using `Format.Underline`.
            So we're doing it like this instead! */
            Write(text, format, color, backgroundColor);
            Write("\n");
        }
    }
}