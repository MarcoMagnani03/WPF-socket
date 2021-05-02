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
//aggiunta delle seguenti librerie
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace APP_WPF_socket
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {

            //creiamo il source socket prendendo l'indirizzo Ip del nostro pc e inserendo un porta libera 
            IPEndPoint sourceSocket = new IPEndPoint(IPAddress.Parse("192.168.1.194"),56000);
            //creiamo il destination socket. Insserendo i valori inseriti nelle rispettive textBox
            string[] numeroIp = txtIP.Text.Split('.');
            btnInvia.IsEnabled = true;
            if (numeroIp.Length != 4)
            {
                btnInvia.IsEnabled = false;
                lblErrore.Content = "L'indirizzo IP selezionato è scorretto";
                lblErrore.Visibility = Visibility.Visible;
            }
            int tmp;
            for (int i = 0; i < numeroIp.Length; i++)
            {
                if (!int.TryParse(numeroIp[i],out tmp))
                {
                    btnInvia.IsEnabled = false;
                    lblErrore.Content = "L'indirizzo IP selezionato è scorretto, sono presenti dei caratteri scorretti";
                    lblErrore.Visibility = Visibility.Visible;
                }
            }
            if (btnInvia.IsEnabled)
            {
                for (int i = 0; i < numeroIp.Length; i++)
                {
                    if (int.Parse(numeroIp[i]) < 0 || int.Parse(numeroIp[i]) > 255)
                    {
                        btnInvia.IsEnabled = false;
                        lblErrore.Content = $"L'indirizzo IP selezionato contiene un valore inferiore allo 0 o superiore al 255";
                        lblErrore.Visibility = Visibility.Visible;
                    }
                }
            }
            if(!int.TryParse((txtPort.Text),out tmp))
            {
                lblErrore.Content = "La porta inserita è scorretta";
            }
            if (btnInvia.IsEnabled)
            {
                lblErrore.Visibility = Visibility.Visible;
            }
            Thread ricezione = new Thread(new ParameterizedThreadStart(SocketReceive));
            ricezione.Start(sourceSocket);
        }
        //async fa in modo che mentre il thread è in ascolto l'interfaccia non si interrompe
        public async void SocketReceive(object sockSource)
        {
            IPEndPoint ipendp = (IPEndPoint)sockSource;
            
            Socket t = new Socket(ipendp.AddressFamily,SocketType.Dgram,ProtocolType.Udp);
            
            t.Bind(ipendp);

            Byte[] bytesRicevuti = new Byte[256];

            string message;

            int contaCaratteri=0;

            //qunando sono qua dentro non blocco l'interfaccia. await va abbinato ad async
            await Task.Run(() =>
            {
                while (true)
                {
                    //andiamo a verificare se sul socket abbiamo ricevuto qualcosa
                    if (t.Available > 0)
                    {
                        message = "";
                        //quello che riceve sul socket lo codifica in ASCII e lo mette dentro message
                        contaCaratteri = t.Receive(bytesRicevuti,bytesRicevuti.Length,0);
                        message = message + Encoding.ASCII.GetString(bytesRicevuti,0,contaCaratteri);
                        //si usa quando nei thread bisogna aggiornare le interfacce. Se non lo si usa da errore
                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            lblRicevi.Content = message;
                        }));
                    }
                }
            });
        }
        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            //Aggiungere controlli sul contenuto delle textbox
            string ipAddress = txtIP.Text;
            int port = int.Parse(txtPort.Text);
            string messaggioInviato = $"Messaggio inviato: {txtMessage.Text} al socket {ipAddress} : {port} il {DateTime.Now}";
            lstInviati.Items.Add(messaggioInviato);
            SocketSend(IPAddress.Parse(ipAddress), port, txtMessage.Text);
        }
        public void SocketSend(IPAddress dest,int destport,string message)
        {
            //transformiamo il messaggio in byte
            Byte[] byteInviati = Encoding.ASCII.GetBytes(message);
            Socket s = new Socket(dest.AddressFamily,SocketType.Dgram,ProtocolType.Udp);

            //Andiamo a creare il socket del destinatario
            IPEndPoint remote_endpoint=new IPEndPoint(dest,destport);
            s.SendTo(byteInviati, remote_endpoint);
        }
    }
}
