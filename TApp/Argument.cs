using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{
    struct Argument
    {
        public Argument(string argumentType, string argumentName, string initialization = "")
        {
            Type = argumentType;
            Name = argumentName;
            Initialization = initialization;
        }

        public Argument(string raw)
        {
            raw = raw.Trim(' ', '\t');
            var match = new Regex(@"^([\w<>\s,\[\]]+)\s+(\w+)\s*(?:=(.*))?$").Match(raw);
            Type = match.Groups[1].Value;
            Name = match.Groups[2].Value;
            Initialization = match.Groups[3].Value;
        }

        public string Type { get; set; }
        public string Name { get; set; }
        public string Initialization { get; set; }
    }
}
