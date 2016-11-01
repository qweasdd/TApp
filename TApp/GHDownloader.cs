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
        public Language lang { get; set; }

        public GHDownloader()
        {
            client = new GitHubClient(new ProductHeaderValue("test"));
            client.Credentials = new Credentials("");
            var bf = new BinaryFormatter();
            using (FileStream fs = new FileStream("conf.dat", System.IO.FileMode.Open))
            {
                dict = (Dictionary<Language, string[]>)bf.Deserialize(fs);
            }
            bag = new ConcurrentBag<RepositoryContent>();
        }

        public  void SetRepository(string uri)
        {
            uri = uri.Substring(uri.LastIndexOf("/", uri.LastIndexOf("/") - 1));
            var t = uri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            username = t[0];
            repository = t[1];
            repId = client.Repository.Get(username, repository).GetAwaiter().GetResult().Id;
        }

        public void Download(DatabaseEntities dbcontext)
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

            Dl(dbcontext);
            dbcontext.SaveChanges();
            OneMoreTry(dbcontext);
            dbcontext.SaveChanges();
        }

        public  void SetBagOfContent()
        {
            taskList = new ConcurrentBag<Task<IReadOnlyList<RepositoryContent>>>();
            SetBagSupp("/");
            while (!taskList.All(x => x.IsCompleted))
            { Thread.Sleep(100); }
        }
        
    
        private async void SetBagSupp(string directory)
        {
            var task = client.Repository.Content.GetAllContents(username, repository, directory);
            taskList.Add(task);
            var content = await task;
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
                    SetBagSupp(item.Path);
                }
            }
        }

        private void Dl(DatabaseEntities dbcontext)
        {
            counter = 0;
            taskList1 = new ConcurrentBag<Task<string>>();
            dlList = new ConcurrentBag<Download>();
            retryList = new ConcurrentBag<RepositoryContent>();
            foreach (var item in bag)
            {
                Dlsup(item, dbcontext );
            }
            while (counter < bag.Count && !taskList1.All(x => x.IsCompleted))
            { Thread.Sleep(100);}
            dbcontext.Downloads.AddRange(dlList);
         }

        private async void Dlsup(RepositoryContent rc, DatabaseEntities dbcontext )
        {
           Download dl = dbcontext.Downloads.FirstOrDefault(x => x.Path == rc.Path);
            
            using (var webclient = new WebClient())
            {
                webclient.DownloadStringCompleted += (s, e) => { Console.WriteLine(rc.Path);
                    Console.WriteLine(dlList.Count);
                    Console.WriteLine(taskList1.Count); };
                var task = webclient.DownloadStringTaskAsync(rc.DownloadUrl);
                taskList1.Add(task);
                
                
                string tContent = "";
                try
                {
                    tContent = await task;
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

        private void  OneMoreTry(DatabaseEntities dbcontext)
        {
            counter = 0;
            taskList1 = new ConcurrentBag<Task<string>>();
            dlList = new ConcurrentBag<Download>();
            foreach (var item in retryList)
            {
                Retrysup(item, dbcontext);
            }
            while (counter < retryList.Count && !taskList1.All(x => x.IsCompleted))
            { Thread.Sleep(100); }
            dbcontext.Downloads.AddRange(dlList);
        }

        private async void Retrysup(RepositoryContent rc, DatabaseEntities dbcontext)
        {
            Download dl = dbcontext.Downloads.FirstOrDefault(x => x.Path == rc.Path);

            using (var webclient = new WebClient())
            {
                webclient.DownloadStringCompleted += (s, e) => {
                    Console.WriteLine(rc.Path);
                    Console.WriteLine(dlList.Count);
                    Console.WriteLine(taskList1.Count);
                };
                var task = webclient.DownloadStringTaskAsync(rc.DownloadUrl);
                taskList1.Add(task);


                string tContent = "Downloading failed";
                try
                {
                    tContent = await task;
                }
                catch
                {}


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

        private GitHubClient client;
        private Dictionary<Language, string[]> dict;
        private string username;
        private string repository;
        private long repId;
        private ConcurrentBag<Task<IReadOnlyList<RepositoryContent>>> taskList;
        private ConcurrentBag<RepositoryContent> bag;
        private ConcurrentBag<Task<string>> taskList1;
        private ConcurrentBag<Download> dlList;
        private ConcurrentBag<RepositoryContent> retryList;
        private int counter;
    }
}