using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ArchiMetrics.Analysis;
using System.Text.RegularExpressions;

namespace TApp
{
    static class DataExtractor
    {
        private static readonly Dictionary<string, int> modifiersDict;

        static DataExtractor()
        {
            modifiersDict = new Dictionary<string, int>
            {
                { "public", 0 },
                { "private", 0 },
                { "internal", 0 },
                { "protected", 0 },
                { "abstract", 0},
                { "const", 0},
                { "event", 0},
                { "extern", 0 },
                { "new", 0},
                { "override", 0 },
                { "partial", 0},
                { "readonly", 0},
                { "sealed", 0},
                { "static", 0},
                { "unsafe", 0},
                { "virtual", 0 },
                { "volatile", 0},
                { "async", 0}
            };

        }

        public static void Extract(DatabaseEntities dbcontext)
        {
            int counter = 0;
            foreach (var download in dbcontext.Downloads)
            {
                if (download.Data == null)
                {
                    Console.WriteLine(download.FContent);
                    var data = ExtractData(download.FContent);
                    data.Download = download;
                    download.Data = data;
                   

                }

                Console.WriteLine(++counter);
                //if (counter % 500 == 0)
                //{
                //    dbcontext.SaveChanges();
                //}
            }

            dbcontext.SaveChanges();
        }

        public static Data ExtractData(string code)
        {
            var data = new Data();

            var tree = CSharpSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            data.FileLength = (code.Length);
            GetClassData(root, data);
            GetStructData(root, data);
            GetInterfaceData(root, data);
            GetNamespaceAmount(root, data);
            GetSowtwareMetrics(tree, data);
            GetCommentaryData(code, data);
            GetNames(root, data);
            GetModifiers(root, data);
            GetLinesLength(code, data);
            GetCommentaryContent(code, data);

            return data;
        }

        private static void GetClassData(CompilationUnitSyntax root, Data data)
        {
            var classData = GetMembersData(root, typeof(ClassDeclarationSyntax), data);
            if (!(classData.NameLengths is null))
            {
                data.AverageClassLength = classData.Lengths.Average();
                data.MedianClassLength = classData.Lengths.Median();
                data.AverageClassNameLength = classData.NameLengths.Average();
                data.MedianClassNameLength = classData.NameLengths.Median();
                data.ClassAmount = classData.NameLengths.Count;
            }
        }

        private static void GetStructData(CompilationUnitSyntax root, Data data)
        {
            var structData = GetMembersData(root, typeof(StructDeclarationSyntax), data);
            if (!(structData.NameLengths is null))
            {
                data.AverageStructLength = structData.Lengths.Average();
                data.MedianStructLength = structData.Lengths.Median();
                data.AverageStructNameLength = structData.NameLengths.Average();
                data.MedianStructNameLength = structData.NameLengths.Median();
                data.StructAmount = structData.NameLengths.Count;
            }
        }

        private static void GetInterfaceData(CompilationUnitSyntax root, Data data)
        {
            var interfaceData = GetMembersData(root, typeof(InterfaceDeclarationSyntax), data);
            if (!(interfaceData.NameLengths is null))
            {
                data.AverageInterfaceLength = interfaceData.Lengths.Average();
                data.MedianInterfaceLength = interfaceData.Lengths.Median();
                data.AverageInterfaceNameLength = interfaceData.NameLengths.Average();
                data.MedianInterfaceNameLength = interfaceData.NameLengths.Median();
                data.InterfaceAmount = interfaceData.NameLengths.Count;
            }
        }

        
        private static (List<int> Lengths, List<int> NameLengths) GetMembersData(CompilationUnitSyntax root, Type type, Data data)
        {
            List<int> lengths = new List<int>();
            
            List<int> nameLengths = new List<int>();

            foreach (var member in root.Members)
            {
                if (member is NamespaceDeclarationSyntax)
                {
                    var temp = member as NamespaceDeclarationSyntax;

                    foreach (var namespaceMember in temp.Members)
                    {
                        if (namespaceMember is NamespaceDeclarationSyntax)
                        {
                            GetNestedNamespaceMembersData((NamespaceDeclarationSyntax)namespaceMember, type,  lengths, nameLengths);
                        }
                        else if (namespaceMember.GetType() == type)
                        {
                            lengths.Add(namespaceMember.Span.Length);
                            nameLengths.Add((namespaceMember as BaseTypeDeclarationSyntax).Identifier.ValueText.Length);
                        }
                    }
                }
            }
            if (nameLengths.Count > 0)
                return (lengths, nameLengths);
            else
                return (null, null);
        }


