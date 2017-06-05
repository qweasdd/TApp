using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{
    class Enum : Type
    {
        public string enumType;

        public Enum() { }

        private List<EnumElement> _listOfEnumElements { get; set; }

        public Enum(ElementContent content, Match match)
            : base(content)
        {
            Name = match.Groups["name"].Value;
            enumType = match.Groups["type"].Value;
            _listOfEnumElements = ParseElements(Content.FileContent[Content.BeginIndex + 2]);
        }

        private static List<EnumElement> ParseElements(string raw)
        {
            var elementRegex = new Regex(@"(\w+)\s*(?:=\s*(\w+))?");
            return raw.Split(',').Select((x) =>
            {
                var match = elementRegex.Match(x);
                return new EnumElement(match.Groups[1].Value, match.Groups[2].Value);
            }).ToList();
        }


        struct EnumElement
        {
            public string Name { get; set; }
            public string IntegerValue { get; set; }

            public EnumElement(string name, string value)
            {
                Name = name;
                IntegerValue = value;
            }
        }
    }
}

