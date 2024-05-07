using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOL_Spells
{
    internal class LeagueHelper
    {
        public static string GetSummonerSpell(JToken player, string spellOrder)
        {
            string spellName = ((string)player["summonerSpells"][spellOrder]["rawDisplayName"]).Split('_')[2];

            if (spellName == "S12")
                spellName = "SummonerTeleport";

            return spellName;
        }

        public static string GetSummonerName(JToken player)
        {
            return (string)player["summonerName"];
        }

        public static string GetSummonerChampion(JToken player)
        {
            return ((string)player["rawChampionName"]).Split('_')[3];
        }

        public static string GetSummonerTeamSide(JArray playerList, string playerName)
        {
            string summonerName = playerName.Split('#')[0];
            foreach (JToken player in playerList)
            {
                if ((string)player["summonerName"] == summonerName)
                {
                    return (string)player["team"];
                }
            }

            return null;
        }

        public static bool IsValidMatch(JArray playerList, string playerName)
        {
            return playerList != null && !string.IsNullOrEmpty(playerName);
        }

        public static bool IsTeam(JToken player, string teamSide)
        {
            return (string)player["team"] == teamSide;
        }

        public static bool NotTeam(JToken player, string teamSide)
        {
            return (string)player["team"] != teamSide;
        }

        public static bool IsPlayerHasItem(JToken player, string itemRawName)
        {
            foreach(var item in player["items"])
            {
                if (((string)item["rawDisplayName"]).StartsWith(itemRawName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
