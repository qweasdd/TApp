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
            
            Console.ReadLine();
        }

        private static void Work()
        {
            var context = new DatabaseEntities();

            var t = new GHSearcher(context);
            t.FindRepos(50);

            GHDownloader ghd = new GHDownloader(context);
            ghd.Download();

            DataExtractor.Extract(context);

        }
    }
}



