namespace GemSharp {
    enum GemtextLineType {
        Text,
        Link,
        PreformatingToggle,
        PreformattedText,
        Heading1,
        Heading2,
        Heading3,
        UnorderedListItem,
        Quote
    }

    struct GemtextLine {
        GemtextLineType type;
        string text;
        Printer.Format format;
        Printer.Color color;

        public GemtextLine(string text, GemtextLineType type, Printer.Format format, Printer.Color color) {
            this.type = type;
            this.text = text;
            this.format = format;
            this.color = color;
        }

        public void Print(ref Printer printer) {
            switch (this.type) {
                case GemtextLineType.Link:
                    string[] split = this.text.Split(new string[] {" ", "\t"}, 3, StringSplitOptions.None);
                    printer.Write(split[0] + " ", Printer.Format.Normal, Printer.Color.Default);
                    if (split.Length == 3) {
                        printer.Write(split[1].Trim(), Printer.Format.Underline, Printer.Color.Cyan);
                        printer.WriteLine("\t" + split[2], Printer.Format.Normal, Printer.Color.Default);
                    }
                    else
                        printer.WriteLine(split[1], Printer.Format.Underline, Printer.Color.Cyan);
                    break;
                case GemtextLineType.Text:
                case GemtextLineType.PreformattedText:
                case GemtextLineType.Heading1:
                case GemtextLineType.Heading2:
                case GemtextLineType.Heading3:
                case GemtextLineType.UnorderedListItem:
                case GemtextLineType.Quote:
                    printer.WriteLine(this.text, this.format, this.color);
                    break;
            }
        }
    }

    internal class GemtextParser {
        private string text;
        private bool parsingPreformattedText;
        private List<GemtextLine> formattedLines;

        public GemtextParser(string text) {
            this.parsingPreformattedText = false;
            this.text = text;
            this.formattedLines = new List<GemtextLine>();

            Parse();
        }

        // TODO: user themes?
        public void Parse() {
            foreach (string line in this.text.Split('\n')) {
                if (this.parsingPreformattedText)
                    this.formattedLines.Add(new GemtextLine(line, GemtextLineType.PreformattedText, Printer.Format.Normal, Printer.Color.Default));
                else if (line.StartsWith("=>"))
                    this.formattedLines.Add(new GemtextLine(line, GemtextLineType.Link, Printer.Format.Underline, Printer.Color.Cyan));
                else if (line.StartsWith("```"))
                    this.parsingPreformattedText = !this.parsingPreformattedText;
                else if (line.StartsWith("#")) {
                    // Pop '#'s from the start of the line to get the heading type
                    GemtextLineType type = GemtextLineType.Heading1;
                    string _line = String.Copy(line);
                    while (_line.Remove(0, 1) == "#")
                        type = (GemtextLineType)(int)type++;
                    this.formattedLines.Add(new GemtextLine(line, type, Printer.Format.Bold, Printer.Color.Red));
                }
                else if (line.StartsWith("* "))
                    this.formattedLines.Add(new GemtextLine(line, GemtextLineType.UnorderedListItem, Printer.Format.Bold, Printer.Color.Default));
                else if (line.StartsWith(">"))
                    this.formattedLines.Add(new GemtextLine(line, GemtextLineType.Quote, Printer.Format.Normal, Printer.Color.Gray));
                else
                    this.formattedLines.Add(new GemtextLine(line, GemtextLineType.Text, Printer.Format.Normal, Printer.Color.Default));
            }
        }

        public void Print(ref Printer printer) {
            foreach (GemtextLine line in this.formattedLines)
                line.Print(ref printer);
        }
    }
}
