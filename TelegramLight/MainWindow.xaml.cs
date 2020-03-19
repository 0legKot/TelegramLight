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
        private int currentChatLastMessageId = 0;
        private TLAbsInputPeer currentPeer;
        public MainWindow()
        {
            InitializeComponent();
            rtbMain.Document.Blocks.Clear();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            await client.ConnectAsync();

            if (!client.IsUserAuthorized())
            {
                await SignInOrRegister();
            }

            if (client.IsUserAuthorized())
            {

                var dialogs = TelegramDialogs.FromTLDialogs(await client.GetUserDialogsAsync(limit: 30));
                foreach (var item in dialogs.Chats.OfType<TLChannel>())
                {
                    lbGroupChats.Items.Add(new GroupView(item));
                }
                foreach (var item in dialogs.Chats.OfType<TLChat>())
                {
                    lbChatChats.Items.Add(new GroupView(item));
                }
                foreach (var item in dialogs.Dialogs)
                {
                    var peer = item.Peer as TLPeerUser;
                    if (peer != null)
                    {
                        var user = dialogs.Users.OfType<TLUser>().First(x => x.Id == peer.UserId);
                        var message = dialogs.Messages.OfType<TLMessage>().FirstOrDefault(x => x.Id == item.TopMessage);
                        lbUserChats.Items.Add(new UserView(user, message, 10));
                    }
                }

            }
        }

        private async Task SignInOrRegister()
        {
            if (TbPhone.Text == "" || TbPhone.Text == "+" || TbCode.Text == "")
            {
                rtbOutput.AppendText("Enter phone number and code to authorize");
                return;
            }

            try
            {
                TLUser user = await client.MakeAuthAsync(TbPhone.Text, hash, TbCode.Text);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "PHONE_NUMBER_UNOCCUPIED")
                {
                    await RegisterUser();
                }
                else
                {
                    rtbOutput.AppendText(ex.Message);
                    rtbOutput.AppendText(Environment.NewLine);
                }
            }
        }


        private async Task RegisterUser()
        {
            if (TbRegistrationName.Text == "" || TbRegistrationSurname.Text == "")
            {
                rtbOutput.AppendText("Enter name and surname to register");
                rtbOutput.AppendText(Environment.NewLine);
                return;
            }
            try
            {
                await client.SignUpAsync(TbPhone.Text, hash, TbCode.Text, TbRegistrationName.Text, TbRegistrationSurname.Text);
            }
            catch (InvalidOperationException ex)
            {
                rtbOutput.AppendText(ex.Message);
                rtbOutput.AppendText(Environment.NewLine);
            }
        }


        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await client.ConnectAsync();

            try
            {
                hash = await client.SendCodeRequestAsync(TbPhone.Text);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "PHONE_NUMBER_INVALID")
                {
                    rtbOutput.AppendText("Please, enter valid phone number");
                    rtbOutput.AppendText(Environment.NewLine);
                }
                else
                {
                    rtbOutput.AppendText(ex.Message);
                }

            }
        }

        private async void lbUserChats_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lbUserChats.SelectedItem == null)
            {
                return;
            }
            await UpdateUserChat();
        }

        private async Task UpdateUserChat()
        {
            rtbMain.SelectAll();
            rtbMain.Selection.Text = "";
            var selected = (UserView)lbUserChats.SelectedItem;
            var peer = new TLInputPeerUser() { UserId = selected.id, AccessHash = selected.accessHash ?? 0 };
            currentPeer = peer;
            currentChatLastMessageId = 0;
            await UpdateChat(peer, currentChatLastMessageId);
        }

        private async void lbGroupChats_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lbGroupChats.SelectedItem == null)
            {
                return;
            }
            await UpdateGroupChat();
        }

        private async Task UpdateGroupChat()
        {
            rtbMain.SelectAll();
            rtbMain.Selection.Text = "";
            var selected = (GroupView)lbGroupChats.SelectedItem;
            var peer = new TLInputPeerChannel() { ChannelId = selected.id, AccessHash = selected.accessHash ?? 0 };
            currentPeer = peer;
            currentChatLastMessageId = 0;
            await UpdateChat(peer, currentChatLastMessageId);
        }


        private async void lbChatChats_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lbChatChats.SelectedItem == null)
            {
                return;
            }
            await UpdateChatChats();
        }

        private async Task UpdateChatChats()
        {
            rtbMain.SelectAll();
            rtbMain.Selection.Text = "";
            var selected = (GroupView)lbChatChats.SelectedItem;
            var peer = new TLInputPeerChat() { ChatId = selected.id };
            currentPeer = peer;
            currentChatLastMessageId = 0;
            await UpdateChat(peer, currentChatLastMessageId);
        }

        private async void btnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }



        private async void tbMessageToSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            string message = tbMessageToSend.Text;
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            TLAbsInputPeer peer = currentPeer;

            //if (lbUserChats.SelectedItem != null)
            //{
            //    var selected = (UserView)lbUserChats.SelectedItem;
            //    peer = new TLInputPeerUser() { UserId = selected.id, AccessHash = selected.accessHash ?? 0 };
            //}
            //else if (lbChatChats.SelectedItem != null)
            //{
            //    var selected = (GroupView)lbChatChats.SelectedItem;
            //    peer = new TLInputPeerChat() { ChatId = selected.id };
            //}
            //else if (lbGroupChats.SelectedItem != null)
            //{
            //    var selected = (GroupView)lbGroupChats.SelectedItem;
            //    peer = new TLInputPeerChannel() { ChannelId = selected.id, AccessHash = selected.accessHash ?? 0 };
            //}
            //else return;

            await client.SendMessageAsync(peer, message);
            tbMessageToSend.Text = "";

            await UpdateChat(peer, currentChatLastMessageId);
        }

        private async Task UpdateChat(TLAbsInputPeer peer, int lastMessageId)
        {
            TLAbsMessages history = null;
            //int retries = 5;
            //for (int i = 0; i < retries; ++i)
            //{
            //    try
            //    {
            //        history = await client.GetHistoryAsync(peer, minId: currentChatLastMessageId);
            //        break;
            //    }
            //    catch (IOException)
            //    {
            //        await client.ConnectAsync(true);
            //        if (i == retries - 1)
            //        {
            //            throw;
            //        }
            //    }
            //    catch (InvalidOperationException ex)
            //    {
            //        if (ex.Message == "Couldn't read the packet length")
            //        {
            //            await client.ConnectAsync(true);
            //        }
            //        else throw;
            //    }
            //}

            history = await client.GetHistoryAsync(peer, minId: currentChatLastMessageId);
            var result = TelegramMessages.FromTLMessages(history);
            if (result == null)
            {
                currentChatLastMessageId = 0;
                return;
            }
            currentChatLastMessageId = result.Messages.OfType<TLMessage>().FirstOrDefault()?.Id ?? 0;
            foreach (var item in result.Messages.OfType<TLMessage>().Reverse())
            {
                var senderU = result.Users.OfType<TLUser>().FirstOrDefault(x => x.Id == item.FromId);
                rtbMain.AppendText(Environment.NewLine);
                rtbMain.AppendText(new UserView(senderU, item));
            }
            rtbMain.ScrollToEnd();
        }


    }
}
