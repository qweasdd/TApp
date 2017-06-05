using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp
{
    abstract class CodeElement
    {
        public string Name { get; set; }

        protected ElementContent Content { get; set; }

        public List<string> ListOfModifiers { get; set; }

        public CodeElement() { }

        public int Length;

        public CodeElement(ElementContent content)
        {
            Content = content;

            Length = Content.GetNumberOfNonEmptyLines();
        }


    }
}

