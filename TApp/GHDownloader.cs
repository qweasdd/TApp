using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Octokit;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
namespace TApp
{
    public class GHDownloader
    {
        private GitHubClient client;
        private Dictionary<Language, string[]> dict;
        private string username;
        private string repository;
        private long repId;
        private ConcurrentBag<Flag> flagList;
        private ConcurrentBag<RepositoryContent> bag;
        private ConcurrentBag<Download> dlList;
        private ConcurrentBag<RepositoryContent> retryList;
        private int counter;
        private Language lang;
        private DatabaseEntities dbcontext;

        public GHDownloader(DatabaseEntities dbc)
        {
            client = new GitHubClient(new ProductHeaderValue("test"));
            client.Credentials = new Credentials("cfdf657c42520bc275e43cc53417cf14fa0ee473");
            var bf = new BinaryFormatter();
            using (FileStream fs = new FileStream("conf.dat", System.IO.FileMode.Open))
            {
                dict = (Dictionary<Language, string[]>)bf.Deserialize(fs);
            }
            bag = new ConcurrentBag<RepositoryContent>();
            dbcontext = dbc;
        }
        
        public void Download()
        {
            foreach (Sourse sourse in dbcontext.Sourses)
            {
                SetRepository(sourse.Url);
                lang = (Octokit.Language)Enum.Parse(typeof(Octokit.Language), sourse.Language);
                RepositoryDownload();
            }
            dbcontext.SaveChanges();
            Console.WriteLine("The End");
        }

        private void SetRepository(string uri)
        {
            uri = uri.TrimEnd('/');
            uri = uri.Substring(uri.LastIndexOf("/", uri.LastIndexOf("/") - 1));
            var t = uri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            username = t[0];
            repository = t[1];
            repId = client.Repository.Get(username, repository).GetAwaiter().GetResult().Id;
        }

        private void RepositoryDownload()
        {
            bag = new ConcurrentBag<RepositoryContent>();
            SetBagOfContent();

            foreach (var item in dbcontext.Downloads)
            {
                if (item.RepositoryID == repId &&
                    dict[lang].Contains(Path.GetExtension(item.Path)) &&
                    !bag.Any(x => x.Path == item.Path))
                {
                    dbcontext.Downloads.Remove(item);
                }
            }

            DownloadContent();
            Console.WriteLine($"{retryList.Count} failed");
            RetryDownloadContent();
            Console.WriteLine($"{username}/{repository} downloaded");
        }

        public void SetBagOfContent()
        {
            flagList = new ConcurrentBag<Flag>();
            SBCHelper("/");

            while (!flagList.All(x => x.IsCompleted))
            { Thread.Sleep(1000); }

        }

        private async void SBCHelper(string directory)
        {
            Flag flag = new Flag();
            flagList.Add(flag);
            var content = await client.Repository.Content.GetAllContents(username, repository, directory);
            foreach (var item in content)
            {
                if (item.Type == ContentType.File && item.DownloadUrl != null
                    && dict[lang].Contains(Path.GetExtension(item.DownloadUrl.ToString()).ToLower()))
                {
                    bag.Add(item);
                }
                else
                if (item.Type == ContentType.Dir)
                {
                    SBCHelper(item.Path);
                }
            }
            flag.IsCompleted = true;
        }
        
        private void DownloadContent()
        {
            counter = 1;
            dlList = new ConcurrentBag<Download>();
            retryList = new ConcurrentBag<RepositoryContent>();
            foreach (var item in bag)
            {
                DCHelper(item);
            }
            while (counter < bag.Count + 1)
            { Thread.Sleep(100); }
            dbcontext.Downloads.AddRange(dlList);
        }

        private async void DCHelper(RepositoryContent rc)
        {
            Download dl = dbcontext.Downloads.FirstOrDefault(x => x.Path == rc.Path);

            using (var webclient = new WebClient())
            {
                webclient.DownloadStringCompleted += (s, e) =>
                {
                    Console.WriteLine(rc.Path);
                    Console.WriteLine($"{counter} / {bag.Count}");
                };

                string tContent = "";
                try
                {
                    tContent = await webclient.DownloadStringTaskAsync(rc.DownloadUrl);
                }
                catch
                {
                    retryList.Add(rc);
                    counter++;
                    return;
                }


                if (dl == null)
                {
                    dlList.Add(new Download()
                    {
                        RepositoryID = repId,
                        Path = rc.Path,
                        Content = tContent
                    });
                }
                else
                {
                    if (dl.Content != tContent) dl.Content = tContent;
                }
                counter++;
            }
        }

        private void RetryDownloadContent()
        {
            counter = 1;

            dlList = new ConcurrentBag<Download>();
            foreach (var item in retryList)
            {
                RDCHelper(item);
            }
            while (counter < retryList.Count + 1)
            { Thread.Sleep(100); }
            dbcontext.Downloads.AddRange(dlList);
        }

        private async void RDCHelper(RepositoryContent rc)
        {
            Download dl = dbcontext.Downloads.FirstOrDefault(x => x.Path == rc.Path);

            using (var webclient = new WebClient())
            {
                webclient.DownloadStringCompleted += (s, e) => {
                    Console.WriteLine(rc.Path);
                    Console.WriteLine($"{counter} / {retryList.Count}");
                };



                string tContent = "Downloading failed";
                try
                {
                    tContent = await webclient.DownloadStringTaskAsync(rc.DownloadUrl); ;
                }
                catch
                { }


                if (dl == null)
                {

                    dlList.Add(new Download()
                    {
                        RepositoryID = repId,
                        Path = rc.Path,
                        Content = tContent
                    });

                }
                else
                {
                    if (dl.Content != tContent) dl.Content = tContent;
                }
                counter++;
            }
        }

    }
}