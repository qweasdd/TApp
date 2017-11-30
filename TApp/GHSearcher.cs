using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;

namespace TApp
{
    class GHSearcher
    {
        private DatabaseEntities dbcontext;
        private GitHubClient client;

        public GHSearcher(DatabaseEntities db)
        {
            client = new GitHubClient(new ProductHeaderValue("test"))
            {
                Credentials = new Credentials(SecretData.Token)
            };
            dbcontext = db;
        }

        public void FindRepos(int amount)
        {
            

            var request = new SearchRepositoriesRequest()
            {
                Language = Octokit.Language.CSharp,
                PerPage = amount
            };

            var repos = client.Search.SearchRepo(request).GetAwaiter().GetResult();

            var results = new List<Sourse>(amount);

            foreach (var res in repos.Items)
            {
                results.Add(new Sourse() { RepositoryID = res.Id, Url = res.HtmlUrl});
            }

            dbcontext.Sourses.AddRange(results);
            dbcontext.SaveChanges();
        }
    }
}
