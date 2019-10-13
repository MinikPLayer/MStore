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
using System.Windows.Shapes;

using System.Xml;
using System.Windows.Markup;
using System.IO;

namespace MStore
{
    /// <summary>
    /// Logika interakcji dla klasy DownloadMenu.xaml
    /// </summary>
    public partial class AdminVoucherMenu : Window
    {
        public class Voucher
        {
            public string code;

            public long coinsAddon;
            public long gameID;

            public int usesLeft;

            public Voucher(string _code, long _coinsAddon, long _gameID, int _usesCount)
            {
                code = _code;
                coinsAddon = _coinsAddon;
                gameID = _gameID;

                usesLeft = _usesCount;
            }

            public override string ToString()
            {
                string val = code;
                val += ": " + usesLeft.ToString() + " uses left. Adds ";
                if(coinsAddon >= 0)
                {
                    val += coinsAddon.ToString() + " coins";
                }
                else if(gameID >= 0)
                {
                    val += "game with ID: " + gameID.ToString();
                }
                else
                {
                    return "Bad voucher";
                }

                val += "\n";

                return val;
            }
        }

        public enum Commands
        {
            unknown,
            ChangeMoney,
            AddMoney,
            SubstractMoney,
            VoucherInfo,
            AddVoucher,
            DeleteVoucher,
            ChangeVoucher,
            VouchersList,
            UserInfo,
            userID
        }

        public static void SendCommand(Commands command, string parameters = "")
        {

            switch (command)
            {
                case Commands.unknown:
                    Debug.LogError("Unknown command");
                    return;
                case Commands.ChangeMoney:
                    App.storeClient.socket._Send("ADMIN" + "MNYCH" + parameters);
                    break;
                case Commands.AddMoney:
                    App.storeClient.socket._Send("ADMIN" + "MNYAD" + parameters);
                    break;
                case Commands.SubstractMoney:
                    App.storeClient.socket._Send("ADMIN" + "MNYSB" + parameters);
                    break;
                case Commands.VoucherInfo:
                    App.storeClient.socket._Send("ADMIN" + "VCHNF" + parameters);
                    break;
                case Commands.AddVoucher:
                    App.storeClient.socket._Send("ADMIN" + "VCHAD" + parameters);
                    break;
                case Commands.DeleteVoucher:
                    App.storeClient.socket._Send("ADMIN" + "VCHDL" + parameters);
                    break;
                case Commands.ChangeVoucher:
                    App.storeClient.socket._Send("ADMIN" + "VCHCH" + parameters);
                    break;
                case Commands.UserInfo:
                    App.storeClient.socket._Send("ADMIN" + "USRNF" + parameters);
                    break;
                case Commands.userID:
                    App.storeClient.socket._Send("ADMIN" + "USRID" + parameters);
                    break;
                case Commands.VouchersList:
                    App.storeClient.socket._Send("ADMIN" + "VCHLS" + parameters);
                    break;
                default:
                    Debug.LogError("Command " + command + " is not implemented");
                    break;
            }
        }

