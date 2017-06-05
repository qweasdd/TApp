using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{

    enum HeaderType { UsingNamespace, UsingStatic };

    class Header
    {
        public Header() { }

        public Header(Match match)
        {

            Type = ((match.Groups[0].Value.Contains(" static ")) ? HeaderType.UsingStatic : HeaderType.UsingNamespace);
            if (match.Groups[2].Success)
            {
                Alias = match.Groups[1].Value;
                Content = match.Groups[2].Value;
            }
            else
                Content = match.Groups[1].Value;
        }
        public string Alias { get; set; }
        public string Content { get; set; }
        public HeaderType Type { get; set; }
    }
}