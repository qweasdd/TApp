using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ArchiMetrics.Analysis.Metrics;
using ArchiMetrics.Analysis;
using ArchiMetrics.Analysis.Common;
using System.Text.RegularExpressions;

namespace TApp
{
    class Program
    {

        static void Main(string[] args)
        {

            Work();


            //            SyntaxTree tree = CSharpSyntaxTree.ParseText(
            //@"using System;
            //                                    using System.Collections;
            //                                    using System.Linq;
            //                                    using System.Text;

            //                                                            namespace HelloWorld
            //                            {

            //                            enum A { a ,b }
            //                                public class Vova
            //                                {
            //                                    private int a;
            //                                    private int b;
            //                                    public int MyProperty { get; set; }
            //                        object alal;
            //                                    public delegate int TestDelegate(MyType m, long num);
            //                                    public event SampleEventHandler SampleEvent;

            //                                    public static bool Vo(int q)
            //                                    {
            //                                        DateTime a =

            //                                        new DateTime();
            //                                        retrun true;
            //                                    }

            //                                    private static void GetMethodVariabesNames(MethodDeclarationSyntax mds, List<string> names, NamesAndLengthData namesData)
            //        {
            //            foreach (var parametr in mds.ParameterList.Parameters)
            //            {
            //                names.Add(parametr.Identifier.ValueText);
            //            }
            //            namesData.SumMethodParameters[namesData.SumMethodParameters.Count - 1] += mds.ParameterList.Parameters.Count();

            //            if (mds.Body == null)
            //                return;

            //            foreach (var statement in mds.Body.Statements)
            //            {
            //                if (statement is LocalDeclarationStatementSyntax)
            //                {
            //                    var t = statement as LocalDeclarationStatementSyntax;
            //                    foreach (var variable in t.Declaration.Variables)
            //                    {
            //                        names.Add(variable.Identifier.ValueText);
            //                        namesData.VariablesNameLengths[namesData.VariablesNameLengths.Count - 1] += variable.Identifier.ValueText.Length;
            //                    }
            //                }
            //            }
            //        }
            //                                }
            //                                class Program
            //                                {

            //                                    static void Main(string[] args)
            //                                    {
            //                                        int a = 5;
            //                                    }
            //                                }
            //                            }");

            //            var root = (CompilationUnitSyntax)tree.GetRoot();



            //Console.WriteLine(CodeFormat.Format(q));

            //string a = @"
            //using System;
            //using System.Collections.Generic;
            //using System.Linq;
            //using System.Text;
            //using System.Threading.Tasks;
            //                        //vovan
            ////za chto //aaadawdaw


            //namespace TApp
            //{
            //    class Flag
            //    {
            //        public bool IsCompleted { get; set; }

            //        public Flag()
            //        {               /*kak he dalshe zdhit suka pizdec
            //vase                        ploho
            //    vot*/
            //            IsCompleted = false;
            //        }
            //    }
            //}";





            Console.ReadLine();
        }

        private static void Work()
        {
            var context = new DatabaseEntities();
            //Console.WriteLine(context.Downloads.Count());
            //Console.WriteLine(context.Data.Count());

            //var res = context.Downloads.Find(x => x.FContent.Contains("OrderID") && x.FContent.Contains("Microsoft.EntityFremework"));

            //context.Downloads.Remove(res);
            //context.SaveChanges();

            //var t = new GHSearcher(context);
            //t.FindRepos(50);
            //GHDownloader ghd = new GHDownloader(context);
            //ghd.Download();
            DataExtractor.Extract(context);

        }
    }
}



