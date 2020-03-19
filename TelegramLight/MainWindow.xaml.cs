using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace TelegramLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly int apiId = int.Parse(Properties.Resources.API_ID);
        private static readonly string apiHash = Properties.Resources.API_HASH;
        string hash = "";
        private static readonly TelegramClient client = new TelegramClient(apiId, apiHash);
        public MainWindow()
        {
            InitializeComponent();
            rtbMain.Document.Blocks.Clear();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {

            if (!client.IsUserAuthorized())
            {
                TLUser user = await client.MakeAuthAsync(TbPhone.Text, hash, TbCode.Text);
            }
            else
            {
                await client.ConnectAsync();
            }


            if (client.IsUserAuthorized())
            {
                
                var dialogs = (TLDialogsSlice)await client.GetUserDialogsAsync(limit: 30);
                foreach (var item in dialogs.Chats.OfType<TLChannel>())
                {
                    lbGroupChats.Items.Add(new GroupView( item));
                }
                foreach (var item in dialogs.Dialogs)
                {
                    var peer = item.Peer as TLPeerUser;
                    if (peer != null)
                    {
                        var user = dialogs.Users.OfType<TLUser>().First(x => x.Id == peer.UserId);
                        var message = dialogs.Messages.OfType<TLMessage>().FirstOrDefault(x => x.Id == item.TopMessage);
                        lbUserChats.Items.Add(new UserView(user,message,10));
                    }
                }

            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await client.ConnectAsync();

            hash = await client.SendCodeRequestAsync(TbPhone.Text);
        }

        private async void lbUserChats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rtbMain.SelectAll();
            rtbMain.Selection.Text = "";
            var selected = (UserView)lbUserChats.SelectedItem;
            var peer = new TLInputPeerUser() { UserId = selected.id, AccessHash = selected.accessHash??0 };
            var result = await client.GetHistoryAsync(peer) as TLMessagesSlice;
            if (result == null) return;
            foreach (var item in result.Messages.OfType<TLMessage>())
            {
                var senderU = result.Users.OfType<TLUser>().FirstOrDefault(x => x.Id == item.FromId);
                rtbMain.AppendText(new UserView(senderU,item));
                rtbMain.AppendText(Environment.NewLine);
            }
        }

        private async void lbGroupChats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rtbMain.SelectAll();
            rtbMain.Selection.Text = "";
            var selected = (GroupView)lbGroupChats.SelectedItem;
            var peer = new TLInputPeerChannel() { ChannelId= selected.id, AccessHash = selected.accessHash ?? 0 };
            var result = await client.GetHistoryAsync(peer) as TLChannelMessages;
            if (result == null) return;
            foreach (var item in result.Messages.OfType<TLMessage>())
            {
                var senderU = result.Users.OfType<TLUser>().FirstOrDefault(x => x.Id == item.FromId);
                rtbMain.AppendText(new UserView(senderU, item));
                rtbMain.AppendText(Environment.NewLine);
            }
        }

        private async void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = tbMessageToSend.Text;
            TLAbsInputPeer peer;
            if (lbGroupChats.SelectedItem != null)
            {
                var selected = (GroupView)lbGroupChats.SelectedItem;
                peer = new TLInputPeerChannel() { ChannelId = selected.id, AccessHash = selected.accessHash ?? 0 };
            }
            else if (lbUserChats.SelectedItem != null)
            {
                var selected = (UserView)lbUserChats.SelectedItem;
                peer = new TLInputPeerUser() { UserId = selected.id, AccessHash = selected.accessHash ?? 0 };
            }
            else return;

            await client.SendMessageAsync(peer, message);
        }
    }
}
