using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace SimpleClient
{
    public partial class GameWindow : Form
    {
        private List<Bitmap> tileImages;
        private List<Bitmap> bombImages;
        private List<Bitmap> characterImages;

        private Player myPlayer;
        private Player otherPlayer;

        private List<Bomb> bombs;
        private List<PacketData.BombPacketData> bombsSinceLastPacket;
        private List<Rectangle> debugBoxes;
        private Pen redPen = new Pen(Color.Red, 1);

        private System.Media.SoundPlayer music;

        private Random RNG;

        private SimpleClient client;

        private bool isPlayerOne;

        private int packetSendDelay;

        private int[,] defaultMapData = new int[15, 17] { 
            { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 },
            { 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4 },
            { 4, 2, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 2, 4 },
            { 4, 2, 0, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 4 },
            { 4, 2, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 2, 4 },
            { 4, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 4 },
            { 4, 2, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 1, 2, 4 },
            { 4, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 4 },
            { 4, 2, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 2, 4 },
            { 4, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 4 },
            { 4, 2, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 2, 4 },
            { 4, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 0, 2, 4 },
            { 4, 2, 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 2, 4 },
            { 4, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4 },
            { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 }
        };

        private TileData[,] currentMapData = new TileData[15, 17];

        public GameWindow(SimpleClient client, bool isPlayerOne)
        {
            InitializeComponent();

            this.client = client;
            this.isPlayerOne = isPlayerOne;

            string mainFolder = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\bomberman_images\";
            string characterFolder = mainFolder + @"character\";

            //Character images
            characterImages = new List<Bitmap>();

            for (int i = 0; i < 2; i++)
            {
                string colourFolder = "";
                switch (i)
                {
                    case 0:
                        colourFolder = @"white\";
                        break;
                    case 1:
                        colourFolder = @"red\";
                        break;
                }
                
                //Left
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"left_idle.png"));          //0
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"left_step_left.png"));     //1
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"left_step_right.png"));    //2

                //Right
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"right_idle.png"));         //3
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"right_step_left.png"));    //4
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"right_step_right.png"));   //5

                //Back
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"back_idle.png"));          //6
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"back_step_left.png"));     //7
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"back_step_right.png"));    //8

                //Front
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"front_idle.png"));         //9
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"front_step_left.png"));    //10
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"front_step_right.png"));   //11

                //Falling
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"falling_0.png"));          //12
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"falling_1.png"));          //13
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"falling_2.png"));          //14
                characterImages.Add(new Bitmap(characterFolder + colourFolder + @"falling_3.png"));          //15
            }
            

            //Bomb images
            bombImages = new List<Bitmap>();

            bombImages.Add(new Bitmap(mainFolder + @"bomb_gray_large.png"));                  //0
            bombImages.Add(new Bitmap(mainFolder + @"bomb_gray_normal.png"));                 //1
            bombImages.Add(new Bitmap(mainFolder + @"bomb_gray_small.png"));                  //2

            bombImages.Add(new Bitmap(mainFolder + @"bomb_red_large.png"));                   //3
            bombImages.Add(new Bitmap(mainFolder + @"bomb_red_normal.png"));                  //4
            bombImages.Add(new Bitmap(mainFolder + @"bomb_red_small.png"));                   //5

            //Tile images
            tileImages = new List<Bitmap>();

            tileImages.Add(new Bitmap(mainFolder + @"green_back.jpg"));                       //0
            tileImages.Add(new Bitmap(mainFolder + @"brick.jpg"));                            //1
            tileImages.Add(new Bitmap(mainFolder + @"block.jpg"));                            //2
            tileImages.Add(new Bitmap(mainFolder + @"green_back_shadow.jpg"));                //3
            tileImages.Add(new Bitmap(mainFolder + @"gray_back.jpg"));                        //4

            //Explosion tile images
            tileImages.Add(new Bitmap(mainFolder + @"explosion_centre_0.jpg"));               //5
            tileImages.Add(new Bitmap(mainFolder + @"explosion_centre_1.jpg"));               //6
            tileImages.Add(new Bitmap(mainFolder + @"explosion_centre_2.jpg"));               //7
            tileImages.Add(new Bitmap(mainFolder + @"explosion_centre_3.jpg"));               //8
            tileImages.Add(new Bitmap(mainFolder + @"explosion_centre_4.jpg"));               //9

            tileImages.Add(new Bitmap(mainFolder + @"explosion_vertical_centre_0.jpg"));      //10
            tileImages.Add(new Bitmap(mainFolder + @"explosion_vertical_centre_1.jpg"));      //11
            tileImages.Add(new Bitmap(mainFolder + @"explosion_vertical_centre_2.jpg"));      //12
            tileImages.Add(new Bitmap(mainFolder + @"explosion_vertical_centre_3.jpg"));      //13
            tileImages.Add(new Bitmap(mainFolder + @"explosion_vertical_centre_4.jpg"));      //14

            tileImages.Add(new Bitmap(mainFolder + @"explosion_horizontal_centre_0.jpg"));    //15
            tileImages.Add(new Bitmap(mainFolder + @"explosion_horizontal_centre_1.jpg"));    //16
            tileImages.Add(new Bitmap(mainFolder + @"explosion_horizontal_centre_2.jpg"));    //17
            tileImages.Add(new Bitmap(mainFolder + @"explosion_horizontal_centre_3.jpg"));    //18
            tileImages.Add(new Bitmap(mainFolder + @"explosion_horizontal_centre_3.jpg"));    //19

            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_left_0.jpg"));             //20
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_left_1.jpg"));             //21
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_left_2.jpg"));             //22
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_left_3.jpg"));             //23
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_left_4.jpg"));             //24

            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_right_0.jpg"));            //25
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_right_1.jpg"));            //26
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_right_2.jpg"));            //27
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_right_3.jpg"));            //28
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_right_4.jpg"));            //29

            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_top_0.jpg"));              //30
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_top_1.jpg"));              //31
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_top_2.jpg"));              //32
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_top_3.jpg"));              //33
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_top_4.jpg"));              //34
                                                                                      
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_bot_0.jpg"));              //35
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_bot_1.jpg"));              //36
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_bot_2.jpg"));              //37
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_bot_3.jpg"));              //38
            tileImages.Add(new Bitmap(mainFolder + @"explosion_end_bot_4.jpg"));              //39

            //Brick fire
            tileImages.Add(new Bitmap(mainFolder + @"brick_fire_0.jpg"));                     //40
            tileImages.Add(new Bitmap(mainFolder + @"brick_fire_1.jpg"));                     //41
            tileImages.Add(new Bitmap(mainFolder + @"brick_fire_2.jpg"));                     //42
            tileImages.Add(new Bitmap(mainFolder + @"brick_fire_3.jpg"));                     //43
            tileImages.Add(new Bitmap(mainFolder + @"brick_fire_4.jpg"));                     //44
            tileImages.Add(new Bitmap(mainFolder + @"brick_fire_5.jpg"));                     //45

            //Powerups
            tileImages.Add(new Bitmap(mainFolder + @"powerup_fire.png"));                     //46
            tileImages.Add(new Bitmap(mainFolder + @"powerup_fire_down.png"));                //47
            tileImages.Add(new Bitmap(mainFolder + @"powerup_bomb.png"));                     //48
            tileImages.Add(new Bitmap(mainFolder + @"powerup_bomb_down.png"));                //49
            tileImages.Add(new Bitmap(mainFolder + @"powerup_skate.png"));                    //50
            tileImages.Add(new Bitmap(mainFolder + @"powerup_geta.png"));                     //51
            tileImages.Add(new Bitmap(mainFolder + @"powerup_red_bomb.png"));                 //52
        }

        private void GameWindow_Load(object sender, EventArgs e)
        {
            Select();

            packetSendDelay = 20;
            RNG = new Random();
            debugBoxes = new List<Rectangle>();

            if (isPlayerOne)
            {
                myPlayer = new Player(80.0f, 80.0f, PlayerColour.WHITE, PlayerDirection.DOWN);
                otherPlayer = new Player(464.0f, 400.0f, PlayerColour.RED, PlayerDirection.DOWN);
            }
            else
            {
                myPlayer = new Player(464.0f, 400.0f, PlayerColour.RED, PlayerDirection.DOWN);
                otherPlayer = new Player(80.0f, 80.0f, PlayerColour.WHITE, PlayerDirection.DOWN);
            }

            bombs = new List<Bomb>();
            bombsSinceLastPacket = new List<PacketData.BombPacketData>();

            InitializeCurrentMapData();

            string filepath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + @"\bomberman_ds_battle.wav";
            music = new System.Media.SoundPlayer(filepath);

            music.PlayLooping();
        }

        private void InitializeCurrentMapData()
        {
            for (int j = 0; j < 15; j++)
            {
                for (int i = 0; i < 17; i++)
                {
                    currentMapData[j,i].value = defaultMapData[j, i];
                    currentMapData[j, i].age = 0;
                }
            }
        }

        private int PickRandomPowerUp()
        {
            int rnum = RNG.Next(1000);
            if (rnum < 200)
            {
                return 46; //fire
            }
            else if (rnum < 400)
            {
                return 48;//bomb
            }
            else if (rnum < 600)
            {
                return 50;//skates
            }
            else if (rnum < 700)
            {
                return 47;//fire down
            }
            else if (rnum < 800)
            {
                return 49;//bomb down
            }
            else if (rnum < 900)
            {
                return 51;//geta
            }
            else
            {
                return 52;//red bomb
            }
        }

        private int UpdateTile(int value, int x, int y)
        {
            if (value > 4 && value < 46)
            { 
                if (value != 9 && value != 14 && value != 19 && value != 24 && value != 29 && value != 34 && value != 39 && value != 45)
                {
                    return value + 1;
                }
                else
                {
                    if (value == 45)
                    {
                        SeedRandomAt(x, y);
                        if (RNG.Next(1000) > 650)
                        {
                            return PickRandomPowerUp();
                        }
                    }
                    return 0;
                }
            }
            return value;
        }

        private void SeedRandomAt(int x, int y)
        {
            RNG = new Random(x + y);
        }

        private void UpdateCurrentMapData()
        {
            for (int j = 2; j < 13; j++)
            {
                for (int i = 2; i < 15; i++)
                {
                    currentMapData[j, i].age++;
                    if (currentMapData[j, i].age > 20)
                    {
                        int newValue = UpdateTile(currentMapData[j, i].value, i , j);
                        if (newValue != currentMapData[j, i].value)
                        {
                            currentMapData[j, i].age = 0;
                            currentMapData[j, i].value = newValue;
                        }
                    }
                }
            }
        }

        public void UpdateGame()
        {
            UpdateBombs();
            UpdateCurrentMapData();
            myPlayer.Update();
            otherPlayer.Update();
            debugBoxes.Clear();
            CheckPlayerCollision(myPlayer);
            CheckPlayerCollision(otherPlayer);

            packetSendDelay--;
            if (packetSendDelay == 0)
            {
                packetSendDelay = 5;
                PacketData.PlayerPacketData playerPacketData = new PacketData.PlayerPacketData();
                playerPacketData.direction = (int)myPlayer.GetDirection();
                playerPacketData.moveSpeed = myPlayer.GetMoveSpeed();
                playerPacketData.moving = myPlayer.GetMoving();
                playerPacketData.positionX = myPlayer.GetPosition().X;
                playerPacketData.positionY = myPlayer.GetPosition().Y;
                PacketData.BombermanClientToServerPacket packet = new PacketData.BombermanClientToServerPacket(bombsSinceLastPacket, playerPacketData);
                client.TCPSendPacket(packet);
                bombsSinceLastPacket.Clear();
            }
        }

        public void UpdateOtherPlayerWithPacket(PacketData.PlayerPacketData playerPacketData)
        {

            otherPlayer.SetPositionX(playerPacketData.positionX);
            otherPlayer.SetPositionY(playerPacketData.positionY);
            otherPlayer.SetMoveSpeed(playerPacketData.moveSpeed);
            otherPlayer.SetMoving(playerPacketData.moving);
            otherPlayer.SetDirection((PlayerDirection)playerPacketData.direction);
        }


        public void AddOtherPlayersBombs(List<PacketData.BombPacketData> otherPlayerBombs)
        {
            for (int i = 0; i < otherPlayerBombs.Count; i++)
            {
                CreateBomb(otherPlayerBombs[i].gridPosX, otherPlayerBombs[i].gridPosY, otherPlayerBombs[i].size, (BombType)otherPlayerBombs[i].type, otherPlayer);
            }
        }

        private void CheckPlayerCollision(Player player)
        {
            if (player.GetFalling()) return;

            int gridPosX = (int)Math.Floor(player.GetPosition().X / 32.0f);
            int gridPosY = (int)Math.Floor(player.GetPosition().Y / 32.0f);

            List<Point> collisionPoints = new List<Point>();

            bool dead = false;

            for (int i = gridPosX - 1; i <= gridPosX + 1; i++)
            {
                if (i > -1 && i < 17)
                {
                    for (int j = gridPosY - 1; j <= gridPosY + 1; j++)
                    {
                        if (j > -1 && j < 15)
                        {
                            //Touching explosion
                            if (currentMapData[j, i].value >= 5 && currentMapData[j, i].value <= 39)
                            {
                                if (player.GetHitBox().IntersectsWith(new Rectangle(i * 32, j * 32, 32, 32)))
                                {
                                    dead = true;
                                }
                            }

                            //Touching powerup
                            if (currentMapData[j, i].value >= 46)
                            {
                                if (player.GetHitBox().IntersectsWith(new Rectangle(i * 32, j * 32, 32, 32)))
                                {
                                    switch (currentMapData[j, i].value)
                                    {
                                        case 46:
                                            player.ApplyPowerup(PlayerPowerup.FIRE);
                                            break;
                                        case 47:
                                            player.ApplyPowerup(PlayerPowerup.FIRE_DOWN);
                                            break;
                                        case 48:
                                            player.ApplyPowerup(PlayerPowerup.BOMB);
                                            break;
                                        case 49:
                                            player.ApplyPowerup(PlayerPowerup.BOMB_DOWN);
                                            break;
                                        case 50:
                                            player.ApplyPowerup(PlayerPowerup.SKATES);
                                            break;
                                        case 51:
                                            player.ApplyPowerup(PlayerPowerup.GETA);
                                            break;
                                        case 52:
                                            player.ApplyPowerup(PlayerPowerup.RED_BOMB);
                                            break;
                                    }
                                    currentMapData[j, i].value = 0;
                                }
                            }

                            //touching wall
                            if (currentMapData[j,i].value == 1 || currentMapData[j,i].value == 2)
                            {
                                debugBoxes.Add(new Rectangle(i * 32, j * 32, 32, 32));
                                if (player.GetHitBox().IntersectsWith(new Rectangle(i * 32, j * 32, 32, 32)))
                                {
                                    collisionPoints.Add(new Point(i * 32 + 16, j * 32 + 16));
                                }
                            }
                        }
                    }
                }
            }

            if (collisionPoints.Count > 0)
            {
                float closestDist = 4096.0f;
                int closestIndex = -1;
                double closestDiffX = 0;
                double closestDiffY = 0;
                for (int i = 0; i < collisionPoints.Count; i++)
                {
                    double diffX = player.GetPosition().X - collisionPoints[i].X;
                    double diffY = player.GetPosition().Y - collisionPoints[i].Y;

                    float dist = (float)Math.Sqrt(Math.Pow(diffX, 2) + Math.Pow(diffY, 2));

                    if (dist < closestDist)
                    {
                        closestIndex = i;
                        closestDist = dist;
                        closestDiffX = diffX;
                        closestDiffY = diffY;
                    }
                }

                Point closestPoint = collisionPoints[closestIndex];

                if (Math.Abs(closestDiffX) > Math.Abs(closestDiffY))// horizontal or vertical
                {
                    if (closestDiffX < 0)//left or right
                    {
                        player.SetPositionX(closestPoint.X - 22);
                    }
                    else
                    {
                        player.SetPositionX(closestPoint.X + 22);
                    }
                }
                else
                {
                    if (closestDiffY < 0)//up or down
                    {
                        player.SetPositionY(closestPoint.Y - 22);
                    }
                    else
                    {
                        player.SetPositionY(closestPoint.Y + 22);
                    }
                }
            }

            player.UpdateHitbox();

            if (dead)
            {
                player.FallOver();
            }
        }
        private void DrawBomb(PaintEventArgs e, Bomb bomb)
        {
            if (bomb.GetBombType() == BombType.GRAY)
            {
                e.Graphics.DrawImage(bombImages[bomb.GetAnimationFrame()], new Rectangle(bomb.GetGridX() * 32, bomb.GetGridY() * 32, 32, 32), 0, 0, 16, 16, GraphicsUnit.Pixel);
            }
            else
            {
                e.Graphics.DrawImage(bombImages[3 + bomb.GetAnimationFrame()], new Rectangle(bomb.GetGridX() * 32, bomb.GetGridY() * 32, 32, 32), 0, 0, 16, 16, GraphicsUnit.Pixel);
            }
        }

        private void UpdateBombs()
        {
            List<int> toRemove = new List<int>();
            for (int i = 0; i < bombs.Count; i++)
            {
                bombs[i].Update();
                if (bombs[i].GetBombState() == BombState.EXPLODED)
                {
                    CreateExplosion(bombs[i].GetGridX(), bombs[i].GetGridY(), bombs[i].GetSize(), bombs[i].GetBombType());
                    toRemove.Add(i);
                }
            }

            for (int i = toRemove.Count - 1; i >= 0; i--)
            {
                bombs.RemoveAt(toRemove[i]);
            }
        }

        public void CreateExplosion(int gridPosX, int gridPosY, int size, BombType type)
        {
            currentMapData[gridPosY, gridPosX].value = 5;
            currentMapData[gridPosY, gridPosX].age = 0;

            int tilesDestroyed = 0;
            int tilesCanDestroy = 1;
            if (type == BombType.RED) tilesCanDestroy = 2;
            
            //right
            for (int i = 1; i < size + 1; i++)
            {
                if (currentMapData[gridPosY, gridPosX + i].value == 0 || currentMapData[gridPosY, gridPosX + i].value == 3 || currentMapData[gridPosY, gridPosX + i].value >= 46)
                {
                    if (i == size)
                    {
                        currentMapData[gridPosY, gridPosX + i].value = 25;
                    }
                    else
                    {
                        currentMapData[gridPosY, gridPosX + i].value = 15;

                    }
                    currentMapData[gridPosY, gridPosX + i].age = 0;
                }
                else if (currentMapData[gridPosY, gridPosX + i].value == 1)
                {
                    currentMapData[gridPosY, gridPosX + i].value = 40;
                    currentMapData[gridPosY, gridPosX + i].age = 0;
                    tilesDestroyed++;
                }
                else if (currentMapData[gridPosY, gridPosX + i].value == 2)
                {
                    break;
                }
                if (tilesDestroyed >= tilesCanDestroy) break;
            }

            tilesDestroyed = 0;

            //left
            for (int i = 1; i < size + 1; i++)
            {
                if (currentMapData[gridPosY, gridPosX - i].value == 0 || currentMapData[gridPosY, gridPosX - i].value == 3 || currentMapData[gridPosY, gridPosX - i].value >= 46)
                {
                    if (i == size)
                    {
                        currentMapData[gridPosY, gridPosX - i].value = 20;
                    }
                    else
                    {
                        currentMapData[gridPosY, gridPosX - i].value = 15;
                    }
                    currentMapData[gridPosY, gridPosX - i].age = 0;
                }
                else if (currentMapData[gridPosY, gridPosX - i].value == 1)
                {
                    currentMapData[gridPosY, gridPosX - i].value = 40;
                    currentMapData[gridPosY, gridPosX - i].age = 0;
                    tilesDestroyed++;
                }
                else if (currentMapData[gridPosY, gridPosX - i].value == 2)
                {
                    break;
                }
                if (tilesDestroyed >= tilesCanDestroy) break;
            }

            tilesDestroyed = 0;

            //down
            for (int i = 1; i < size + 1; i++)
            {
                if (currentMapData[gridPosY + i, gridPosX].value == 0 || currentMapData[gridPosY + i, gridPosX].value == 3 || currentMapData[gridPosY + i, gridPosX].value >= 46)
                {
                    if (i == size)
                    {
                        currentMapData[gridPosY + i, gridPosX].value = 35;
                    }
                    else
                    {
                        currentMapData[gridPosY + i, gridPosX].value = 10;
                    }
                    currentMapData[gridPosY + i, gridPosX].age = 0;
                }
                else if (currentMapData[gridPosY + i, gridPosX].value == 1)
                {
                    currentMapData[gridPosY + i, gridPosX].value = 40;
                    currentMapData[gridPosY + i, gridPosX].age = 0;
                    tilesDestroyed++;
                }
                else if (currentMapData[gridPosY + i, gridPosX].value == 2)
                {
                    break;
                }
                if (tilesDestroyed >= tilesCanDestroy) break;
            }

            tilesDestroyed = 0;

            //up
            for (int i = 1; i < size + 1; i++)
            {
                if (currentMapData[gridPosY - i, gridPosX].value == 0 || currentMapData[gridPosY - i, gridPosX].value == 3 || currentMapData[gridPosY - i, gridPosX].value >= 46)
                {
                    if (i == size)
                    {
                        currentMapData[gridPosY - i, gridPosX].value = 30;
                    }
                    else
                    {
                        currentMapData[gridPosY - i, gridPosX].value = 10;
                    }
                    currentMapData[gridPosY - i, gridPosX].age = 0;
                }
                else if (currentMapData[gridPosY - i, gridPosX].value == 1)
                {
                    currentMapData[gridPosY - i, gridPosX].value = 40;
                    currentMapData[gridPosY - i, gridPosX].age = 0;
                    tilesDestroyed++;
                }
                else if (currentMapData[gridPosY - i, gridPosX].value == 2)
                {
                    break;
                }
                if (tilesDestroyed >= tilesCanDestroy) break;
            }
        }

        private void DrawPlayer(PaintEventArgs e, Player player)
        {
            int playerX = (int)player.GetPosition().X - 15;
            int playerY = (int)player.GetPosition().Y - 36;

            int frameColourOffset = 16 * (int)player.GetColour();

            if (player.GetFalling())
            {
                e.Graphics.DrawImage(characterImages[frameColourOffset + 12 + player.GetAnimationFrame()], new Rectangle(playerX, playerY, 38, 50), 0, 0, 19, 25, GraphicsUnit.Pixel);
            }
            else
            {
                e.Graphics.DrawImage(characterImages[frameColourOffset + (int)player.GetDirection() * 3 + player.GetAnimationFrame()], new Rectangle(playerX, playerY, 30, 48), 0, 0, 15, 24, GraphicsUnit.Pixel);
            }

            //e.Graphics.DrawRectangle(redPen, player.GetHitBox());
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            for (int i = 0; i < 17; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    int value = currentMapData[j, i].value;
                    if (value == 0)
                    {
                        if (currentMapData[j - 1, i].value == 1 || currentMapData[j - 1, i].value == 2)
                        {
                            value = 3;
                        }
                        
                    }
                    e.Graphics.DrawImage(tileImages[value], new Rectangle(i * 32, j * 32, 33, 33), 0, 0, 16, 16, GraphicsUnit.Pixel);
                }
            }

            for (int i = 0; i < bombs.Count; i++)
            {
                DrawBomb(e, bombs[i]);
            }

            DrawPlayer(e, myPlayer);
            DrawPlayer(e, otherPlayer);

            //for (int i = 0; i < debugBoxes.Count; i++)
            //{
            //    e.Graphics.DrawRectangle(redPen, debugBoxes[i]);
            //}
        }

        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                myPlayer.StartMoving(PlayerDirection.LEFT);
            }
            else if (e.KeyCode == Keys.D)
            {
                myPlayer.StartMoving(PlayerDirection.RIGHT);
            }
            else if (e.KeyCode == Keys.W)
            {
                myPlayer.StartMoving(PlayerDirection.UP);
            }
            else if (e.KeyCode == Keys.S)
            {
                myPlayer.StartMoving(PlayerDirection.DOWN);
            }
            else if (e.KeyCode == Keys.Space)
            {
                int gridPosX = (int)Math.Floor(myPlayer.GetPosition().X / 32.0f);
                int gridPosY = (int)Math.Floor(myPlayer.GetPosition().Y / 32.0f);

                if (myPlayer.GetBombsLeft() > 0)
                {
                    myPlayer.DecrementBombsLeft();
                    PacketData.BombPacketData bombPacketData = new PacketData.BombPacketData();
                    bombPacketData.gridPosX = gridPosX;
                    bombPacketData.gridPosY = gridPosY;
                    bombPacketData.size = myPlayer.GetBombSize();
                    bombPacketData.type = (int)myPlayer.GetBombType();
                    bombsSinceLastPacket.Add(bombPacketData);
                    CreateBomb(gridPosX, gridPosY, myPlayer.GetBombSize(), myPlayer.GetBombType(), myPlayer);
                }
            }
        }

        private void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A)
            {
                myPlayer.StopMoving(PlayerDirection.LEFT);
            }
            else if (e.KeyCode == Keys.D)
            {
                myPlayer.StopMoving(PlayerDirection.RIGHT);
            }
            else if (e.KeyCode == Keys.W)
            {
                myPlayer.StopMoving(PlayerDirection.UP);
            }
            else if (e.KeyCode == Keys.S)
            {
                myPlayer.StopMoving(PlayerDirection.DOWN);
            }
        }

        private void GameWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            music.Stop();
        }

        private void CreateBomb(int gridPosX, int gridPosY, int size, BombType type, Player owner)
        {
            bombs.Add(new Bomb(gridPosX, gridPosY, size, type, owner));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Select();
        }
    }
}
