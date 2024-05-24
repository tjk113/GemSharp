namespace GemSharp {
    class Program {
        static void Main(string[] args) {
            /* Set the output encoding before passing
            the standard output stream to `printer` */
            Console.InputEncoding = System.Text.Encoding.Unicode;
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            Printer printer = new Printer(Console.OpenStandardOutput());
            //string url = args.Length > 0 ? args[0] : "gemini://geminiprotocol.net/";
            string url = args.Length > 0 ? args[0] : "gemini://senders.io/gemlog/depression.gmi";
            //string url = args.Length > 0 ? args[0] : "gemini://gemini.circumlunar.space/capcom/";
            //string url = args.Length > 0 ? args[0] : "gemini://senders.io/gemlog/comic-mono.gmi";

            GeminiResponse? response = Client.Request(url);
            printer.WriteLine(url, Printer.Format.Underline, Printer.Color.Green);
            if (response != null) {
                GeminiResponse _response = response.Value;
                printer.Write("Status: ");
                printer.WriteLine(_response.status.ToString(), Printer.Format.Bold,
                    (int)_response.status <= 11 ? Printer.Color.Purple
                    : (int)_response.status < 40 ? Printer.Color.Cyan
                    : Printer.Color.Red);
                printer.WriteLine();
                if ((int)_response.status <= 20) {
                    GemtextParser parser = new GemtextParser(_response.body);
                    parser.Print(ref printer);
                }
                else
                    printer.WriteLine(_response.metadata);
            }
            else
                printer.WriteLine($"{url} seems to be down :(");
        }
    }
}