        private static void GetNestedNamespaceMembersData
            (NamespaceDeclarationSyntax nds, Type type, List<int> lengths,List<int> nameLengths)
        {
            foreach (var namespaceMember in nds.Members)
            {
                if (namespaceMember is NamespaceDeclarationSyntax)
                {
                    GetNestedNamespaceMembersData((NamespaceDeclarationSyntax)namespaceMember, type, lengths, nameLengths);
                }
                else if (namespaceMember.GetType() == type)
                {
                    lengths.Add(namespaceMember.Span.Length);
                    nameLengths.Add((namespaceMember as BaseTypeDeclarationSyntax).Identifier.ValueText.Length);
                }
            }
        }


        private static void GetNamespaceAmount(CompilationUnitSyntax root, Data data)
        {
            int count = 0;
            foreach (var member in root.Members)
            {
                if (member is NamespaceDeclarationSyntax)
                {
                    count++;
                    GetNestedNamespaceAmount((NamespaceDeclarationSyntax)member, ref count);
                }
            }

            data.NamespaceAmount = count;
        }

        private static void GetNestedNamespaceAmount(NamespaceDeclarationSyntax nds, ref int count)
        {
            foreach (var member in nds.Members)
            {
                if (member is NamespaceDeclarationSyntax)
                {
                    count++;
                    GetNestedNamespaceAmount((NamespaceDeclarationSyntax)member, ref count);
                }
            }
        }


        private static void GetSowtwareMetrics(SyntaxTree tree, Data data)
        {
            var calculator = new CodeMetricsCalculator();
            var res = calculator.Calculate(new List<SyntaxTree>() { tree });

            if (res.Status != TaskStatus.Faulted && res.Result.Count() > 0)
            {
                data.MaintainabilityIndex = res.Result.Sum(x => x.MaintainabilityIndex) / res.Result.Count();
                data.CyclomaticComplexity = res.Result.Sum(x => x.CyclomaticComplexity) / res.Result.Count();
                data.LinesOfCode = res.Result.Sum(x => x.LinesOfCode);
            }
            
        }

