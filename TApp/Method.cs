using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{

    public enum MethodCategory
    {
        Constructor, Destructor, Overload, Indexser, Lambda, None
    }

    class Method : Member
    {


        public MethodCategory Category { get; set; }
        public List<Argument> ListOfArguments { get; set; }

        public string ConstructorCall;

        public int MaxBracketDepth;
        public Method() { }

        public Method(ElementContent content)
            : base(content)
        {
            MaxBracketDepth = Content.GetMaxDepthOfNestedBrackets();
        }
        public string returnType;

        public Method(ElementContent content, Match match, MethodCategory type)
            : base(content)
        {

            Name = match.Groups["name"].Value;
            ConstructorCall = match.Groups["cc"].Value;
            ListOfArguments = ParseAguments(match.Groups["arguments"].Value);
            ListOfModifiers = CodeFile.GetModifiersList(match.Groups["modifiers"].Value);
            returnType = match.Groups["return"].Value;

            Category = type;

            MaxBracketDepth = Content.GetMaxDepthOfNestedBrackets();
        }



        public static List<Argument> ParseAguments(string raw)
        {
            var list = new List<Argument>();
            if (raw.Trim(' ', '\t') == string.Empty) return list;

            Console.WriteLine(raw);
            int lastChunkIndex = 0;
            int bracketDepth = 0;
            for (int i = 0; i < raw.Length; i++)
            {

                if (raw[i] == '<')
                    bracketDepth++;
                else if (raw[i] == '>')
                    bracketDepth--;
                else if (bracketDepth == 0 && raw[i] == ',')
                {
                    list.Add(new Argument(raw.Substring(lastChunkIndex, i - lastChunkIndex)));
                    lastChunkIndex = i + 1;
                }
            }
            list.Add(new Argument(raw.Substring(lastChunkIndex, raw.Length - lastChunkIndex)));

            return list;
        }

    }
}
