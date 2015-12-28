using System.Collections.Generic;
using Newtonsoft.Json;

namespace VKVoteCounter.Engine
{
    public class JSON
    {
        public class AnswerInfo
        {
            [JsonProperty("id")]
            public int ID { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("votes")]
            public int Votes { get; set; }

            [JsonProperty("rate")]
            public double Rate { get; set; }

            public override string ToString()
            {
                return Text + ": " + Votes;
            }
        }

        public class RespAnswers
        {
            [JsonProperty("created")]
            public long Created { get; set; }

            [JsonProperty("question")]
            public string Question { get; set; }

            [JsonProperty("votes")]
            public int Votes { get; set; }

            [JsonProperty(PropertyName = "answers", Required = Required.Default)]
            public IList<AnswerInfo> Answers { get; set; }
        }

        public class RespAnswersGlobal
        {
            [JsonProperty("response")]
            public RespAnswers Response { get; set; }
        }
        
        public class RespVotes
        {
            [JsonProperty("answer_id")]
            public int AnswerID { get; set; }

            [JsonProperty("users")]
            public IList<string> Users { get; set; }

            public override string ToString()
            {
                return Users == null || Users.Count < 1 ? string.Empty : Users.Count.ToString();
            }
        }

        public class RespVotesGlobal
        {
            [JsonProperty("response")]
            public IList<RespVotes> Response { get; set; }
        }

        public class RespGroupMembers
        {
            [JsonProperty("count")]
            public int Count { get; set; }

            [JsonProperty("users")]
            public IList<string> Users { get; set; }
        }

        public class RespGroupMembersGlobal
        {
            [JsonProperty("response")] 
            public RespGroupMembers Response { get; set; }
        }
    }
}
