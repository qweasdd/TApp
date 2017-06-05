using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{
    class Delegate : Type
    {

        public string returnType;
        public List<Argument> ListOfArguments { get; set; }

        public Delegate()
        { }

        public Delegate(ElementContent content, Match match)
            : base(content)
        {
            Name = match.Groups[3].Value;
            ListOfModifiers = match.Groups[1].Value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            returnType = match.Groups[2].Value;

            ListOfArguments = Method.ParseAguments(match.Groups[4].Value);


        }




    }
}

