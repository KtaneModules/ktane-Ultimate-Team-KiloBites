using System.Collections.Generic;
using Newtonsoft.Json;
    public class Contributors
    {
        public List<string> Developer { get; set; }

        [JsonProperty("Twitch Plays")]
        public List<string> TwitchPlays { get; set; }
        public List<string> Maintainer { get; set; }
    }

    public class KtaneModule
    {
        public string Author { get; set; }
        public string Compatibility { get; set; }
        public string Description { get; set; }
        public string ModuleID { get; set; }
        public string Name { get; set; }
        public string Origin { get; set; }
        public string Published { get; set; }
        public List<string> Sheets { get; set; }
        public string SteamID { get; set; }
        public string Symbol { get; set; }
        public List<TutorialVideo> TutorialVideos { get; set; }
        public string Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string DefuserDifficulty { get; set; }
        public string DisplayName { get; set; }
        public string ExpertDifficulty { get; set; }
        public TwitchPlays TwitchPlays { get; set; }
        public Contributors Contributors { get; set; }
        public string RuleSeedSupport { get; set; }
        public TimeMode TimeMode { get; set; }
        public Souvenir Souvenir { get; set; }
        public string SourceUrl { get; set; }
        public string BossStatus { get; set; }
        public List<string> Ignore { get; set; }
        public string MysteryModule { get; set; }
        public string Quirks { get; set; }
        public List<string> IgnoreProcessed { get; set; }
    }

    public class Root
    {
        public List<KtaneModule> KtaneModules { get; set; }
    }

    public class Souvenir
    {
        public string Status { get; set; }
    }

    public class TimeMode
    {
        public double Score { get; set; }
        public string Origin { get; set; }
        public double? ScorePerModule { get; set; }
    }

    public class TutorialVideo
    {
        public string Language { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }

    public class TwitchPlays
    {
        public double Score { get; set; }
        public string ScoreStringDescription { get; set; }
    }

