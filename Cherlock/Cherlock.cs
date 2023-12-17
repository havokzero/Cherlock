using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cherlock_form
{
    public class CherlockClass
    {
        private readonly HttpClient _httpClient;
        private List<SiteInfo> sites;
        private readonly SemaphoreSlim _semaphore;

        private TextBox usernameTextBox;
        private Button searchButton;
        private RichTextBox resultsRichTextBox;
        private bool isSearchInProgress = false;

        // Rename the constructor to match the class name
        public CherlockClass(TextBox textBox, Button button, RichTextBox richTextBox)
        {
            _httpClient = new HttpClient();
            sites = new List<SiteInfo>();
            _semaphore = new SemaphoreSlim(20, 20);

            usernameTextBox = textBox;
            searchButton = button;
            resultsRichTextBox = richTextBox;

            //searchButton.Click += SearchButton_Click;
        }

        private string ConvertToString(dynamic value)
        {
            if (value is JArray arrayValue)
            {
                return string.Join(", ", arrayValue.ToObject<string[]>());
            }
            else if (value is string strValue)
            {
                return strValue.Replace("{}", "{0}");
            }
            return value?.ToString();
        }

         public async Task GetSitesAsync()
         {
             string filePath = "resources.json"; // Ensure this file is in the correct location
             string jsonResponse = File.ReadAllText(filePath);
             var sitesDictionary = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonResponse);

             sites = new List<SiteInfo>();
             foreach (var entry in sitesDictionary)
             {
                 var site = new SiteInfo
                 {
                     Name = entry.Key,
                     Url = entry.Value.url.ToString().Replace("{}", "{0}"),
                     ErrorType = ConvertToString(entry.Value.errorType),
                     ErrorMsg = ConvertToString(entry.Value.errorMsg),
                     UsernameClaimed = ConvertToString(entry.Value.username_claimed)
                 };
                 sites.Add(site);
             }
         }
       
        public async Task SearchForUsernames(string input, RichTextBox resultsRichTextBox, CancellationToken cancellationToken)
        {
            if (isSearchInProgress)
            {
                resultsRichTextBox.AppendText("A search is already in progress.\n");
                return;
            }

            isSearchInProgress = true;

            var usernames = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (usernames.Length == 0)
            {
                resultsRichTextBox.AppendText("No usernames provided.\n");
                isSearchInProgress = false;
                return;
            }

            if (sites == null || sites.Count == 0)
            {
                await GetSitesAsync();
            }

            resultsRichTextBox.AppendText($"Number of sites to search: {sites.Count}\n");

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            foreach (var username in usernames)
            {
                resultsRichTextBox.AppendText($"\nSearching for username: {username}\n");

                stopwatch.Start();

                var tasks = new List<Task<bool>>();
                foreach (var site in sites)
                {
                    tasks.Add(ProcessSiteAsync(site, username, cancellationToken));
                }

                var results = await Task.WhenAll(tasks);
                int resultCount = results.Count(r => r);

                stopwatch.Stop();

                resultsRichTextBox.SelectionColor = System.Drawing.Color.Green;
                resultsRichTextBox.AppendText($"[*] Search completed for '{username}' with {resultCount} results. Duration: {stopwatch.Elapsed:mm\\:ss}\n");
                resultsRichTextBox.SelectionColor = resultsRichTextBox.ForeColor;

                stopwatch.Reset();
            }

            isSearchInProgress = false;
        }

        private async Task<bool> ProcessSiteAsync(SiteInfo site, string username, CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                if (await CheckUsernameAsync(site, username, cancellationToken))
                {
                    // Set color to green for "[+]" and site name
                    resultsRichTextBox.SelectionColor = System.Drawing.Color.Green;
                    resultsRichTextBox.AppendText("[+] " + site.Name + "\n"); // Append "[+]" and site name in green

                    // Reset color for URL
                    resultsRichTextBox.SelectionColor = resultsRichTextBox.ForeColor;
                    resultsRichTextBox.AppendText(site.FormattedUrl + "\n"); // Append URL in normal color

                    return true;
                }
                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<bool> CheckUsernameAsync(SiteInfo site, string username, CancellationToken cancellationToken)
        {
            if (site == null || string.IsNullOrEmpty(site.Url) || !site.Url.Contains("{0}"))
            {
                return false;
            }

            string formattedUrl = string.Format(site.Url, username);
            site.FormattedUrl = formattedUrl;

            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }

                var response = await _httpClient.GetAsync(formattedUrl, cancellationToken);
                var finalUrl = response.RequestMessage.RequestUri.ToString();
                var content = await response.Content.ReadAsStringAsync();

                if (!finalUrl.Contains(username))
                {
                    return false;
                }

                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.OK:
                        if (site.ErrorType == "message" && !string.IsNullOrEmpty(site.ErrorMsg))
                        {
                            return !Regex.IsMatch(content, site.ErrorMsg);
                        }
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private class SiteInfo
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public string ErrorType { get; set; }
            public string ErrorMsg { get; set; }
            public string UsernameClaimed { get; set; }
            public string FormattedUrl { get; set; }
        }
    }
}
