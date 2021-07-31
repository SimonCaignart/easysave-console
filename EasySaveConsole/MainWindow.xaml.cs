using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

namespace EasySaveConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IPHostEntry ipHost;
        IPAddress ipAddr;
        Socket sender;

        // Threads
        Thread updateLoop;
        Thread listeningLoop;

        bool isServerDown = false;

        public MainWindow()
        {

            InitializeComponent();


            ipHost = Dns.GetHostEntry(Dns.GetHostName());
            ipAddr = ipHost.AddressList[0];
            ipAddressBox.Text = ipAddr.ToString();
            listeningPortBox.Text = "11111";
            ConnectionSuccessBox.Foreground = Brushes.Red;
            ConnectionSuccessBox.Text = "Not connected yet.";
        }

        // ExecuteClient() Method 
        public void ExecuteClient()
        {
            // Establish the remote endpoint  
            // for the socket. This example  
            // uses port 11111 on the local  
            // computer. 
            IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

            // Creation TCP/IP Socket using  
            // Socket Class Costructor 
            sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // Connect Socket to the remote  
                // endpoint using method Connect() 
                sender.Connect(localEndPoint);

                // We print EndPoint information  
                // that we are connected
                ConnectionSuccessBox.Foreground = Brushes.Green;
                ConnectionSuccessBox.Text = "Success ! Socket connected to -> " + sender.RemoteEndPoint.ToString();

                // Indicate that the server is running
                isServerDown = false;

                // Disable the connect button because we are now connected.
                Connect.IsEnabled = false;

                if (listeningLoop == null && updateLoop == null)
                {
                    // Launch the listening thread
                    listeningLoop = new Thread(new ThreadStart(ListeningLoop));
                    listeningLoop.IsBackground = true;
                    listeningLoop.Start();

                    // Launch the update thread
                    updateLoop = new Thread(new ThreadStart(UpdateLoop));
                    updateLoop.IsBackground = true;
                    updateLoop.Start();
                }
            }

            // Manage of Socket's Exceptions 
            catch (Exception)
            {
                MessageBox.Show("Can't connect to the server. Verify that the IP Adrress and the Port you entered are correct, then try again.", "Alert ! ", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        public void ListeningLoop()
        {
            while (true)
            {
                if (isServerDown == false)
                {
                    try
                    {
                        // Data buffer 
                        byte[] bytes = new Byte[1024];
                        string data = null;

                        while (true)
                        {
                            int numByte = sender.Receive(bytes);

                            data += Encoding.ASCII.GetString(bytes,
                                                       0, numByte);

                            if (data.Length > 0)
                                updateSaveList(data);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("The server has been shutdown.", "Alert ! ", MessageBoxButton.OK, MessageBoxImage.Error);

                        Disconnect();
                    }
                }
                else
                {
                    // Nothing until the sever is not up              
                }
            }
        }

        private void Disconnect()
        {
            this.Dispatcher.Invoke(() =>
            {
                savesListBox.ClearValue(ItemsControl.ItemsSourceProperty);
                System.Collections.IEnumerable NewSource = null;
                savesListBox.ItemsSource = NewSource;
                Connect.IsEnabled = true;
                ConnectionSuccessBox.Foreground = Brushes.Red;
                ConnectionSuccessBox.Text = "Not connected yet.";
            });

            isServerDown = true;
        }


        public void UpdateLoop()
        {
            if (isServerDown == false)
            {
                while (true)
                {
                    sendMessage("UpdateSaves");
                    Thread.Sleep(1500);
                }
            }
            else
            {
                // Nothing until the sever is not up     
            }
        }

        public void sendMessage(string message)
        {
            if (isServerDown == false)
            {
                try
                {
                    byte[] messageSent = Encoding.ASCII.GetBytes(message);
                    sender.Send(messageSent);
                }
                catch (Exception)
                {

                }
            }
        }

        public void updateSaveList(string data)
        {
            string[] dataSorted = data.Split(',');
            Array.Sort(dataSorted);

            this.Dispatcher.Invoke(() =>
            {
                savesListBox.ItemsSource = dataSorted;
            });
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            ExecuteClient();
        }

        private void RunSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isServerDown && savesListBox.SelectedIndex != -1)
            {
                string[] result = savesListBox.SelectedItem.ToString().Split('-');
                sendMessage("RunSave," + result[0].Trim());
                MessageBox.Show("Request to run the save \"" + savesListBox.SelectedItem.ToString() + "\" sent !", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RunAllSaves_Click(object sender, RoutedEventArgs e)
        {
            if (!isServerDown && listeningLoop != null && updateLoop != null && listeningLoop != null && updateLoop != null)
            {
                sendMessage("RunAllSaves");
                MessageBox.Show("Request to run all the saves sent !", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void StopSave_Click(object sender, RoutedEventArgs e)
        {
            if (savesListBox.SelectedIndex != -1 && !isServerDown && listeningLoop != null && updateLoop != null)
            {
                string[] result = savesListBox.SelectedItem.ToString().Split('-');
                sendMessage("StopSave," + result[0].Trim());
                MessageBox.Show("Request to stop the save \"" + savesListBox.SelectedItem.ToString().Split('-')[0] + "\" sent !", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PauseSave_Click(object sender, RoutedEventArgs e)
        {
            if (savesListBox.SelectedIndex != -1 && !isServerDown && listeningLoop != null && updateLoop != null)
            {
                string[] result = savesListBox.SelectedItem.ToString().Split('-');
                sendMessage("PauseSave," + result[0].Trim());
                MessageBox.Show("Request to pause the save \"" + savesListBox.SelectedItem.ToString().Split('-')[0] + "\" sent !", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PlaySave_Click(object sender, RoutedEventArgs e)
        {
            if (savesListBox.SelectedIndex != -1 && !isServerDown && listeningLoop != null && updateLoop != null)
            {
                string[] result = savesListBox.SelectedItem.ToString().Split('-');

                sendMessage("PlaySave," + result[0].Trim());

                MessageBox.Show("Request to play the save \"" + savesListBox.SelectedItem.ToString().Split('-')[0] + "\" sent !", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}