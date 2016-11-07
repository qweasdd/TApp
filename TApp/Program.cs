using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace TApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Q();
            Work();
            Console.ReadLine();
        }

        private  static void Work()
        {
            var context = new DatabaseEntities();
            GHDownloader ghd = new GHDownloader(context);
            ghd.Download();
         }

        private static void Q()
        {
            var context = new DatabaseEntities();
            List<string> list = new List<string>();
            foreach (var item in context.Sourses.First().Languages)
            {
                var temp = item.Extentions.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                list.AddRange(temp);
            }
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }
        
    }
}
