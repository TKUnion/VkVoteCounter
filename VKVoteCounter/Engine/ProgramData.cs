using System.Collections.Generic;

namespace VKVoteCounter.Engine
{
    public class ProgramData
    {
        public static string Token { get; set; }
        public static string UserID { get; set; }
        public static Dictionary<JSON.AnswerInfo, List<string>> Votes { get; set; }
        public static List<string> GroupMembers { get; set; } 

        static ProgramData()
        {
            Votes = new Dictionary<JSON.AnswerInfo, List<string>>();
            GroupMembers = new List<string>();
        }
    }
}
