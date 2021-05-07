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
        static private string simbolo;
        static private string message;
        static private string simboloConfermato;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtPort.Text = "56000";
            txtIP.Focus();
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            IPEndPoint sourceSocket = new IPEndPoint(IPAddress.Parse(localIP), 56000);
            Thread ricezione = new Thread(new ParameterizedThreadStart(SocketReceive));
            ricezione.Start(sourceSocket);
        }
        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            //creiamo il source socket prendendo l'indirizzo Ip del nostro pc e inserendo un porta libera
           
            string[] numeroIp = txtIP.Text.Split('.');
            btnGioca.IsEnabled = true;
            //controllo contenuto delle textBox
            if (numeroIp.Length != 4)
            {
                btnGioca.IsEnabled = false;
                lblErrore.Content = "L'indirizzo IP selezionato è errato";
                lblErrore.Visibility = Visibility.Visible;
            }
            int tmp;
            for (int i = 0; i < numeroIp.Length; i++)
            {
                if (!int.TryParse(numeroIp[i],out tmp))
                {
                    btnGioca.IsEnabled = false;
                    lblErrore.Content = $"L'indirizzo IP selezionato è errato \n      sono presenti dei caratteri\n                 non numerici";
                    lblErrore.Visibility = Visibility.Visible;
                }
            }
            if (btnGioca.IsEnabled)
            {
                for (int i = 0; i < numeroIp.Length; i++)
                {
                    if (int.Parse(numeroIp[i]) < 0 || int.Parse(numeroIp[i]) > 255)
                    {
                        btnGioca.IsEnabled = false;
                        lblErrore.Content = $"L'indirizzo IP selezionato contiene\n          un valore inferiore allo 0\n                o superiore al 255";
                        lblErrore.Visibility = Visibility.Visible;
                    }
                }
            }
            if(!int.TryParse((txtPort.Text),out tmp))
            {
                lblErrore.Content = "La porta inserita non è valida";
                btnGioca.IsEnabled = false;
            }
            if (btnGioca.IsEnabled)
            {
                lblErrore.Visibility = Visibility.Visible;
            }
        }
        //async fa in modo che mentre il thread è in ascolto l'interfaccia non si interrompe
        public async void SocketReceive(object sockSource)
        {
            IPEndPoint ipendp = (IPEndPoint)sockSource;
            
            Socket t = new Socket(ipendp.AddressFamily,SocketType.Dgram,ProtocolType.Udp);
            
            t.Bind(ipendp);
            


            Byte[] bytesRicevuti = new Byte[256];

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
                        string tmp="";
                        //quello che riceve sul socket lo codifica in ASCII e lo mette dentro message
                        contaCaratteri = t.Receive(bytesRicevuti,bytesRicevuti.Length,0);
                        message = message + Encoding.ASCII.GetString(bytesRicevuti,0,contaCaratteri);
                        //Qui controllo se il messaggio che mi è arrivato è per la sincronizzazione o se è perchè mi è arrivato il simbolo selezionato dall'avversario
                        if (message == "partenza")
                        {
                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                btnConferma.IsEnabled = true;
                            }));
                        }
                        else
                        {
                            ControlloVittoria();
                        } 
                    }
                }
            });
            
        }
        public void SocketSend(IPAddress dest,int destport,string messaggio)
        {
            //transformiamo il messaggio in byte
            Byte[] byteInviati = Encoding.ASCII.GetBytes(messaggio);
            Socket s = new Socket(dest.AddressFamily,SocketType.Dgram,ProtocolType.Udp);
            ControlloVittoria();
            //Andiamo a creare il socket del destinatario
            IPEndPoint remote_endpoint = new IPEndPoint(dest, destport);
            s.SendTo(byteInviati, remote_endpoint);
        }
        private void ControlloVittoria()
        {
            string vittoria = "";
            switch (message)
            {
                //QUI controllo quale dei due utenti ha vinto controllando le varie casistiche, lo devo controllare anche qui perchè altrimenti la vittoria me la da solamente da chi riceve il secondo turno.
                case "sasso":
                    switch (simbolo)
                    {
                        case "sasso":
                            vittoria = "Anche il tuo avversaro ha scelto sasso è un PAREGGIO";
                            break;
                        case "carta":
                            vittoria = "Il tuo avversario ha scelto sasso mentre tu carta hai VINTO";
                            break;
                        case "forbici":
                            vittoria = "Il tuo avversario ha scelto sasso mentre tu forbici hai PERSO";
                            break;
                    }
                    break;
                case "carta":
                    switch (simbolo)
                    {
                        case "sasso":
                            vittoria = "Il tuo avversario ha scelto carta mentre tu sasso hai PERSO";
                            break;
                        case "carta":
                            vittoria = "Anche il tuo avversaro ha scelto carta è un PAREGGIO";
                            break;
                        case "forbici":
                            vittoria = "Il tuo avversario ha scelto carta mentre tu forbici hai VINTO";
                            break;
                    }
                    break;
                case "forbici":
                    switch (simbolo)
                    {
                        case "sasso":
                            vittoria = "Il tuo avversario ha scelto forbici mentre tu sasso hai VINTO";
                            break;
                        case "carta":
                            vittoria = "Il tuo avversario ha scelto forbici mentre tu carta hai PERSO";
                            break;
                        case "forbici":
                            vittoria = "Anche il tuo avversaro ha scelto forbici è un PAREGGIO";
                            break;
                    }
                    break;
            }
            this.Dispatcher.BeginInvoke(new Action(() =>
            {

                lblVittoria.Content = vittoria;
                lblVittoria.Visibility = Visibility.Visible;
                if (vittoria.Contains("PERSO"))
                {
                    lblVittoria.Foreground = Brushes.Red;
                }
                if (vittoria.Contains("VINTO"))
                {
                    lblVittoria.Foreground = Brushes.Green;
                }
                if (vittoria.Contains("PAREGGIO"))
                {
                    lblVittoria.Foreground = Brushes.Blue;
                }
                if (lblVittoria.Content.ToString() != "")
                {
                    btnRigioca.Visibility = Visibility.Visible;
                }
                else
                {
                    lblVittoria.Content = "Attendi la giocata dell'avversario...";
                }
            }));


        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            simboloConfermato = ((Button)sender).Name.Substring(3);
            switch (simbolo)
            {
                case "sasso":
                    btncarta.IsEnabled = true;
                    btnsasso.IsEnabled = false;
                    btnforbici.IsEnabled = true;
                    lblCartaSelezionata.Visibility = Visibility.Hidden;
                    lblForbiceSelezionata.Visibility = Visibility.Hidden;
                    lblSassoSelezionato.Visibility = Visibility.Visible;
                    break;
                case "carta":
                    btncarta.IsEnabled = false;
                    btnsasso.IsEnabled = true;
                    btnforbici.IsEnabled = true;
                    lblCartaSelezionata.Visibility = Visibility.Visible;
                    lblForbiceSelezionata.Visibility = Visibility.Hidden;
                    lblSassoSelezionato.Visibility = Visibility.Hidden;
                    break;
                case "forbici":
                    btncarta.IsEnabled = true;
                    btnsasso.IsEnabled = true;
                    btnforbici.IsEnabled = false;
                    lblCartaSelezionata.Visibility = Visibility.Hidden;
                    lblForbiceSelezionata.Visibility = Visibility.Visible;
                    lblSassoSelezionato.Visibility = Visibility.Hidden;
                    break;
            }
        }

        private void btnGioca_Click(object sender, RoutedEventArgs e)
        {
            Height = 400;
            txtIP.Visibility = Visibility.Hidden;
            txtPort.Visibility = Visibility.Hidden;
            lblIndirizzoIp.Visibility = Visibility.Hidden;
            lblPorta.Visibility = Visibility.Hidden;
            btncarta.Visibility = Visibility.Visible;
            btnforbici.Visibility = Visibility.Visible;
            btnsasso.Visibility = Visibility.Visible;
            btnCreaSocket.Visibility = Visibility.Hidden;
            lblFaiScelta.Visibility = Visibility.Visible;
            btnGioca.Visibility = Visibility.Hidden;
            imgCarta.Visibility = Visibility.Visible;
            imgForbici.Visibility = Visibility.Visible;
            imgSasso.Visibility = Visibility.Visible;
            btnConferma.Visibility = Visibility.Visible;
            //Qui invio un messaggio al destinatario con partenza. Il bottone conferma del destinatario si accenderà solo quando riceverà questo messaggio
            string ipAddress = txtIP.Text;
            int port = int.Parse(txtPort.Text);
            SocketSend(IPAddress.Parse(ipAddress), port, "partenza");
        }

        private void btnConferma_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = txtIP.Text;
            int port = int.Parse(txtPort.Text);
            simbolo = simboloConfermato;
            lblVittoria.Content = "Attendi la giocata dell'avversario...";
            lblVittoria.Visibility = Visibility.Visible;
            SocketSend(IPAddress.Parse(ipAddress), port, simbolo);
            btnConferma.Visibility = Visibility.Hidden;
            lblFaiScelta.Visibility = Visibility.Hidden;
            btnsasso.Visibility = Visibility.Hidden;
            btnforbici.Visibility = Visibility.Hidden;
            btncarta.Visibility = Visibility.Hidden;
        }


        private void btnRigioca_Click(object sender, RoutedEventArgs e)
        {
            btnRigioca.Visibility = Visibility.Hidden;
            lblVittoria.Visibility = Visibility.Hidden;
            btncarta.Visibility = Visibility.Visible;
            btnforbici.Visibility = Visibility.Visible;
            btnsasso.Visibility = Visibility.Visible;
            btnConferma.Visibility = Visibility.Visible;
            lblFaiScelta.Visibility = Visibility.Visible;
            lblVittoria.Content = "";
            simbolo = "";
            simboloConfermato = "";
            message = "";
        }
    }
}
