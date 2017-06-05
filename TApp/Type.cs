using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp
{
    abstract class Type : CodeElement
    {
        public Type() { }
        public Type(ElementContent content)
            : base(content)
        {
            ListOfModifiers = new List<string>();
        }

    }
}
