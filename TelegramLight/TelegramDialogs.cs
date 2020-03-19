using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;

namespace TelegramLight
{
    class TelegramDialogs
    {
        public int Count { get; set; }
        public TLVector<TLDialog> Dialogs { get; set; }
        public TLVector<TLAbsMessage> Messages { get; set; }
        public TLVector<TLAbsChat> Chats { get; set; }
        public TLVector<TLAbsUser> Users { get; set; }

        public TelegramDialogs(TLDialogs dialogs)
        {
            Dialogs = dialogs.Dialogs;
            Messages = dialogs.Messages;
            Chats = dialogs.Chats;
            Users = dialogs.Users;
            Count = dialogs.Dialogs.Count + dialogs.Chats.Count;
        }

        public TelegramDialogs(TLDialogsSlice dialogs)
        {
            Dialogs = dialogs.Dialogs;
            Messages = dialogs.Messages;
            Chats = dialogs.Chats;
            Users = dialogs.Users;
            Count = dialogs.Count;
        }

        public static TelegramDialogs FromTLDialogs(TLAbsDialogs absDialogs)
        {
            if (absDialogs is TLDialogs dialogs)
            {
                return new TelegramDialogs(dialogs);
            }
            else if (absDialogs is TLDialogsSlice dialogsSlice)
            {
                return new TelegramDialogs(dialogsSlice);
            }
            else
            {
                throw new Exception("Unsupported type " + absDialogs.GetType());
            }
        }
    }
}
