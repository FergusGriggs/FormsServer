using System;
using System.Collections.Generic;
using System.Text;

namespace PacketData
{
    public enum PacketType
    {
        EMPTY,
        CHAT_MESSAGE
    }

    [Serializable]
    public class Packet
    {
        public PacketType type = PacketType.EMPTY;
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
}
