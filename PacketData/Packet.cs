using System;
using System.Collections.Generic;
using System.Text;

namespace PacketData
{
    public enum PacketType
    {
        EMPTY,
        CHAT_MESSAGE,
        TERMINATE_CLIENT,
        CLEAR_WINDOW,
        END_CONNECTION,
        LOGIN
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
}
