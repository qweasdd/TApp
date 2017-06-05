using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace TApp
{
    class CodeFile
    {
        public CodeFile(string file)
        {
            ListOfHeaders = new List<Header>();
            ListOfNamespaces = new List<Namespace>(1);
            _fileContent = new List<string>(file.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public List<string> _fileContent;

        private static Regex _headerRegex = new Regex(@"using\s+(?:static)?\s*([\w.]+)(?:\s*=\s*([\w.]+))?");

        public List<Header> ListOfHeaders { get; set; }

        public List<Namespace> ListOfNamespaces { get; set; }

        public void ParseFile()
        {
            ParseHeaders();
            ParseNamespaces();
        }


        private void ParseHeaders()
        {
            foreach (string str in _fileContent)
            {
                var match = _headerRegex.Match(str);
                if (match.Success)
                    ListOfHeaders.Add(new Header(match));
            }
        }

        private void ParseNamespaces()
        {
            for (int i = 0; i < _fileContent.Count; i++)
            {
                if (_fileContent[i].Trim(' ', '\t').StartsWith("namespace "))
                {
                    int closingBracketPos = FindClosingBracket(_fileContent, i + 1);

                    ListOfNamespaces.Add(new Namespace(new ElementContent(_fileContent, i, closingBracketPos)));
                    i = closingBracketPos;
                }
            }
        }

        public static int FindClosingBracket(List<string> fileContent, int openingBracketPos)
        {
            int bracketDepth = 1;
            int currentLine = openingBracketPos;
            while (bracketDepth > 0)
            {
                currentLine++;
                var tempLine = fileContent[currentLine].Replace(" ", "").Replace("\t", "");
                if (tempLine == "{")
                        bracketDepth++;
                else if (tempLine == "}" || tempLine == "};")
                        bracketDepth--;
               
            }

            return currentLine;
        }

        public static List<string> GetModifiersList(string data)
        {
            return data.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}