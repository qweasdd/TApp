using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace TApp
{

    enum CTCategory
    {
        Class, Struct, Interface
    }
    
    class ComplicatedType : Type
    {

        public CTCategory Category { get; set; }

        public List<string> InheritanceList { get; set; }
        public List<Member> ListOfMembers { get; set; }

        public List<Type> ListOfNestedTypes { get; set; }
        public ComplicatedType() { }

        public bool Generic { get; set; }


        public ComplicatedType(ElementContent content, Match match, CTCategory category)
            : base(content)
        {
            Name = match.Groups["name"].Value.Trim(' ' , '\t');
            if (Name.Contains("<"))
            {
                Name = Name.Substring(0, Name.IndexOf('<'));
                Generic = true;
            }

            InheritanceList = match.Groups["inheritance"].Value.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries).TakeWhile(x => x != "where").ToList();
            ListOfModifiers = CodeFile.GetModifiersList(match.Groups["modifiers"].Value);
            Category = category;

            ListOfMembers = new List<Member>();
            ListOfNestedTypes = new List<Type>();

            ParseContent();
        }
        

        private static Regex classRegex = new Regex(@"^(?:\s*(?<modifiers>(?:\w+\s+)*))?class\s+(?<name>[\w<>\s,\.]+)\s*(?::(?<inheritance>.*))?");
        private static Regex structRegex = new Regex(@"^(?:\s*(?<modifiers>(?:\w+\s+)*))?struct\s+(?<name>[\w<>\s,\.]+)\s*(?::(?<inheritance>.*))?");
        private static Regex interfaceRegex = new Regex(@"^(?:\s*(?<modifiers>(?:\w+\s+)*))?interface\s+(?<name>[\w<>\s,\.]+)\s*(?::(?<inheritance>.*))?");
        private static Regex delegateRegex = new Regex(@"^(?:(?<modifiers>[\s\w]*)\s+)?delegate\s+(?<return>[\w<>\s,\?\[\]]+)\s+(?<name>[\w\.]+)\s*\((?<arguments>.*)\)\s*;\s*$");
        private static Regex propertyRegex = new Regex(@"^(?:\s*(?<modifiers>(?:(?!delegate)\w+\s+)*))?(?!interface|class|struct)(?<return>[\w<>\?\s,\[\]]+)\s+(?<name>[\w\.]+)\s*$");

        private static Dictionary<MethodCategory, Regex> _methodRegexDictionary = new Dictionary<MethodCategory, Regex>()
        {
            { MethodCategory.Constructor, new Regex(@"^(?:\s*(?<modifiers>(?:(?!operator)\w+\s+)*))?(?<name>[\.\w]+)\s*\((?<arguments>[\w\?<>\=\[\],\s\.]*)\)(?:\s*:\s*(?<cc>\w+)\s*\(.*\))?\s*$") },
            { MethodCategory.Destructor, new Regex(@"^\s*~\s*(?<name>[\.\w])+\s*\(\)$") },
            { MethodCategory.Indexser, new Regex(@"^(?:(?<modifiers>[\s\w]*)\s+)?(?<return>[\w<>\s,\.\[\]]+)\s+(?:\w+\s*.\s*)?this\s*\[(?<arguments>[\w\=\?\s<>\[\],\.]*)\]\s*(?:where[^;]*)?") },
            { MethodCategory.Overload, new Regex(@"^(?:(?<modifiers>[\s\w]*)\s+)?(?:(?!implicit|explicit)(?<return>[\w<>\.\s,\[\]]+)\s+(?<name>operator\s*\S+)|(?<name>(?:implicit|explicit) operator)\s+(?<return>\?\w+))\s*\((?<arguments>[\w\=\?\s<>,\[\]\.]*)\)\s*(?:where[^;]*)?$") },
            { MethodCategory.None, new Regex(@"^(?:(?<modifiers>[\s\w]*)\s+)?(?!operator)(?<return>[\w<>\?\s,\.\[\]]+)\s+(?<name>[\.\w<>\.,\s]+)\s*\((?<arguments>[\w\=\?\s<>\[\],\.]*)\)\s*(?:where[^;]*)?(?<abstract>;\s*)?$") },
            { MethodCategory.Lambda, new Regex(@"^(?:(?<modifiers>[\s\w]*)\s+)?(?!operator)(?<return>[\w<>\?\s,\.\[\]]+)\s+(?<name>[.\w<>\.,\s]+)\s*\((?<arguments>[\w\=\?\s<>\[\],\.]*)\)\s*(?:where[^;=]*)?=>([^;]+);$")}
        };

        private static Dictionary<FieldCategory, Regex> _fieldRegexDictionary = new Dictionary<FieldCategory, Regex>()
        {
            { FieldCategory.Event , new Regex(@"^(?:(?<modifiers>[\s\w]*)\s+)?event\s+(?<type>[\w<>\s,\.\[\]]+)\s+(?<name>\w+)\s*;\s*$") },
            { FieldCategory.None, new Regex(@"^(?:\s*(?<modifiers>(?:(?!event)\w+\s+)*))?(?<type>[\w<>\?\s,\.\[\]]+)\s+(?<name>\w+)\s*(?:=\s*(?<init>.+)\s*)?;\s*$") }
        };

        private static List<MethodCategory> _methodCategories = _methodRegexDictionary.Keys.ToList();
        private static List<FieldCategory> _fieldCategories = _fieldRegexDictionary.Keys.ToList();

        
       

        public void ParseContent()
        {
            for (int i = Content.BeginIndex + 2; i < Content.EndIndex; i++)
            {
                if (classRegex.IsMatch(Content.FileContent[i]))
                {
                    int closingBracketPos = CodeFile.FindClosingBracket(Content.FileContent, i + 1);
                    ListOfNestedTypes.Add(new ComplicatedType(new ElementContent(Content.FileContent, i, closingBracketPos), classRegex.Match(Content.FileContent[i]), CTCategory.Class));
                    i = closingBracketPos;
                }
                else if (structRegex.IsMatch(Content.FileContent[i]))
                {
                    int closingBracketPos = CodeFile.FindClosingBracket(Content.FileContent, i + 1);
                    ListOfNestedTypes.Add(new ComplicatedType(new ElementContent(Content.FileContent, i, closingBracketPos), structRegex.Match(Content.FileContent[i]), CTCategory.Struct));
                    i = closingBracketPos;
                }
                else if (interfaceRegex.IsMatch(Content.FileContent[i]))
                {
                    int closingBracketPos = CodeFile.FindClosingBracket(Content.FileContent, i + 1);
                    ListOfNestedTypes.Add(new ComplicatedType(new ElementContent(Content.FileContent, i, closingBracketPos), interfaceRegex.Match(Content.FileContent[i]), CTCategory.Interface));
                    i = closingBracketPos;
                }
                else if (delegateRegex.IsMatch(Content.FileContent[i]))
                {
                    ListOfNestedTypes.Add(new Delegate(new ElementContent(Content.FileContent, i, i), delegateRegex.Match(Content.FileContent[i])));
                }
                else if (propertyRegex.IsMatch(Content.FileContent[i]))
                {
                    int closingBracketPos = CodeFile.FindClosingBracket(Content.FileContent, i + 1);
                    if (closingBracketPos + 1 < Content.FileContent.Count && Content.FileContent[closingBracketPos + 1].TrimStart(' ', '\t').StartsWith("="))
                        closingBracketPos++;

                    ListOfMembers.Add(new Property(new ElementContent(Content.FileContent, i, closingBracketPos), propertyRegex.Match(Content.FileContent[i])));

                    i = closingBracketPos;
                }
                else
                {
                    bool isMatched = false;

                    for (int j = 0; j < _methodCategories.Count; j++)
                    {
                        if (_methodRegexDictionary[_methodCategories[j]].IsMatch(Content.FileContent[i]))
                        {
                            var match = _methodRegexDictionary[_methodCategories[j]].Match(Content.FileContent[i]);
                            if (match.Groups["abstract"].Value.Length > 0 || _methodCategories[j] == MethodCategory.Lambda)
                            {
                                ListOfMembers.Add(new Method(new ElementContent(Content.FileContent, i, i), match, _methodCategories[j]));
                            }
                            else
                            {
                                int closingBracketPos = CodeFile.FindClosingBracket(Content.FileContent, i + 1);

                                if (_methodCategories[j] == MethodCategory.None && Name == match.Groups["name"].Value)
                                    ListOfMembers.Add(new Method(new ElementContent(Content.FileContent, i, closingBracketPos), match, MethodCategory.Constructor));
                                else if (_methodCategories[j] == MethodCategory.Constructor && Name != match.Groups["name"].Value)
                                {
                                    ListOfMembers.Add(new Method(new ElementContent(Content.FileContent, i, closingBracketPos), _methodRegexDictionary[MethodCategory.None].Match(Content.FileContent[i]), MethodCategory.None));
                                }
                                else
                                    ListOfMembers.Add(new Method(new ElementContent(Content.FileContent, i, closingBracketPos), match, _methodCategories[j]));

                                i = closingBracketPos;
                            }
                            isMatched = true;
                            break;
                        }
                    }

                    if (!isMatched)
                    {
                        for (int j = 0; j < _fieldCategories.Count; j++)
                        {
                            if (_fieldRegexDictionary[_fieldCategories[j]].IsMatch(Content.FileContent[i]))
                            {
                                var match = _fieldRegexDictionary[_fieldCategories[j]].Match(Content.FileContent[i]);
                                ListOfMembers.Add(new Field(new ElementContent(Content.FileContent, i, i), match, _fieldCategories[j]));

                                break;
                            }
                        }
                    }
                }

            }
        }

    }
}
