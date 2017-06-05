using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{
    public enum FieldCategory
    {
        Event, None
    }

    class Field : Member
    {
        public FieldCategory Category { get; set; }

        public string Initialization { get; set; }
        public Field() { }

        public string Type { get; set; }

        public Field(ElementContent content, Match match, FieldCategory type)
            : base(content)
        {
            Name = match.Groups["name"].Value;
            ListOfModifiers = CodeFile.GetModifiersList(match.Groups["modifiers"].Value);
            Type = match.Groups["type"].Value;
            Initialization = match.Groups["init"].Value;

            Category = type;
        }
    }
}
