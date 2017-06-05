﻿using System;
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

            Console.ReadLine();
        }

        private static void Work()
        {
            var context = new DatabaseEntities();
            GHDownloader ghd = new GHDownloader(context);
            ghd.Download();
        }
    }
}