        public static class AdminCommands
        {
            public static string AddMoney(long userID, decimal value)
            {
                if (value == 0) return "OK";
                SendCommand(Commands.AddMoney, userID.ToString() + ";" + value.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string ChangeMoney(long userID, decimal value)
            {
                SendCommand(Commands.ChangeMoney, userID.ToString() + ";" + value.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string SubstractValue(long userID, decimal value)
            {
                if (value == 0) return "OK";
                SendCommand(Commands.SubstractMoney, userID.ToString() + ";" + value.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string VoucherInfo(string code)
            {
                if(code.Length == 0)
                {
                    return "BV";
                }

                SendCommand(Commands.VoucherInfo, code);

                return App.storeClient.socket.WaitForReceive();
            }

            public static string AddVoucher(long gameIDorCoins, int uses, string code, bool coins)
            {
                if(gameIDorCoins < 0 || uses == 0 || code.Length == 0)
                {
                    return "BA";
                }

                if (coins)
                {
                    SendCommand(Commands.AddVoucher, code + ";" + uses.ToString() + ";c" + gameIDorCoins.ToString());
                }
                else
                {
                    SendCommand(Commands.AddVoucher, code + ";" + uses.ToString() + ";" + gameIDorCoins.ToString());
                }

                return App.storeClient.socket.WaitForReceive();
            }

            public static string DeleteVoucher(string code)
            {
                if (code.Length == 0)
                {
                    return "OK";
                }

                SendCommand(Commands.DeleteVoucher, code);

                return App.storeClient.socket.WaitForReceive();
            }

            /*public static string ChangeVoucher(string code, long gameID, int uses)
            {
                if (gameID < 0 || uses == 0 || code.Length == 0)
                {
                    return "OK";
                }

                SendCommand(Commands.ChangeVoucher, gameID.ToString() + ";" + uses.ToString() + ";" + code);

                return App.storeClient.socket.WaitForReceive();
            }*/

            public static string ChangeVoucherUses(string code, int uses)
            {
                if(code.Length == 0 || uses < -1)
                {
                    return "BA";
                }

                SendCommand(Commands.ChangeVoucher, code + ";0;" + uses.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string ChangeVoucherCoins(string code, long coins)
            {
                if (code.Length == 0 || coins < -1)
                {
                    return "BA";
                }

                Debug.Log("Parameters: \"" + (code + ";1;" + coins.ToString()) + "\"");

                SendCommand(Commands.ChangeVoucher, code + ";1;" + coins.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string ChangeVoucherGameID(string code, long gameID)
            {
                if (code.Length == 0 || gameID < -1)
                {
                    return "BA";
                }

                SendCommand(Commands.ChangeVoucher, code + ";2;" + gameID.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string GetVouchersList()
            {
                SendCommand(Commands.VouchersList);

                return App.storeClient.socket.WaitForReceive();
            }

            public static string UserInfo(long userID)
            {
                if(userID < 0)
                {
                    return "BU";
                }

                SendCommand(Commands.UserInfo, userID.ToString());

                return App.storeClient.socket.WaitForReceive();
            }

            public static string GetUserID(string userName)
            {
                if(userName.Length == 0)
                {
                    return "BU";
                }

                SendCommand(Commands.userID, userName);

                return App.storeClient.socket.WaitForReceive();
            }
        }


        private void RequestVoucher(string code)
        {
            code = ConvertVoucherCodeToSend(code);

            string response = AdminCommands.VoucherInfo(code);
            /*if(response != "OK")
            {
                Debug.LogError("Server response: " + response);
                return;
            }*/

            targetVoucher = ParseStringToVoucher(response);
            UpdateVoucherDisplay();
        }

        Voucher targetVoucher = null;

        private void UpdateVoucherDisplay()
        {
            if(targetVoucher == null)
            {
                TargetVoucherCode.Text = "Error, voucher not found";
                CoinsTextBox.Text = "-";
                GameTextBox.Text = "-";
                UsesTextBox.Text = "0";
                VoucherCodeBox.Text = "";
                return;
            }

            TargetVoucherCode.Text = targetVoucher.code;
            if(targetVoucher.coinsAddon >= 0)
            {
                CoinsTextBox.Text = targetVoucher.coinsAddon.ToString();
                GameTextBox.Text = "-";
            }
            else
            {
                GameTextBox.Text = targetVoucher.gameID.ToString();
                CoinsTextBox.Text = "-";
            }

            UsesTextBox.Text = targetVoucher.usesLeft.ToString();
            VoucherCodeBox.Text = targetVoucher.code;
        }

        public Voucher ParseStringToVoucher(string data)
        {
            Voucher voucher = new Voucher("", -1, -1, -1);

            string wStr = "";

            if (data.Length == 0)
            {
                Debug.LogError("Voucher data is corrupted");
                return null;
            }
            // Code
            data = MUtil.GetStringToSpecialCharAndDelete(data, ':', out wStr);
            voucher.code = wStr;

            if (data.Length == 0)
            {
                Debug.LogError("Voucher data is corrupted");
                return null;
            }
            // Coins / game
            data = MUtil.GetStringToSpecialCharAndDelete(data, ';', out wStr);
            if (wStr[0] == 'c')
            {
                long coins = -1;
                wStr = wStr.Remove(0, 1);
                if(!long.TryParse(wStr, out coins))
                {
                    Debug.ConversionError(wStr, "coins", coins);
                    return null;
                }

                voucher.coinsAddon = coins;
            }
            else
            {
                long gameID = -1;
                if (!long.TryParse(wStr, out gameID))
                {
                    Debug.ConversionError(wStr, "gameID", gameID);
                    return null;
                }

                voucher.gameID = gameID;
            }

            if (data.Length == 0)
            {
                Debug.LogError("Voucher data is corrupted");
                return null;
            }
            int usesLeft = -1;
            if (!int.TryParse(data, out usesLeft))
            {
                Debug.ConversionError(wStr, "usesLeft", usesLeft);
                return null;
            }

            voucher.usesLeft = usesLeft;

            return voucher;

        }

        private List<Voucher> vouchers = new List<Voucher>();
        string voucherPrefab = "";


        public static SolidColorBrush normalVoucherColor = Brushes.White;
        public static SolidColorBrush selectedVoucherColor = Brushes.Orange;
        public void ResetVouchersListColors()
        {
            for(int i = 0;i<VouchersSP.Children.Count;i++)
            {
                TextBlock block = (TextBlock)VouchersSP.Children[i];
                block.Foreground = normalVoucherColor;
            }
        }

        public void OnVoucherClick(object sender, MouseButtonEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            Debug.Log(textBlock.Text + " clicked");

            int id = -1;
            string idStr = MUtil.GetStringToSpecialChar(textBlock.Text, ')');
            if(!int.TryParse(idStr, out id))
            {
                Debug.ConversionError(idStr, "id", id);
                return;
            }

            if(id > vouchers.Count || id < 0)
            {
                Debug.LogError("Bad ID");
                return;
            }

            targetVoucher = vouchers[id];
            UpdateVoucherDisplay();

            ResetVouchersListColors();

            textBlock.Foreground = selectedVoucherColor;
        }

        public void AddVoucher(Voucher voucher)
        {
            vouchers.Add(voucher);

            StringReader reader = new StringReader(voucherPrefab);
            XmlReader xmlReader = XmlReader.Create(reader);
            TextBlock newButton = (TextBlock)XamlReader.Load(xmlReader);

            newButton.Text = (vouchers.Count - 1).ToString() + ") " + voucher.ToString();

            newButton.MouseLeftButtonDown += OnVoucherClick;

            VouchersSP.Children.Add(newButton);
        }

        public void ClearVouchersList()
        {
            VouchersSP.Children.Clear();
            vouchers.Clear();
        }

        public void RefreshVouchersList()
        {
            ClearVouchersList();

            string response = AdminCommands.GetVouchersList();


            string voucherLine;
            while (response.Length != 0)
            { 
                response = MUtil.GetStringToSpecialCharAndDelete(response, '\n', out voucherLine);

                Voucher v = ParseStringToVoucher(voucherLine);
                if(v == null)
                {
                    Debug.LogError("Error in vouchers data");
                    return;
                }

                AddVoucher(v);
            }
        }
        
        public AdminVoucherMenu()
        {
            InitializeComponent();

            NavBar.SetMode(Controls.NavBar.Modes.Admin);

            //RequestUser(0);
            //UpdateUserDisplay();
            //Debug.Log("Command response: " + response);
            /*if (targetUser == null)
            {
                Debug.LogError("Target user is null");
                return;
            }
            Debug.Log("Target user: " + targetUser.userName);*/

            voucherPrefab = XamlWriter.Save(VoucherButtonTemplate);
            VouchersSP.Children.Clear();

            RefreshVouchersList();
        }

        private bool IsNumber(char ch)
        {
            return (ch > 47 && ch < 58 ) || ch == ',' || ch == '-';
        }

        private void UserIDTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*TextBox block = (TextBox)sender;
            for(int i = 0;i<block.Text.Length;i++)
            {
                if(!IsNumber(block.Text[i]))
                {
                    block.Text = block.Text.Remove(i, 1);
                    i--;
                }
            }*/
        }

        private string ConvertVoucherCodeToSend(string code)
        {
            for(int i = 0;i<code.Length;i++)
            {
                if(code[i] == '-')
                {
                    code = code.Remove(i, 1);
                    i--;
                }
            }

            return code;
        }

        private void AddVoucherButton_Click(object sender, RoutedEventArgs e)
        {
            //Debug.LogError("Not implemented yet!");
            string code = ConvertVoucherCodeToSend(VoucherCodeBox.Text);
            int uses = -1;
            long coins = -1;
            long gameID = -1;

            if(code.Length == 0)
            {
                Debug.LogError("No code entered");
                return;
            }

            if (!int.TryParse(UsesTextBox.Text, out uses))
            {
                Debug.ConversionError(UsesTextBox.Text, "uses", uses);
                return;
            }

            if(uses < -1)
            {
                Debug.LogError("Cannot add voucher without any uses");
                return;
            }

            string response;
            if (!long.TryParse(GameTextBox.Text, out gameID))
            {
                gameID = -1;
                if(!long.TryParse(CoinsTextBox.Text, out coins))
                {
                    Debug.ConversionError(CoinsTextBox.Text, "gameID and coins", coins);
                    return;
                }

                response = AdminCommands.AddVoucher(coins, uses, code, true);
            }
            else
            {
                response = AdminCommands.AddVoucher(gameID, uses, code, false);
            }


            if(response != "OK")
            {
                Debug.LogError("Server response: \"" + response + "\"");
            }

            RefreshVouchersList();
        }

        private void DeleteVoucherButton_Click(object sender, RoutedEventArgs e)
        {
            
            string code = ConvertVoucherCodeToSend(VoucherCodeBox.Text);

            if (code.Length == 0)
            {
                Debug.LogError("No code entered");
                return;
            }

            string response = AdminCommands.DeleteVoucher(code);

            if (response != "OK")
            {
                Debug.LogError("Server response: \"" + response + "\"");
            }

            RefreshVouchersList();
        }

        private void UsesChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (targetVoucher == null)
            {
                Debug.LogError("No voucher selected");
                return;
            }

            //Debug.LogError("Not implemented yet!");
            int uses = -1;
            if(!int.TryParse(UsesTextBox.Text, out uses))
            {
                Debug.ConversionError(UsesTextBox.Text, "uses", uses);
                return;
            }

            AdminCommands.ChangeVoucherUses(targetVoucher.code, uses);
        }

        private void CoinsChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (targetVoucher == null)
            {
                Debug.LogError("No voucher selected");
                return;
            }

            long coins = -1;
            if (!long.TryParse(CoinsTextBox.Text, out coins))
            {
                Debug.ConversionError(CoinsTextBox.Text, "uses", coins);
                return;
            }

            AdminCommands.ChangeVoucherCoins(targetVoucher.code, coins);
        }

        private void GameChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if(targetVoucher == null)
            {
                Debug.LogError("No voucher selected");
                return;
            }

            long gameID = -1;
            if (!long.TryParse(GameTextBox.Text, out gameID))
            {
                Debug.ConversionError(GameTextBox.Text, "uses", gameID);
                return;
            }

            AdminCommands.ChangeVoucherGameID(targetVoucher.code, gameID);
        }

        private void RequestButton_Click(object sender, RoutedEventArgs e)
        {
            ResetVouchersListColors();

            //Debug.LogError("Not implemented yet!");
            RequestVoucher(VoucherCodeBox.Text);
        }

        private void CoinsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            

            TextBox block = (TextBox)sender;

            if(block.Text == "-")
            {
                return;
            }

            for (int i = 0; i < block.Text.Length; i++)
            {
                if (!IsNumber(block.Text[i]))
                {
                    block.Text = block.Text.Remove(i, 1);
                    i--;
                }
            }

            

            if (GameTextBox != null)
            {
                GameTextBox.Text = "-";
            }

            if(block.Text.Length > 1)
            {
                if(block.Text[block.Text.Length - 1] == '-')
                {
                    block.Text = block.Text.Remove(block.Text.Length - 1, 1);
                }

                block.CaretIndex = block.Text.Length;
            }
        }

        private void GameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox block = (TextBox)sender;

            if (block.Text == "-")
            {
                return;
            }
            for (int i = 0; i < block.Text.Length; i++)
            {
                if (!IsNumber(block.Text[i]))
                {
                    block.Text = block.Text.Remove(i, 1);
                    i--;
                }
            }

            if (CoinsTextBox != null)
            {
                CoinsTextBox.Text = "-";
            }

            if (block.Text.Length > 1)
            {
                if (block.Text[block.Text.Length - 1] == '-')
                {
                    block.Text = block.Text.Remove(block.Text.Length - 1, 1);
                }

                block.CaretIndex = block.Text.Length;
            }
        }

        private void UsesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
