using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SimpleClient
{
    public enum PlayerDirection
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    public enum PlayerColour
    {
        WHITE,
        RED,
        GRAY,
        BLUE
    }

    public enum PlayerPowerup
    {
        FIRE,
        FIRE_DOWN,
        BOMB,
        BOMB_DOWN,
        SKATES,
        GETA,
        RED_BOMB
    }

    public class Player
    {
        PlayerDirection playerDirection;
        PlayerColour playerColour;

        BombType bombType;

        Rectangle hitbox;
        PointF position;

        bool moving;
        int bombsLeft;
        int bombSize;
        float moveSpeed;
        int maxBombs;

        bool falling;

        int animationFrame;
        int animationTick;
        bool animationFoot;

        bool[] keysDown;

        public Player(float posX, float posY, PlayerColour colour, PlayerDirection direction)
        {
            position = new PointF(posX, posY);
            UpdateHitbox();

            playerColour = colour;
            playerDirection = direction;

            moving = false;
            maxBombs = 1;
            bombsLeft = maxBombs;
            moveSpeed = 1.0f;
            bombSize = 1;
            bombType = BombType.GRAY;

            falling = false;

            animationTick = 0;
            animationFrame = 0;
            animationFoot = false;

            keysDown = new bool[4] { false, false, false, false };
        }

        public void ApplyPowerup(PlayerPowerup powerup)
        {
            switch (powerup)
            {
                case PlayerPowerup.FIRE:
                    bombSize++;
                    break;
                case PlayerPowerup.FIRE_DOWN:
                    if (bombSize > 1)
                    {
                        bombSize--;
                    }
                    break;
                case PlayerPowerup.BOMB:
                    bombsLeft++;
                    maxBombs++;
                    break;
                case PlayerPowerup.BOMB_DOWN:
                    if (maxBombs > 1)
                    {
                        maxBombs--;
                        bombsLeft--;
                    }
                    break;
                case PlayerPowerup.SKATES:
                    moveSpeed += 0.2f;
                    break;
                case PlayerPowerup.GETA:
                    moveSpeed -= 0.2f;
                    break;
                case PlayerPowerup.RED_BOMB:
                    bombType = BombType.RED;
                    break;
            }

        }

        public void ReplenishBomb()
        {
            bombsLeft++;
        }

        public void FallOver()
        {
            moving = false;
            falling = true;
            animationFrame = 0;
            animationTick = 0;
        }

        public int GetBombsLeft()
        {
            return bombsLeft;
        }

        public void DecrementBombsLeft()
        {
            bombsLeft--;
        }

        public BombType GetBombType()
        {
            return bombType;
        }

        public int GetBombSize()
        {
            return bombSize;
        }

        public PointF GetPosition()
        {
            return position;
        }

        public bool GetFalling()
        {
            return falling;
        }

        public bool GetMoving()
        {
            return moving;
        }

        public float GetMoveSpeed()
        {
            return moveSpeed;
        }

        public void SetPositionX(float x)
        {
            this.position.X = x;
        }

        public void SetPositionY(float y)
        {
            this.position.Y = y;
        }

        public PlayerDirection GetDirection()
        {
            return playerDirection;
        }

        public int GetAnimationFrame()
        {
            return animationFrame;
        }

        public void SetPosition(PointF position)
        {
            this.position = position;
        }

        public Rectangle GetHitBox()
        {
            return hitbox;
        }

        public PlayerColour GetColour()
        {
            return playerColour;
        }

        public void SetMoveSpeed(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }

        public void SetMoving(bool moving)
        {
            this.moving = moving;
        }

        public void SetDirection(PlayerDirection direction)
        {
            this.playerDirection = direction;
        }

        public void UpdateHitbox()
        {
            hitbox = new Rectangle((int)position.X - 6, (int)position.Y - 6, 12, 12);
        }

        public void StartMoving(PlayerDirection direction)
        {
            if (!falling)
            {
                moving = true;
                keysDown[(int)direction] = true;
                playerDirection = direction;
            }
            
        }

        public void StopMoving(PlayerDirection direction)
        {
            keysDown[(int)direction] = false;
            if (!falling)
            {
                if (direction == playerDirection)
                {
                    bool foundActiveKey = false;
                    for (int i = 0; i < 4; i++)
                    {
                        if (keysDown[i])
                        {
                            foundActiveKey = true;
                            playerDirection = (PlayerDirection)i;
                        }
                    }

                    if (!foundActiveKey)
                    {
                        moving = false;
                        animationTick = 0;
                        animationFrame = 0;
                    }
                }
            }
        }

        private void UpdateAnimation()
        {
            animationTick++;

            if (falling)
            {
                if (animationTick > 50)
                {
                    if (animationFrame != 3)
                    {
                        animationFrame++;
                        animationTick = 0;
                    }
                    else
                    {
                        if (animationTick > 200)
                        {
                            falling = false;
                            animationFrame = 0;
                            animationTick = 0;
                        }
                    }
                }
            }
            else
            {
                if (animationTick > 20)
                {
                    animationTick = 0;
                    if (animationFrame == 0)
                    {
                        if (animationFoot)
                        {
                            animationFrame = 1;
                        }
                        else
                        {
                            animationFrame = 2;
                        }
                        animationFoot = !animationFoot;
                    }
                    else
                    {
                        animationFrame = 0;
                    }
                }
            }
        }

        public void Update()
        {
            if (moving)
            {
                if (playerDirection == PlayerDirection.LEFT)
                {
                    position.X -= 0.5f * moveSpeed;
                }
                else if (playerDirection == PlayerDirection.RIGHT)
                {
                    position.X += 0.5f * moveSpeed;
                }
                else if (playerDirection == PlayerDirection.UP)
                {
                    position.Y -= 0.5f * moveSpeed;
                }
                else if (playerDirection == PlayerDirection.DOWN)
                {
                    position.Y += 0.5f * moveSpeed;
                }
            }

            if (falling || moving)
            {
                UpdateAnimation();
            }
            else
            {
                animationFrame = 0;
            }
        }
    };
}
