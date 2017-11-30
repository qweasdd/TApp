using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Octokit;
using System.Net;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace TApp
{
    public class GHDownloader
    {
        private GitHubClient client;
        private string username;
        private string repository;
        private List<string> extentionsList;
        private long repId;
        private ConcurrentBag<Flag> flagList;
        private ConcurrentBag<RepositoryContent> bag;
        private ConcurrentBag<Download> dlList;
        private ConcurrentBag<RepositoryContent> retryList;
        private int counter;
        private DatabaseEntities dbcontext;
        private bool isLimitExceeded;

        public GHDownloader(DatabaseEntities dbc)
        {
            client = new GitHubClient(new ProductHeaderValue("test"))
            {
                Credentials = new Credentials(SecretData.Token)
            };
            bag = new ConcurrentBag<RepositoryContent>();
            dbcontext = dbc;

            extentionsList = new List<string>() { ".cs" };
            isLimitExceeded = false;
        }

        public void Download()
        {
            foreach (Sourse sourse in dbcontext.Sourses.ToList())
            {
                
                SetRepository(sourse.Url);

                if (isLimitExceeded == true)
                {
                    break;
                }

                RepositoryDownload();
            }

            DownloadsFormat();
            Console.WriteLine("The End");
        }

        private void SetRepository(string uri)
        {
            uri = uri.TrimEnd('/');
            uri = uri.Substring(uri.LastIndexOf("/", uri.LastIndexOf("/") - 1));
            var t = uri.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            username = t[0];
            repository = t[1];
            try
            {
                repId = client.Repository.Get(username, repository).GetAwaiter().GetResult().Id;
            }
            catch (Exception e)
            {
                if (e is RateLimitExceededException)
                {
                    isLimitExceeded = true;
                    return;
                }
            }
        }

        private void RepositoryDownload()
        {
            Console.WriteLine($"{username}/{repository} is starting download");
            bag = new ConcurrentBag<RepositoryContent>();
            SetBagOfContent();

            foreach (var item in dbcontext.Downloads)
            {
                if (item.RepositoryID == repId &&
                    extentionsList.Contains(Path.GetExtension(item.Path)) &&
                    !bag.Any(x => x.Path == item.Path))
                {
                    dbcontext.Downloads.Remove(item);
                }
            }

            DownloadContent();
            Console.WriteLine($"{retryList.Count} failed");
            dbcontext.SaveChanges();
            RetryDownloadContent();
            dbcontext.SaveChanges();
            Console.WriteLine($"{username}/{repository} downloaded");
         }

        public void SetBagOfContent()
        {
            flagList = new ConcurrentBag<Flag>();
            if (!isLimitExceeded)
            {
                SBCHelper("/");
            }

            while (!flagList.All(x => x.IsCompleted))
            { Thread.Sleep(1000); }

        }

        private async void SBCHelper(string directory)
        {
            Flag flag = new Flag();
            flagList.Add(flag);
            try
            {
                var content = await client.Repository.Content.GetAllContents(username, repository, directory);
                foreach (var item in content)
                {
                    if (item.Type == ContentType.File && item.DownloadUrl != null &&
                        extentionsList.Contains(Path.GetExtension(item.DownloadUrl.ToString()).ToLower()))
                    {
                        bag.Add(item);
                    }
                    else
                    if (!isLimitExceeded && item.Type == ContentType.Dir)
                    {
                        SBCHelper(item.Path);
                    }
                }
            }

            catch (Exception e)
            {
                if (e is RateLimitExceededException)
                {
                    isLimitExceeded = true;
                    flag.IsCompleted = true;
                    return;
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
            {
                Thread.Sleep(100);
            }

            dbcontext.Downloads.AddRange(dlList);
        }

        private async void DCHelper(RepositoryContent rc)
        {
            var dl = dbcontext.Downloads.FirstOrDefault(x => x.Path == rc.Path);

            using (var webclient = new WebClient())
            {
                webclient.DownloadStringCompleted += (s, e) =>
                {
                    Console.WriteLine(rc.Path);
                    Console.WriteLine($"{counter} / {bag.Count}");
                };

                var tContent = string.Empty;
                try
                {
                    tContent = await webclient.DownloadStringTaskAsync(rc.DownloadUrl);
                }
                catch
                {
                    retryList.Add(rc);
                    Interlocked.Increment(ref counter);
                    return;
                }

                if (tContent != string.Empty)
                {
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
                }
                Interlocked.Increment(ref counter);
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
            var dl = dbcontext.Downloads.FirstOrDefault(x => x.Path == rc.Path);

            using (var webclient = new WebClient())
            {
                webclient.DownloadStringCompleted += (s, e) => {
                    Console.WriteLine(rc.Path);
                    Console.WriteLine($"{counter} / {retryList.Count}");
                };



                var tContent = "Downloading failed";
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
                Interlocked.Increment(ref counter);
            }
        }


        

        private void DownloadsFormat() 
        {
            foreach (var item in dbcontext.Downloads)
            {
                if (item.FContent is null)
                {
                    item.FContent = CodeFormat.Format(item.Content);
                    Console.WriteLine(item.FContent);
                }
            }
            

            dbcontext.SaveChanges();
        }


    }
}
