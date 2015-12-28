using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using VKVoteCounter.Engine;
using VKVoteCounter.Resources;
using VKVoteCounter.UI;
using xNet;

namespace VKVoteCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var auth = new Auth();
            auth.ShowDialog();
        }

        private async void GetVotesButton_OnClick(object sender, RoutedEventArgs e)
        {
            ProgramData.GroupMembers.Clear();
            ProgramData.Votes = new Dictionary<JSON.AnswerInfo, List<string>>();
            string tmp;
            var controller = await this.ShowProgressAsync("Loading", "Information loading");
            try
            {
                using (var req = new HttpRequest())
                {
                    req.AddUrlParam("owner_id", ProgramData.UserID);
                    req.AddUrlParam("poll_id", PollIDBox.Text);
                    req.AddUrlParam("is_board", "0");
                    req.AddUrlParam("user_id", ProgramData.UserID);
                    req.AddUrlParam("access_token", ProgramData.Token);
                    var resp =
                        Encoding.Default.GetBytes(
                            req.Get(CStrings.VkApiMethodUrl + CStrings.GetPollAnsversMethodName).ToString());
                    var res = JsonConvert.DeserializeObject<JSON.RespAnswersGlobal>(Encoding.UTF8.GetString(resp));
                    foreach (var answerInfo in res.Response.Answers)
                    {
                        ProgramData.Votes.Add(answerInfo, new List<string>());
                    }
                }
                controller.SetProgress(0.2);
                int offset = 0;
                int count = 1000;
                var answerIDs = ProgramData.Votes.Keys.Aggregate(string.Empty,
                    (current, key) => current + (key.ID.ToString() + ","));
                answerIDs = answerIDs.Substring(0, answerIDs.Length - 1);
                bool isNext;
                do
                {
                    isNext = false;
                    using (var req = new HttpRequest())
                    {
                        req.AddUrlParam("owner_id", ProgramData.UserID);
                        req.AddUrlParam("poll_id", PollIDBox.Text);
                        req.AddUrlParam("answer_ids", answerIDs);
                        req.AddUrlParam("is_board", "0");
                        req.AddUrlParam("friends_only", "0");
                        req.AddUrlParam("offset", offset);
                        req.AddUrlParam("count", count);
                        req.AddUrlParam("user_id", ProgramData.UserID);
                        req.AddUrlParam("access_token", ProgramData.Token);
                        var resp = req.Get(CStrings.VkApiMethodUrl + CStrings.GetPollVotesMethodName).ToString();
                        var res = JsonConvert.DeserializeObject<JSON.RespVotesGlobal>(resp);
                        foreach (var respVotes in res.Response)
                        {
                            var answerInfo = ProgramData.Votes.Keys.Single(k => k.ID.Equals(respVotes.AnswerID));
                            var votes = respVotes.Users.ToList();
                            if (int.Parse(votes[0]) > (offset + count))
                                isNext = true;
                            votes.RemoveAt(0);
                            ProgramData.Votes[answerInfo].AddRange(votes);
                        }
                        if (isNext)
                            offset += count;
                    }
                } while (isNext);
                controller.SetProgress(0.6);
                offset = 0;
                do
                {
                    isNext = false;
                    using (var req = new HttpRequest())
                    {
                        req.AddUrlParam("group_id", GroupIDBox.Text);
                        req.AddUrlParam("sort", "id_asc");
                        req.AddUrlParam("offset", offset);
                        req.AddUrlParam("count", count);
                        var resp = req.Get(CStrings.VkApiMethodUrl + CStrings.GetGroupMembersMethodName).ToString();
                        var res = JsonConvert.DeserializeObject<JSON.RespGroupMembersGlobal>(resp);
                        if (res.Response.Count > (offset + count))
                            isNext = true;
                        ProgramData.GroupMembers.AddRange(res.Response.Users);
                        if (isNext)
                            offset += count;
                    }
                } while (isNext);
                controller.SetProgress(0.8);
                var beforeCount = ProgramData.Votes.Sum(vote => vote.Value.Count);
                var fa = File.Open("Allow.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var far = new StreamWriter(fa);
                var fd = File.Open("Deny.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var fdr = new StreamWriter(fd);
                var fr = File.Open("Result.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                var frr = new StreamWriter(fr);
                tmp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                far.WriteLine(tmp);
                fdr.WriteLine(tmp);
                frr.WriteLine(tmp);
                for (var i = 0; i < ProgramData.Votes.Count; i++)
                {
                    var answerInfo = ProgramData.Votes.ElementAt(i).Key;
                    var denied = new List<string>();
                    for (var j = 0; j < ProgramData.Votes[answerInfo].Count; j++)
                    {
                        if (ProgramData.GroupMembers.Contains(ProgramData.Votes[answerInfo][j])) continue;
                        denied.Add(ProgramData.Votes[answerInfo][j]);
                        ProgramData.Votes[answerInfo].RemoveAt(j);
                        j--;
                    }
                    far.WriteLine(answerInfo.Text + ": (" + ProgramData.Votes[answerInfo].Count + ")");
                    tmp = ProgramData.Votes[answerInfo].Aggregate(string.Empty, (current, vote) => current + vote + ",");
                    tmp = tmp.Substring(0, tmp.Length - 1);
                    far.WriteLine(tmp);
                    fdr.WriteLine(answerInfo.Text + ": (" + denied.Count + ")");
                    tmp = denied.Aggregate(string.Empty, (current, vote) => current + vote + ",");
                    tmp = tmp.Substring(0, tmp.Length - 1);
                    fdr.WriteLine(tmp);
                }
                far.Flush();
                far.Close();
                fa.Close();
                fdr.Flush();
                fdr.Close();
                fd.Close();
                controller.SetProgress(0.9);
                var totalCount = ProgramData.Votes.Sum(vote => vote.Value.Count);
                ResultBox.Text = ProgramData.Votes.Aggregate(string.Empty,
                    (current, vote) =>
                        current +
                        string.Format("{0}: {1:###0}/{2:###0} - {3:#0.00%}\n", vote.Key.Text, vote.Value.Count,
                            vote.Key.Votes, vote.Value.Count / (double) totalCount)) + totalCount + "/" + beforeCount;
                frr.Write(ResultBox.Text);
                frr.Flush();
                frr.Close();
                fr.Close();
                controller.SetProgress(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error", "Error was occured: " + ex.Message);
            }
            await controller.CloseAsync();
        }
    }
}
