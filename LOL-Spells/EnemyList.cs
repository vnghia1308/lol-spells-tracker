using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace LOL_Spells
{
    public partial class EnemyList : Form
    {
        // Enemy data
        private Dictionary<string, string> playerSpells = new Dictionary<string, string>();
        private Dictionary<string, PictureBox> playerChampionBox = new Dictionary<string, PictureBox>();
        private Dictionary<string, PictureBox> playerFirstSpellBox = new Dictionary<string, PictureBox>();
        private Dictionary<string, PictureBox> playerSecondSpellBox = new Dictionary<string, PictureBox>();
        private int playerIndex = 0;

        // special enemy data
        private List<string> playerHasIonianBoots = new List<string>();

        // Spell data
        private List<string> spellOnCooldown = new List<string>();
        private Dictionary<string, int> spellCooldown = new Dictionary<string, int>();

        // Always On Top Setup
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // Free move setup
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        // public states
        public bool isSummonerUnleashedTeleportTime = false;

        public EnemyList()
        {
            InitializeComponent();

            this.BackColor = Color.LimeGreen;
            this.TransparencyKey = Color.LimeGreen;
            this.FormBorderStyle = FormBorderStyle.None;

            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

            // Player 1
            playerChampionBox.Add("1", player1Champion);
            playerFirstSpellBox.Add("1", player1FirstSpell);
            playerSecondSpellBox.Add("1", player1SecondSpell);

            // Player 2
            playerChampionBox.Add("2", player2Champion);
            playerFirstSpellBox.Add("2", player2FirstSpell);
            playerSecondSpellBox.Add("2", player2SecondSpell);

            // Player 3
            playerChampionBox.Add("3", player3Champion);
            playerFirstSpellBox.Add("3", player3FirstSpell);
            playerSecondSpellBox.Add("3", player3SecondSpell);

            // Player 4
            playerChampionBox.Add("4", player4Champion);
            playerFirstSpellBox.Add("4", player4FirstSpell);
            playerSecondSpellBox.Add("4", player4SecondSpell);

            // Player 5
            playerChampionBox.Add("5", player5Champion);
            playerFirstSpellBox.Add("5", player5FirstSpell);
            playerSecondSpellBox.Add("5", player5SecondSpell);

            // Add spell cooldown
            spellCooldown.Add("SummonerBarrier", 180);
            spellCooldown.Add("SummonerBoost", 210);
            spellCooldown.Add("SummonerDot", 180);
            spellCooldown.Add("SummonerExhaust", 210);
            spellCooldown.Add("SummonerFlash", 300);
            spellCooldown.Add("SummonerHaste", 210);
            spellCooldown.Add("SummonerHeal", 240);
            spellCooldown.Add("SummonerMana", 240);
            spellCooldown.Add("SummonerSmite", 15);
            spellCooldown.Add("SummonerSnowball", 80);
            spellCooldown.Add("SummonerTeleport", 360);
            spellCooldown.Add("SummonerUnleashedTeleport", 330);
        }

        public void AddPlayerInfo(string playerName, string championName, string firstSpell, string secondSpell)
        {
            string currentPlayerIndex = (playerIndex + 1).ToString();

            if (playerFirstSpellBox.ContainsKey(currentPlayerIndex))
            {
                playerChampionBox[currentPlayerIndex].ImageLocation = $"https://ddragon.leagueoflegends.com/cdn/14.8.1/img/champion/{championName}.png";
                playerFirstSpellBox[currentPlayerIndex].ImageLocation = $"https://ddragon.leagueoflegends.com/cdn/14.8.1/img/spell/{firstSpell}.png";
                playerSecondSpellBox[currentPlayerIndex].ImageLocation = $"https://ddragon.leagueoflegends.com/cdn/14.8.1/img/spell/{secondSpell}.png";

                playerSpells.Add(playerFirstSpellBox[currentPlayerIndex].Name, firstSpell);
                playerSpells.Add(playerSecondSpellBox[currentPlayerIndex].Name, secondSpell);

                // add player name to picturebox tag
                playerFirstSpellBox[currentPlayerIndex].Tag = playerName;
                playerSecondSpellBox[currentPlayerIndex].Tag = playerName;

                // Add click event
                playerFirstSpellBox[currentPlayerIndex].Click += SummonerSpellClick;
                playerSecondSpellBox[currentPlayerIndex].Click += SummonerSpellClick;
            }

            playerIndex++;
        }

        private void HoldMouseToMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void SummonerSpellClick(object sender, EventArgs e)
        {
            if (sender is PictureBox summonerSpell)
            {
                if (summonerSpell.Image != null)
                {
                    if (spellOnCooldown.Contains(summonerSpell.Name))
                    {
                        return;
                    }

                    string summonerName = (string)summonerSpell.Tag;
                    spellOnCooldown.Add(summonerSpell.Name);

                    Image oImage = summonerSpell.Image;

                    string spellName = playerSpells[summonerSpell.Name];
                    int secondsLeft = spellCooldown[spellName]; // spell cooldown

                    if (playerHasIonianBoots.Contains(summonerName))
                    {
                        secondsLeft -= (int)(secondsLeft * 0.11);
                    }

                    // first draw to keep fast render
                    summonerSpell.Image = DrawUsingSpellWithTime(oImage, secondsLeft);

                    int index = 0;

                    // Initialize the timer
                    Timer timer = new Timer();
                    timer.Interval = 1000; // 1 second interval
                    timer.Tick += (ss, ee) =>
                    {
                        // Decrease the remaining seconds
                        secondsLeft--;

                        // Update the image with the remaining seconds
                        summonerSpell.Image = DrawUsingSpellWithTime(oImage, secondsLeft);

                        // If countdown reaches 0, stop the timer, reset image
                        if (secondsLeft == 0)
                        {
                            timer.Stop();
                            summonerSpell.Image = oImage;
                            spellOnCooldown.Remove(summonerSpell.Name);
                        }
                    };

                    timer.Start();
                }

            }
        }

        private string TimeLeftPars(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString(@"mm\:ss");
        }

        private Image DrawUsingSpellWithTime(Image im, int timeLeft)
        {
            // Create a new bitmap with the same dimensions as the original image
            Bitmap originalBitmap = new Bitmap(im);

            // Create a blank bitmap with the same dimensions and format as the original image
            Bitmap outputImage = new Bitmap(originalBitmap.Width, originalBitmap.Height, PixelFormat.Format32bppArgb);

            // Convert the original image to grayscale
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                            new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                            new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                            new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                            new float[] {0, 0, 0, 1, 0},
                            new float[] {0, 0, 0, 0, 1}
                });

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    graphics.DrawImage(originalBitmap, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height),
                        0, 0, originalBitmap.Width, originalBitmap.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                using (ImageAttributes attributes = new ImageAttributes())
                {
                    attributes.SetColorMatrix(colorMatrix);
                    graphics.DrawImage(originalBitmap, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height),
                        0, 0, originalBitmap.Width, originalBitmap.Height, GraphicsUnit.Pixel, attributes);

                    string defaultText = "00:00";
                    Font font = new Font("Consolas", 15, FontStyle.Bold);
                    SizeF textSize = graphics.MeasureString(defaultText, font);
                    PointF position = new PointF((outputImage.Width - textSize.Width) / 2, (outputImage.Height - textSize.Height) / 2);
                    Brush textBrush = Brushes.White;
                    Brush backgroundBrush = Brushes.Black;
                    graphics.FillRectangle(backgroundBrush, position.X - 5, position.Y - 2, textSize.Width + 10, textSize.Height + 4); // Adjust padding as needed

                    // Draw the text on top of the background rectangle
                    graphics.DrawString(TimeLeftPars(timeLeft), font, textBrush, position);
                }
            }

            originalBitmap.Dispose();

            return outputImage;
        }

        // Handle when player buy Ionian Boots of Lucidity
        public void AddPlayerBuyIonianBoots(string playerName)
        {
            playerHasIonianBoots.Add(playerName);
        }

        public void RemovePlayerBuyIonianBoots(string playerName)
        {
            playerHasIonianBoots.Remove(playerName);
        }

        public bool IsPlayerWasBoughtIonianBoots(string playerName)
        {
            return playerHasIonianBoots.Contains(playerName);
        }

        public void UpgradeSummonerTeleport()
        {
            this.isSummonerUnleashedTeleportTime = true;
            string SummonerUnleashedTeleportUrl = "https://static.wikia.nocookie.net/leagueoflegends/images/7/7c/Unleashed_Teleport.png";

            var playerSpellKeys = playerSpells.Where(pair => pair.Value == "SummonerTeleport").Select(pair => pair.Key);

            foreach (var key in playerSpellKeys)
            {
                var pb = this.Controls.Find(key, true);

                if(pb.Length > 0)
                {
                    if(pb[0] is PictureBox)
                    {
                        ((PictureBox)pb[0]).ImageLocation = SummonerUnleashedTeleportUrl;
                    }
                }

                playerSpells[key] = "SummonerUnleashedTeleport";
            }
        }
    }
}
