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

namespace TApp
{
    class Program
    {
        static void Main(string[] args)
        {

            Work();

            //using (StreamReader sr = new StreamReader(@"D:\pr\ConsoleApplication4\ConsoleApplication4\bin\Debug\input.txt", Encoding.GetEncoding(1251)))
            //{

            //    using (StreamWriter sw = new StreamWriter(@"D:\pr\ConsoleApplication4\ConsoleApplication4\bin\Debug\output.txt", false, Encoding.GetEncoding(1251)))
            //    {
            //        sw.WriteLine(CodeFormat.Format(sr.ReadToEnd()));
            //    }
            //}

            Console.ReadLine();

            //using (StreamReader sr = new StreamReader(@"D:\pr\ConsoleApplication4\ConsoleApplication4\bin\Debug\output.txt", Encoding.GetEncoding(1251)))
            //{

            //    var cf = new CodeFile(sr.ReadToEnd());
            //    cf.ParseFile();
            //}
            //Console.ReadLine();

            //var context = new DatabaseEntities();
            //using (StreamWriter sw = new StreamWriter(@"D:\pr\ConsoleApplication4\ConsoleApplication4\bin\Debug\output.txt", false, Encoding.GetEncoding(1251)))
            //{
            //    var a = context.Downloads.Find(646);
            //    sw.WriteLine(a.FContent);
            //}

        }

        private static void Work()
        {
            var context = new DatabaseEntities();
            GHDownloader ghd = new GHDownloader(context);
            ghd.Download();
        }
    }
}



