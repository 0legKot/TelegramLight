using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;

namespace TelegramLight
{
    class TelegramMessages
    {
        public int Count { get; set; }
        public TLVector<TLAbsMessage> Messages { get; set; }
        public TLVector<TLAbsChat> Chats { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }

        public TelegramMessages(TLMessages messages)
        {
            Messages = messages.Messages;
            Chats = messages.Chats;
            Users = messages.Users;
            Count = Messages.Count;
        }

        public TelegramMessages(TLMessagesSlice messages)
        {
            Messages = messages.Messages;
            Chats = messages.Chats;
            Users = messages.Users;
            Count = messages.Count;
        }

        public TelegramMessages(TLChannelMessages messages)
        {
            Messages = messages.Messages;
            Chats = messages.Chats;
            Users = messages.Users;
            Count = messages.Count;
        }

        public static TelegramMessages FromTLMessages(TLAbsMessages absMessages)
        {
            if (absMessages is TLMessages messages)
            {
                return new TelegramMessages(messages);
            }
            else if (absMessages is TLMessagesSlice messagesSlice)
            {
                return new TelegramMessages(messagesSlice);
            }
            else if (absMessages is TLChannelMessages channelMessages)
            {
                return new TelegramMessages(channelMessages);
            }
            else
            {
                throw new Exception("Unsupported type " + absMessages.GetType());
            }
        }

        public static implicit operator TelegramMessages(TLMessages messages)
        {
            return new TelegramMessages(messages);
        }

        public static implicit operator TelegramMessages(TLMessagesSlice messages)
        {
            return new TelegramMessages(messages);
        }

        public static implicit operator TelegramMessages(TLChannelMessages messages)
        {
            return new TelegramMessages(messages);
        }


    }
}
