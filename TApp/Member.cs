using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp
{
    abstract class Member : CodeElement
    {
        public Member()
        {

        }

        public Member(ElementContent content)
            : base(content)
        {

        }
    }
}