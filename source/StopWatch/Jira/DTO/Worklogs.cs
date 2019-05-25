using System.Collections.Generic;

namespace StopWatch
{
    internal class Worklogs
    {
        public int Total { get; set; }
        [RestSharp.Deserializers.DeserializeAs(Name = "worklogs")]
        public List<Worklog> Items { get; set; }
    }

    internal class Author
    {
       public string Name { get; set; }
       public string AccountId { get; set; }
       public string EmailAddress { get; set; }
    }

    internal class Worklog
    {
        public Author Author { get; set; }
        public int TimeSpentSeconds { get; set; }
    }
}
