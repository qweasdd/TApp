using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{


    class Namespace : CodeElement
    {
        public Namespace() { }

        public Namespace(ElementContent elementContent)
            : base(elementContent)
        {
            Name = new Regex(@"namespace\s+(\w+)").Match(elementContent.FileContent[elementContent.BeginIndex]).Groups[1].Value;

            ListOfNamespaces = new List<Namespace>();
            ListOfTypes = new List<Type>();

            ParseContent();
        }

        public List<Namespace> ListOfNamespaces { get; set; }

        public List<Type> ListOfTypes { get; set; }



        private Regex complicatedTypeRegex = new Regex(@"^(?:\s*(?<modifiers>(?:\w+\s+)*))?(?<type>class|struct|interface)\s+(?<name>[\w<>\s,\.]+)\s*(?::(?<inheritance>.*))?");
        private Regex delegateRegex = new Regex(@"^([^\""]*)\s* delegate\s+([<>\[\]\w]+)\s+(\w+)\s*\((.*)\)\s*;");
        private Regex enumRegex = new Regex(@"^.*enum\s+(?<name>\w+)\s*(?::\s*(?<type>\w+))?\s*");



        public void ParseContent()
        {
            for (int i = Content.BeginIndex + 2; i < Content.EndIndex; i++)
            {
                if (complicatedTypeRegex.IsMatch(Content.FileContent[i]))
                {
                    int closingBracketPos = CodeFile.FindClosingBracket(Content.FileContent, i + 1);
                    var match = complicatedTypeRegex.Match(Content.FileContent[i]);
                    switch (match.Groups["type"].Value)
                    {
                        case "class":
                            ListOfTypes.Add(new ComplicatedType(new ElementContent(Content.FileContent, i, closingBracketPos), match, CTCategory.Class));
                            break;
                        case "interface":
                            ListOfTypes.Add(new ComplicatedType(new ElementContent(Content.FileContent, i, closingBracketPos), match, CTCategory.Interface));
                            break;
                        case "struct":
                            ListOfTypes.Add(new ComplicatedType(new ElementContent(Content.FileContent, i, closingBracketPos), match, CTCategory.Struct));
                            break;
                    }
                    i = closingBracketPos;
                }
                else if (delegateRegex.IsMatch(Content.FileContent[i]))
                {
                    ListOfTypes.Add(new Delegate(new ElementContent(Content.FileContent, i, i), delegateRegex.Match(Content.FileContent[i])));
                }
                else if (enumRegex.IsMatch(Content.FileContent[i]))
                {
                    int closingBracketPos = CodeFile.FindClosingBracket(Content.FileContent, i + 1);
                    ListOfTypes.Add(new Enum(new ElementContent(Content.FileContent, i, i), enumRegex.Match(Content.FileContent[i])));
                    i = closingBracketPos;
                }

            }
        }
    }
}

