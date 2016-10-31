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
            client.Credentials = new Credentials("7d98e295f930ae8675211eeb33947a63c6c78548");
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
            SetBagOfContent("/");

            foreach (var item in dbcontext.Downloads)
            {
                if (item.RepositoryID == repId &&
                    dict[lang].Contains(Path.GetExtension(item.Path)) &&
                    !bag.Any(x => x.Path == item.Path))
                {
                    dbcontext.Downloads.Remove(item);
                }
            }

            foreach (var item in bag)
            {
               Dl(item , dbcontext);
            }
        }

        public  void SetBagOfContent(string directory)
        {
            TaskList = new List<Task<IReadOnlyList<RepositoryContent>>>();
            SetBagSupp("/");
            while (!TaskList.All(x => x.IsCompleted))
            { Thread.Sleep(100); }
        }
        
    
        private async void SetBagSupp(string directory)
        {
            var task = client.Repository.Content.GetAllContents(username, repository, directory);
            TaskList.Add(task);
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

        private void Dl(RepositoryContent rc , DatabaseEntities dbcontext)
        {

            Download dl = dbcontext.Downloads.FirstOrDefault(x => x.Path == rc.Path);

                using (var webclient = new WebClient())
                {
                    using (Stream data = webclient.OpenRead(rc.DownloadUrl))
                    {
                        using (StreamReader reader = new StreamReader(data))
                        {
                            string tContent =  reader.ReadToEnd();
                            Console.WriteLine(rc.Path);
                            if (dl == null)
                            {
                                dbcontext.Downloads.Add(new Download()
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
                        }
                    }
               }
            
        }


        private GitHubClient client;
        private Dictionary<Language, string[]> dict;
        private string username;
        private string repository;
        private long repId;
        private List<Task<IReadOnlyList<RepositoryContent>>> TaskList;
        private ConcurrentBag<RepositoryContent> bag;
    }
}