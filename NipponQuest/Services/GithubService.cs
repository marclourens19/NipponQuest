using Octokit;

namespace NipponQuest.Services
{
    public class GithubService
    {
        private readonly GitHubClient _client;

        public GithubService()
        {
            // The product header identifies your app to the GitHub API
            _client = new GitHubClient(new ProductHeaderValue("NipponQuest-App"));
        }

        public async Task<IReadOnlyList<GitHubCommit>> GetLatestCommitsAsync(string owner, string repo)
        {
            try
            {
                // We only want the most recent 5 quests (commits)
                var request = new ApiOptions { PageCount = 1, PageSize = 5 };
                return await _client.Repository.Commit.GetAll(owner, repo, request);
            }
            catch (Exception)
            {
                // Returns an empty list if the API is down or the repo is private
                return new List<GitHubCommit>();
            }
        }
    }
}