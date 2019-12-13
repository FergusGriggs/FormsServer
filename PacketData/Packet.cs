using System;
using System.Collections.Generic;
using System.Text;

namespace PacketData
{
    [Serializable]
    public enum PacketType
    {
        EMPTY,
        CHAT_MESSAGE,
        TERMINATE_CLIENT,
        CLEAR_WINDOW,
        END_CONNECTION,
        LOGIN,
        LEAVE_GAME,
        OPEN_BOMBERMAN_WINDOW,
        BOMBERMAN_CLIENT_TO_SERVER,
        BOMBERMAN_SERVER_TO_CLIENT
    }

    [Serializable]
    public class PlayerPacketData
    {
        public float positionX;
        public float positionY;
        public int direction;
        public float moveSpeed;
        public bool moving;
    }

    [Serializable]
    public class BombPacketData
    {
        public int gridPosX;
        public int gridPosY;
        public int type;
        public int size;
    }

    [Serializable]
    public class Packet
    {
        public PacketType type = PacketType.EMPTY;
        public Packet(PacketType type)
        {
            this.type = type;
        }

        public Packet()
        {

        }
    }

    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public String message = "";
        public ChatMessagePacket(String message)
        {
            this.type = PacketType.CHAT_MESSAGE;
            this.message = message;
        }
    }

    [Serializable]
    public class LogInPacket : Packet
    {
        public String username = "";
        public LogInPacket(String username)
        {
            this.type = PacketType.LOGIN;
            this.username = username;
        }
    }

    [Serializable]
    public class BombermanClientToServerPacket : Packet
    {
        public List<BombPacketData> bombsPlaced;
        public PlayerPacketData player;

        public BombermanClientToServerPacket(List<BombPacketData> bombsPlaced, PlayerPacketData player)
        //public BombermanClientToServerPacket(PlayerPacketData player)
        {
            this.type = PacketType.BOMBERMAN_CLIENT_TO_SERVER;
            this.bombsPlaced = bombsPlaced;
            this.player = player;
        }
    }

    [Serializable]
    public class BombermanServerToClientPacket : Packet
    {
        public List<BombPacketData> otherPlayerBombs;
        public PlayerPacketData otherPlayer;

        public BombermanServerToClientPacket(List<BombPacketData> otherPlayerBombs, PlayerPacketData otherPlayer)
        //public BombermanServerToClientPacket(PlayerPacketData otherPlayer)
        {
            this.type = PacketType.BOMBERMAN_SERVER_TO_CLIENT;
            this.otherPlayerBombs = otherPlayerBombs;
            this.otherPlayer = otherPlayer;
        }
    }

    [Serializable]
    public class OpenBombermanWindowPacket : Packet
    {
        public bool isPlayerOne;

        public OpenBombermanWindowPacket(bool isPlayerOne)
        {
            this.type = PacketType.OPEN_BOMBERMAN_WINDOW;
            this.isPlayerOne = isPlayerOne;
        }
    }
}
