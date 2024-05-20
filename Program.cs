namespace GemSharp {
    class Program {
        static void Main(string[] args) {
            Printer printer = new Printer(Console.OpenStandardOutput());
            string url = args.Length > 0 ? args[0] : "gemini://geminiprotocol.net/";

            GeminiResponse? response = Client.Request(url);
            printer.WriteLine(url, Printer.Format.Underline, Printer.Color.Green);
            if (response != null) {
                GeminiResponse _response = response.Value;
                Console.Write("Status: ");
                printer.WriteLine(_response.status.ToString(), Printer.Format.Bold,
                    (int)_response.status <= 11 ? Printer.Color.Purple
                    : (int)_response.status < 40 ? Printer.Color.Cyan
                    : Printer.Color.Red);
                Console.WriteLine();
                if ((int)_response.status <= 20) {
                    GemtextParser parser = new GemtextParser(_response.body);
                    parser.Print(ref printer);
                }
                else
                    Console.WriteLine(_response.metadata);
            }
            else
                Console.WriteLine($"{url} seems to be down :(");
        }
    }
}