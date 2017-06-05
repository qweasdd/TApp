using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace TApp
{
    class Property : Member
    {
        public string ReturnType { get; set; }
        public int MaxBracketDepth { get; set; }

        public string Initialization { get; set; }

        public List<Method> GetternSetter;

        public Property() { }

        public Property(ElementContent content, Match match)
            : base(content)
        {

            Name = match.Groups["name"].Value;
            ListOfModifiers = CodeFile.GetModifiersList(match.Groups["modifiers"].Value);
            ReturnType = match.Groups["return"].Value;

            if (Content.FileContent[Content.EndIndex].Replace(" ", "").Replace("\t", "") != "}")
            {
                Initialization = new Regex(@"^\s*=\s*(?<init>.*);").Match(Content.FileContent[Content.EndIndex]).Groups["init"].Value;
            }

            GetternSetter = new List<Method>();
            ParseGetnSet();

            MaxBracketDepth = Content.GetMaxDepthOfNestedBrackets();
        }

        public void ParseGetnSet()
        {
            var getsetRegex = new Regex(@"^\s*(?<name>get|set)\s*(?<auto>(?:;|=>[^;]+;))?\s*$");
            for (int i = Content.BeginIndex + 2; i < Content.EndIndex; i++)
            {
                if (getsetRegex.IsMatch(Content.FileContent[i]))
                {
                    var match = getsetRegex.Match(Content.FileContent[i]);

                    if (match.Groups["auto"].Value == ";")
                        GetternSetter.Add(new Method(new ElementContent(Content.FileContent, i, i)) { Name = match.Groups["name"].Value, Category = MethodCategory.None });
                    else if (match.Groups["auto"].Value != string.Empty)
                    {
                        GetternSetter.Add(new Method(new ElementContent(Content.FileContent, i, i)) { Name = match.Groups["name"].Value, Category = MethodCategory.Lambda });
                    }
                    else
                    {
                        int closingBracketPos = CodeFile.FindClosingBracket(Content.FileContent, i + 1);

                        GetternSetter.Add(new Method(new ElementContent(Content.FileContent, i, closingBracketPos)) { Name = match.Groups["name"].Value, Category = MethodCategory.None });

                        i = closingBracketPos;
                    }
                }
            }
        }
    }
}

