using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleSharp.TL;

namespace TelegramLight
{
    class UserView
    {
        public readonly int id;
        public readonly long? accessHash;
        string text="";
        public UserView(TLUser user, TLMessage message=null, int maxlen=int.MaxValue)
        {
            id = user.Id;
            accessHash = user.AccessHash;
            text = user.Username ?? $"{user.FirstName} {user.LastName}";
            if (message != null)
                if (message.Message.Length > maxlen)
                    text += $": {message.Message.Substring(0, maxlen)}...";
                else text += $": {message.Message}";

        }
        public static implicit operator string (UserView x)
        {
            return x.ToString();
        }
        public override string ToString()
        {
            return text;
        }
    }
    class GroupView
    {
        public readonly int id;
        public readonly long? accessHash;
        string text = "";
        public GroupView(TLChannel group)
        {
            id = group.Id;
            accessHash = group.AccessHash;
            text = group.Title;

        }
        public static implicit operator string(GroupView x)
        {
            return x.ToString();
        }
        public override string ToString()
        {
            return text;
        }
    }
}
