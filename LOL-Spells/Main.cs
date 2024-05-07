using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LOL_Spells
{
    public partial class Main : Form
    {
        private EnemyList enemyList;
        private bool isGameStart = false;
        private Thread mainThread;

        public Main()
        {
            InitializeComponent();

            /**
             * LeagueAPI Library is official League of Legends API
             * Documentation: https://developer.riotgames.com/docs/lol
             **/

            mainThread = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        JArray playerList = LeagueAPI.GetCurrentMatchPlayerList(); // players in current match
                        string currentPlayerName = LeagueAPI.GetCurrentPlayerName(); // current player name

                        // Check match and current player is valid
                        if (LeagueHelper.IsValidMatch(playerList, currentPlayerName))
                        {
                            // get current player team side (ORDER or CHAOS)
                            string currentPlayerTeamSide = LeagueHelper.GetSummonerTeamSide(playerList, currentPlayerName);

                            if (!isGameStart)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    // Create enemy list
                                    enemyList = new EnemyList();

                                    foreach (var player in playerList)
                                    {
                                        if (LeagueHelper.NotTeam(player, currentPlayerTeamSide))
                                        {
                                            // get player info (name, champion, spells)
                                            string pChampion = LeagueHelper.GetSummonerChampion(player);
                                            string pSummonerName = LeagueHelper.GetSummonerName(player);

                                            string pFirstSpell = LeagueHelper.GetSummonerSpell(player, "summonerSpellOne");
                                            string pSecondSpell = LeagueHelper.GetSummonerSpell(player, "summonerSpellTwo");

                                            // add enemy to list
                                            enemyList.AddPlayerInfo(pSummonerName, pChampion, pFirstSpell, pSecondSpell);
                                        }
                                    }

                                    isGameStart = true; // lock for current game
                                    enemyList.Show(); // show enemy list

                                    this.Hide(); // hide main form
                                });
                            }
                            else
                            {
                                if (!enemyList.isSummonerUnleashedTeleportTime)
                                {
                                    var gameStats = LeagueAPI.GetCurrentMatchStats();
                                   
                                    if((float)gameStats["gameTime"] >= 600f)
                                    {
                                        enemyList.UpgradeSummonerTeleport();
                                    }
                                }

                                // update when game is running
                                foreach (var player in playerList)
                                {
                                    if (LeagueHelper.IsTeam(player, currentPlayerTeamSide))
                                    {
                                        // player name
                                        string pSummonerName = LeagueHelper.GetSummonerName(player);

                                        // player spells
                                        string pFirstSpell = LeagueHelper.GetSummonerSpell(player, "summonerSpellOne");
                                        string pSecondSpell = LeagueHelper.GetSummonerSpell(player, "summonerSpellTwo");

                                        if (LeagueHelper.IsPlayerHasItem(player, "Item_3158"))
                                        {
                                            if(enemyList != null)
                                            {
                                                if (!enemyList.IsPlayerWasBoughtIonianBoots(pSummonerName))
                                                {
                                                    enemyList.AddPlayerBuyIonianBoots(pSummonerName);
                                                }  
                                            }
                                        }
                                        else
                                        {
                                            if (enemyList.IsPlayerWasBoughtIonianBoots(pSummonerName))
                                            {
                                                enemyList.RemovePlayerBuyIonianBoots(pSummonerName);
                                            }
                                        }    
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (isGameStart)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    enemyList?.Hide(); // hide enemy list
                                    enemyList = null; // release enemy list

                                    isGameStart = false; // wait for new game
                                    this.Show(); // hide main form
                                });
                            }
                        }
                    } 
                    catch (Exception ex) 
                    {
                        Console.WriteLine(ex.Message);
                    }

                    Thread.Sleep(200);
                }
            });

            mainThread.IsBackground = false;
            mainThread.Start();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mainThread != null && mainThread.IsAlive)
            {
                mainThread.Abort();
            }
        }
    }
}
