using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TApp
{
    class ElementContent
    {
        public ElementContent(List<string> fileContent, int beginIndex, int endIndex)
        {
            FileContent = fileContent;
            BeginIndex = beginIndex;
            EndIndex = endIndex;
        }


        public List<string> FileContent { get; set; }
        public int BeginIndex { get; set; }
        public int EndIndex { get; set; }

        public int GetNumberOfNonEmptyLines()
        {
            int count = 0;

            for (int i = BeginIndex; i <= EndIndex; i++)
            {
                if (FileContent[i].Trim(new char[] { ' ', '\t' }) != string.Empty)
                {
                    count++;
                }
            }

            return count;
        }

        public int GetMaxDepthOfNestedBrackets()
        {
            int max = 0;
            int counter = 0;
            for (int i = BeginIndex; i < EndIndex; i++)
            {
                if (FileContent[i].Trim(new char[] { ' ', '\t' }) == "{")
                {
                    if (++counter > max)
                        max = counter;
                }
                else if (FileContent[i].Trim(new char[] { ' ', '\t' }) == "}")
                    counter--;
            }

            return max;
        }
    }
}

