using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using TLSharp.Core;

namespace TelegramLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int apiId = 0;
        const string apiHash = "";
        string hash = "";
        string code = "";
        static readonly TelegramClient client= new TelegramClient(apiId, apiHash);
        public MainWindow()
        {
            InitializeComponent();
            rtbMain.Document.Blocks.Clear();
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            

            TLUser user = await client.MakeAuthAsync(TbPhone.Text, hash, TbCode.Text);
            
            if (client.IsUserAuthorized())
            {
                //get available contacts
                var result = await client.GetContactsAsync();
                rtbMain.AppendText(result.Contacts.Count.ToString()+"\n");
                foreach (var item in result.Users.OfType<TLUser>())
                {
                    rtbMain.AppendText(item.Username+"\n");
                }
                var kostya = result.Users
                    .OfType<TLUser>()
                    .FirstOrDefault(x => x.Username == "KostyaNes");
                await client.SendMessageAsync(new TLInputPeerUser() { UserId = kostya.Id }, "Hello from my stupid telegram app");
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            await client.ConnectAsync();

            hash = await client.SendCodeRequestAsync(TbPhone.Text);
        }
    }
}