        private static void GetCommentaryData(string code, Data data)
        {
            var file = new List<string>(code.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
            int commentaryLength = 0;
            int commentaryLines = 0;
            for (int i = 0; i < file.Count; i++)
            {
                var trimLine = file[i].Trim();
                if (trimLine.StartsWith("//") || trimLine.StartsWith("/*"))
                {
                    commentaryLength += trimLine.Length;
                    commentaryLines++;
                }
            }

            data.CommentaryLines = commentaryLines;
            data.CommentaryProportion = (double)commentaryLength / data.FileLength;
        }

        private static void GetNames(CompilationUnitSyntax root, Data data)
        {

            var namesList = new List<string>();
            var namesData = new NamesAndLengthData();

            foreach (var member in root.Members)
            {
                if (member is NamespaceDeclarationSyntax)
                {
                    var temp = member as NamespaceDeclarationSyntax;
                    foreach (var namespaceMember in temp.Members)
                    {
                        if (namespaceMember is NamespaceDeclarationSyntax)
                        {
                            GetNestedNamespaceNames((NamespaceDeclarationSyntax)namespaceMember, namesList, namesData);
                        }
                        else if (namespaceMember is TypeDeclarationSyntax)
                        {
                            namesData.AddNewElement();

                            var t = namespaceMember as TypeDeclarationSyntax;
                            namesList.Add(t.Identifier.ValueText);
                            GetTypeMemebersName(t, namesList, namesData);

                        }
                        else if (namespaceMember is EnumDeclarationSyntax)
                        {
                            var t = namespaceMember as EnumDeclarationSyntax;
                            namesList.Add(t.Identifier.ValueText);
                            GetEnumMembersNames(t, namesList);
                        }
                        
                    }
                }
            }

            data.Names = string.Join(" ",  namesList);
            GetNamesFeatures(data, namesList);
        

            if (namesData.VariablesNameLengths.Count > 0)
            {
                data.AverageVariableNameLength = namesData.VariablesNameLengths.Average();
                data.MedianVariableNameLength = namesData.VariablesNameLengths.Median();
            }
            if (namesData.MethodsNameLengths.Count > 0)
            {
                data.AverageMethodNameLength = namesData.MethodsNameLengths.Average();
                data.MedianMethodNameLength = namesData.MethodsNameLengths.Median();
                data.AverageMethodParametersAmount = namesData.MethodParametersAmount.Average();
                data.MedianMethodParametersAmount = namesData.MethodParametersAmount.Median();
                data.AverageMethodLength = namesData.MethodLengths.Average();
                data.MedianMethodLength = namesData.MethodLengths.Median();
            }
            if (namesData.ClassFieldsAmounts.Count > 0)
            {
                data.AverageClassFieldsAmount = namesData.ClassFieldsAmounts.Average();
                data.MedianClassFieldsAmount = namesData.ClassFieldsAmounts.Median();
            }
            if (namesData.StructFieldsAmounts.Count > 0)
            {
                data.AverageStructFieldsAmount = namesData.StructFieldsAmounts.Average();
                data.MedianStructFieldsAmount = namesData.StructFieldsAmounts.Median();
            }
            if (namesData.ClassPropertiesAmounts.Count > 0)
            {
                data.AverageClassPropertiesAmount = namesData.ClassPropertiesAmounts.Average();
                data.MedianClassPropertiesAmount = namesData.ClassPropertiesAmounts.Median();
            }
            if (namesData.StructPropertiesAmounts.Count > 0)
            {
                data.AverageStructPropertiesAmount = namesData.StructPropertiesAmounts.Average();
                data.MedianStructPropertiesAmount = namesData.StructPropertiesAmounts.Median();
            }
            if (namesData.InterfacePropertiesAmounts.Count > 0)
            {
                data.AverageInterfacePropertiesAmount = namesData.InterfacePropertiesAmounts.Average();
                data.MedianInterfacePropertiesAmount = namesData.InterfacePropertiesAmounts.Median();
            }
            if (namesData.ClassMethodsAmounts.Count > 0)
            {
                data.AverageClassMethodsAmount = namesData.ClassMethodsAmounts.Average();
                data.MedianClassMethodsAmount = namesData.ClassMethodsAmounts.Median();
            }
            if (namesData.StructMethodsAmounts.Count > 0)
            {
                data.AverageStructMethodsAmount = namesData.StructMethodsAmounts.Average();
                data.MedianStructMethodsAmount = namesData.StructMethodsAmounts.Median();
            }
            if (namesData.InterfaceMethodsAmounts.Count > 0)
            {
                data.AverageInterfaceMethodsAmount = namesData.InterfaceMethodsAmounts.Average();
                data.MedianInterfaceMethodsAmount = namesData.InterfaceMethodsAmounts.Median();
            }
        }

        private static void GetNestedNamespaceNames(NamespaceDeclarationSyntax nds, List<string> namesList, NamesAndLengthData namesData)
        {
            foreach (var namespaceMember in nds.Members)
            {
                if (namespaceMember is NamespaceDeclarationSyntax)
                {
                    GetNestedNamespaceNames((NamespaceDeclarationSyntax)namespaceMember, namesList, namesData);
                }
                else if (namespaceMember is TypeDeclarationSyntax)
                {
                    namesData.AddNewElement();

                    var t = namespaceMember as TypeDeclarationSyntax;
                    namesList.Add(t.Identifier.ValueText);
                    GetTypeMemebersName(t, namesList, namesData);

                }
                else if (namespaceMember is EnumDeclarationSyntax)
                {
                    var t = namespaceMember as EnumDeclarationSyntax;
                    namesList.Add(t.Identifier.ValueText);
                    GetEnumMembersNames(t, namesList);
                }
            }
        }

        private static void GetTypeMemebersName(TypeDeclarationSyntax tds, List<string> namesList, NamesAndLengthData namesData)
        {
            foreach (var member in tds.Members)
            {
                if (member is FieldDeclarationSyntax)
                {
                    var t = member as FieldDeclarationSyntax;
                    foreach (var variable in t.Declaration.Variables)
                    {
                        namesList.Add(variable.Identifier.ValueText);
                        namesData.VariablesNameLengths.Add(variable.Identifier.ValueText.Length);
                    }

                    switch (tds.Kind())
                    {
                        case SyntaxKind.ClassDeclaration:
                            namesData.ClassFieldsAmounts[namesData.ClassFieldsAmounts.Count - 1]++;
                            break;
                        case SyntaxKind.StructDeclaration:
                            namesData.StructFieldsAmounts[namesData.ClassFieldsAmounts.Count - 1]++;
                            break;
                    }

                }
                else if (member is PropertyDeclarationSyntax)
                {
                    namesList.Add(((PropertyDeclarationSyntax)member).Identifier.ValueText);

                    switch (tds.Kind())
                    {
                        case SyntaxKind.ClassDeclaration:
                            namesData.ClassPropertiesAmounts[namesData.ClassPropertiesAmounts.Count - 1]++;
                            break;
                        case SyntaxKind.StructDeclaration:
                            namesData.StructPropertiesAmounts[namesData.StructPropertiesAmounts.Count - 1]++;
                            break;
                        case SyntaxKind.InterfaceDeclaration:
                            namesData.InterfacePropertiesAmounts[namesData.InterfacePropertiesAmounts.Count - 1]++;
                            break;
                    }
                }
                else if (member is EventDeclarationSyntax)
                {
                    namesList.Add(((EventDeclarationSyntax)member).Identifier.ValueText);
                }
                else if (member is DelegateDeclarationSyntax)
                {
                    namesList.Add(((DelegateDeclarationSyntax)member).Identifier.ValueText);
                }
                else if (member is MethodDeclarationSyntax)
                {
                    var t = member as MethodDeclarationSyntax;
                    namesList.Add(t.Identifier.ValueText);
                    namesData.MethodsNameLengths.Add(t.Identifier.ValueText.Length);
                    namesData.MethodLengths.Add(t.Span.Length);

                    switch (tds.Kind())
                    {
                        case SyntaxKind.ClassDeclaration:
                            namesData.ClassMethodsAmounts[namesData.ClassMethodsAmounts.Count - 1]++;
                            break;
                        case SyntaxKind.StructDeclaration:
                            namesData.StructMethodsAmounts[namesData.StructMethodsAmounts.Count - 1]++;
                            break;
                        case SyntaxKind.InterfaceDeclaration:
                            namesData.InterfaceMethodsAmounts[namesData.InterfaceMethodsAmounts.Count - 1]++;
                            break;
                    }

                    GetMethodVariabesNames(t, namesList, namesData);
                }
                else if (member is EnumDeclarationSyntax)
                {
                    var t = member as EnumDeclarationSyntax;
                    namesList.Add(t.Identifier.ValueText);
                    GetEnumMembersNames(t, namesList);
                }

            }
        }

        private static void GetMethodVariabesNames(MethodDeclarationSyntax mds, List<string> names, NamesAndLengthData namesData)
        {
            foreach (var parametr in mds.ParameterList.Parameters)
            {
                names.Add(parametr.Identifier.ValueText);
            }
            namesData.MethodParametersAmount.Add(mds.ParameterList.Parameters.Count());

            if (mds.Body == null)
                return;

            foreach (var statement in mds.Body.Statements)
            {
                if (statement is LocalDeclarationStatementSyntax)
                {
                    var t = statement as LocalDeclarationStatementSyntax;
                    foreach (var variable in t.Declaration.Variables)
                    {
                        names.Add(variable.Identifier.ValueText);
                        namesData.VariablesNameLengths.Add(variable.Identifier.ValueText.Length);
                    }
                }
            }
        }

        private static void GetEnumMembersNames(EnumDeclarationSyntax eds, List<string> names)
        {
            foreach (var member in eds.Members)
            {
                names.Add(member.Identifier.ValueText);
            }
        }

        private static void GetNamesFeatures(Data data, List<string> names)
        {
            List<string> splittedNames = new List<string>(); 

            foreach (var name in names)
            {
                var temp = name;
                if (name.StartsWith("_"))
                {
                    temp = name.Substring(1);
                    data.PrefixUnderscoreNamesAmount++;
                    
                }

                if (Regex.IsMatch(temp, @"^[A-Z0-9]+(?:(?:_[A-Z0-9]+)+)?$"))
                {
                    data.UppercaseWithUnderscoreNamesAmount++;
                    GetWordsFromUnderscoreName(temp, splittedNames);
                }
                else if (Regex.IsMatch(temp, @"^[a-z0-9]+$"))
                {
                    data.LowercaseNamesAmount++;
                    splittedNames.Add(temp);
                }
                else if (Regex.IsMatch(temp, @"^[a-z0-9]+(?:(?:_[a-z0-9]+)+)?$"))
                {
                    data.UnderscoreNamesAmount++;
                    GetWordsFromUnderscoreName(temp, splittedNames);
                }
                else if (Regex.IsMatch(temp, @"^(?:[A-Z][a-z]*[0-9]*)+$"))
                {
                    data.PascalCaseNamesAmount++;
                    GetNamesFromPascalCaseName(temp, splittedNames);
                }
                else if (Regex.IsMatch(temp, @"^[a-z]+[0-9]*(?:[A-Z][a-z]*[0-9]*)+$"))
                {
                    data.CamelCaseNamesAmount++;
                    GetNamesFromCamelCaseName(temp, splittedNames);
                }
            }

            if (names.Count > 0)
            {
                data.PrefixUnderscoreNamesFraction = (double)data.PrefixUnderscoreNamesAmount / names.Count;
                data.UnderscoreNamesFraction = (double)data.UnderscoreNamesAmount / names.Count;
                data.UppercaseWithUnderscoreNamesFraction = (double)data.UppercaseWithUnderscoreNamesAmount / names.Count;
                data.LowercaseNamesFraction = (double)data.LowercaseNamesAmount / names.Count;
                data.PascalCaseNamesFraction = (double)data.PascalCaseNamesAmount / names.Count;
                data.CamelCaseNamesFraction = (double)data.CamelCaseNamesAmount / names.Count;
            }

            data.WordsFromNames = string.Join(" ", splittedNames);
        }

        private static void GetWordsFromUnderscoreName(string name, List<string> list)
        {
            list.AddRange(name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static void GetNamesFromPascalCaseName(string name, List<string> list)
        { 
            if (name.Length == 1)
            {
                list.Add(name);
                return;
            }
            StringBuilder str = new StringBuilder(name.Substring(0, 1));
            
            for (int i = 1; i < name.Length - 1; i++)
            {
                if (char.IsUpper(name[i]) && (char.IsLower(name[i + 1]) || char.IsNumber(name[i - 1]) || char.IsLower(name[i - 1])))
                {
                    list.Add(str.ToString());
                    str = new StringBuilder(name[i].ToString());
                }
                else
                {
                    str.Append(name[i]);
                }
            }
            if (char.IsUpper(name[name.Length - 1]) && char.IsLower(name[name.Length - 2]))
            {
                list.Add(str.ToString());
                list.Add(name.Last().ToString());
            }
            else
            {
                list.Add(str.Append(name.Last()).ToString());
            }
        }

        private static void GetNamesFromCamelCaseName(string name, List<string> list)
        {
            var pos = name.Select((x, i) => (x, i)).First(input => char.IsUpper(input.Item1)).Item2;
            list.Add(name.Substring(0, pos));
            GetNamesFromPascalCaseName(name.Substring(pos), list);
        }

        private static void GetModifiers(CompilationUnitSyntax root, Data data)
        {

            foreach (var key in modifiersDict.Keys.ToList())
            {
                modifiersDict[key] = 0;
            }


            foreach (var member in root.Members)
            {
                if (member is NamespaceDeclarationSyntax)
                {
                    var temp = member as NamespaceDeclarationSyntax;
                    foreach (var namespaceMember in temp.Members)
                    {
                        if (namespaceMember is NamespaceDeclarationSyntax)
                        {
                            GetNestedNamepsaceModifiers((NamespaceDeclarationSyntax)namespaceMember);
                        }
                        else if (namespaceMember is TypeDeclarationSyntax)
                        {
                            var t = namespaceMember as TypeDeclarationSyntax;
                            CountModifiers(t.Modifiers);
                            GetTypeMembersModifiers(t, modifiersDict);
                        }
                        else if (namespaceMember is EnumDeclarationSyntax)
                        {
                            CountModifiers((namespaceMember as BaseTypeDeclarationSyntax).Modifiers);
                        }
                    }
                }
            }


            data.PublicAmount = modifiersDict["public"];
            data.PrivateAmount = modifiersDict["private"];
            data.InternalAmount = modifiersDict["internal"];
            data.ProtectedAmount = modifiersDict["protected"];
            data.AbstractAmount = modifiersDict["abstract"];
            data.ConstAmount = modifiersDict["const"];
            data.EventAmount = modifiersDict["event"];
            data.ExternAmount = modifiersDict["extern"];
            data.NewAmount = modifiersDict["new"];
            data.OverrideAmount = modifiersDict["override"];
            data.PartialAmount = modifiersDict["partial"];
            data.ReadonlyAmount = modifiersDict["readonly"];
            data.SealedAmount = modifiersDict["sealed"];
            data.StaticAmount = modifiersDict["static"];
            data.UnsafeAmount = modifiersDict["unsafe"];
            data.VirtualAmount = modifiersDict["virtual"];
            data.VolatileAmount = modifiersDict["volatile"];
            data.AsyncAmount = modifiersDict["async"];
        }

        private static void GetNestedNamepsaceModifiers(NamespaceDeclarationSyntax nds)
        {
            foreach (var namespaceMember in nds.Members)
            {
                if (namespaceMember is NamespaceDeclarationSyntax)
                {
                    GetNestedNamepsaceModifiers((NamespaceDeclarationSyntax)namespaceMember);
                }
                else if (namespaceMember is TypeDeclarationSyntax)
                {
                    var t = namespaceMember as TypeDeclarationSyntax;
                    CountModifiers(t.Modifiers);
                    GetTypeMembersModifiers(t, modifiersDict);
                }
                else if (namespaceMember is EnumDeclarationSyntax)
                {
                    CountModifiers((namespaceMember as BaseTypeDeclarationSyntax).Modifiers);
                }
            }
        }

        private static void CountModifiers(SyntaxTokenList stl)
        {
            foreach (var modifier in stl)
            {
                try
                {
                    modifiersDict[modifier.ValueText]++;
                }
                catch (KeyNotFoundException ex)
                { }
            }
        }

        private static void GetTypeMembersModifiers(TypeDeclarationSyntax tds, Dictionary<string, int> modifiersDict)
        {
            foreach (var member in tds.Members)
            {
                if (member is FieldDeclarationSyntax)
                {
                    CountModifiers((member as FieldDeclarationSyntax).Modifiers);
                }
                else if (member is PropertyDeclarationSyntax)
                {
                    CountModifiers((member as PropertyDeclarationSyntax).Modifiers);
                }
                else if (member is EventDeclarationSyntax)
                {
                    CountModifiers((member as EventDeclarationSyntax).Modifiers);
                }
                else if (member is DelegateDeclarationSyntax)
                {
                    CountModifiers((member as DelegateDeclarationSyntax).Modifiers);
                }
                else if (member is MethodDeclarationSyntax)
                {
                    var t = member as MethodDeclarationSyntax;
                    CountModifiers(t.Modifiers);
                    GetMethodVariabesModifiers(t);
                }
                else if (member is EnumDeclarationSyntax)
                {
                    CountModifiers((member as EnumDeclarationSyntax).Modifiers);
                }

            }
        }


        private static void GetMethodVariabesModifiers(MethodDeclarationSyntax mds)
        {
            if (mds.Body == null)
                return;

            foreach (var statement in mds.Body.Statements)
            {
                if (statement is LocalDeclarationStatementSyntax)
                {
                    CountModifiers((statement as LocalDeclarationStatementSyntax).Modifiers);
                }
            }
        }

        private static void GetLinesLength(string code, Data data)
        {
            var linesLengths = code.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Length).ToList();
            data.AverageLineLength = linesLengths.Average();
            data.MedianLineLength = linesLengths.Median();
        }

        private static void GetCommentaryContent(string code, Data data)
        {
            var codeLines = code.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder str = new StringBuilder();
            bool isStackedCommentary = false;
            List<int> commentaryLengths = new List<int>();

            for (int i = 0; i < codeLines.Count(); i++)
            {
                if (codeLines[i].StartsWith(@"//"))
                {
                    str.Append(codeLines[i].Substring(2) + " ");

                    if (isStackedCommentary)
                    {
                        commentaryLengths[commentaryLengths.Count - 1] += codeLines[i].Length;
                    }
                    else
                    {
                        isStackedCommentary = true;
                        commentaryLengths.Add(codeLines[i].Length);
                    }
                }
                else
                {
                    if (isStackedCommentary)
                        isStackedCommentary = false;

                    if (codeLines[i].StartsWith(@"/*"))
                    {
                        str.Append(codeLines[i].Substring(2, codeLines[i].Length - 4) + " ");
                        commentaryLengths.Add(codeLines[i].Length);
                    }
                }
            }

            data.CommentaryContent = str.ToString();
            if (commentaryLengths.Count > 0)
            {
                data.AverageCommentaryLength = commentaryLengths.Average();
                data.MedianCommentaryLength = commentaryLengths.Median();
            }
           
        }
    }

    
    class NamesAndLengthData
    {
        public List<int> VariablesNameLengths = new List<int>();
        public List<int> MethodsNameLengths = new List<int>();
        public List<int> MethodParametersAmount = new List<int>();
        public List<int> MethodLengths = new List<int>();

        public List<int> ClassFieldsAmounts = new List<int>();
        public List<int> StructFieldsAmounts = new List<int>();
        public List<int> ClassPropertiesAmounts = new List<int>();
        public List<int> StructPropertiesAmounts = new List<int>();
        public List<int> InterfacePropertiesAmounts = new List<int>();
        public List<int> ClassMethodsAmounts = new List<int>();
        public List<int> StructMethodsAmounts = new List<int>();
        public List<int> InterfaceMethodsAmounts = new List<int>();

        public void AddNewElement()
        {
            VariablesNameLengths.Add(0);
            MethodsNameLengths.Add(0);
            MethodParametersAmount.Add(0);
            MethodLengths.Add(0);
            ClassFieldsAmounts.Add(0);
            StructFieldsAmounts.Add(0);
            ClassPropertiesAmounts.Add(0);
            StructPropertiesAmounts.Add(0);
            InterfacePropertiesAmounts.Add(0);
            ClassMethodsAmounts.Add(0);
            StructMethodsAmounts.Add(0);
            InterfaceMethodsAmounts.Add(0);

        }
    }
}
