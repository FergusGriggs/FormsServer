using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleClient
{
    public enum BombState
    { 
        PLACED,
        EXPLODED
    }

    public enum BombType
    {
        GRAY,
        RED
    }

    public class Bomb
    {
        BombType type;
        BombState state;
        int gridPosX;
        int gridPosY;
        int size;
        int age;
        int animationTick;
        bool animationBool;
        int animationFrame;
        Player owner;

        public Bomb(int gridPosX, int gridPosY, int size, BombType type, Player owner)
        {
            this.gridPosX = gridPosX;
            this.gridPosY = gridPosY;
            this.size = size;
            this.type = type;
            this.owner = owner;

            state = BombState.PLACED;
            age = 0;

            animationTick = 0;
            animationBool = false;
            animationFrame = 0;
        }

        public int GetAnimationFrame()
        {
            return animationFrame;
        }

        public BombState GetBombState()
        {
            return state;
        }

        public int GetSize()
        {
            return size;
        }

        public void Update()
        {
            age++;
            animationTick++;
            if (animationTick > 40)
            {
                animationTick = 0;
                if (animationFrame == 1)
                {
                    if (animationBool)
                    {
                        animationFrame = 0;
                    }
                    else
                    {
                        animationFrame = 2;
                    }
                    animationBool = !animationBool;
                }
                else
                {
                    animationFrame = 1;
                }
            }
            if (age > 500)
            {
                owner.ReplenishBomb();
                state = BombState.EXPLODED;
            }
        }

        public int GetGridX()
        {
            return gridPosX;
        }

        public int GetGridY()
        {
            return gridPosY;
        }

        public BombType GetBombType()
        {
            return type;
        }
    }

    public struct TileData
    {
        public int value;
        public int age;
    }

}
