using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.Threading;
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

            Sourse sourse = context.Sourses.First();
            GHDownloader ghd = new GHDownloader();
            ghd.SetRepository(sourse.Url);
            ghd.lang = (Octokit.Language)Enum.Parse(typeof(Octokit.Language), sourse.Language);
            ghd.Download(context);
            context.SaveChanges();
            Console.WriteLine("done");
        }
        
    }
}
