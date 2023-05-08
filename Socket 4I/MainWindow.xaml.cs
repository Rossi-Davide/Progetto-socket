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
using System.Windows.Threading;

namespace Socket_4I
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        Socket socket = null;

        Contatti contatti = null;

        //mutua esclusione per aggiornamento dati destinatario
        Object locker;
        List<string> incomingMessages;
        Thread listenProcess;
        bool closing = false;
        bool? broadcast;
        string messaggioToSend;
        
        public MainWindow()
        {
            InitializeComponent();

            locker = new Object();

            incomingMessages = new List<string>();

            contatti = new Contatti();


            //aggiungo di default dei contatti
            contatti.AggiungiContatto("marcella","127.0.0.1",11001);
            contatti.AggiungiContatto("marta", "127.0.0.1", 11002);
            contatti.AggiungiContatto("matteo", "127.0.0.1", 11003);
            contatti.AggiungiContatto("luca", "127.0.0.1", 11004);
            contatti.AggiungiContatto("dario", "127.0.0.1", 11005);


            bool passed = true;

            do
            {
                Login loginWindow = new Login(contatti);

                loginWindow.ShowDialog();


                #region AperturaPorta

                try
                {
                    //specifichiamo la famiglia di indirizzi (IPV4), il tipo di protocollo (UDP) e che quindi verranno usati datagrammi
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    //indirizzo ip della socket, IpAddress.Any ritorna 0.0.0.0, vuol dire che saremo in ascolto su tutte le network interfaces della macchina
                    IPAddress local_address = IPAddress.Any;

                    //associo indirizzo ip e porta creando un endpoint
                    IPEndPoint local_endpoint = new IPEndPoint(local_address, contatti.Rubrica[contatti.idUtente].Porta);

                    //associo l'endpoint alla porta
                    socket.Bind(local_endpoint);

                    //la richiesta è non bloccante, il programma non viene messo in pausa finchè la trasmissione non è completa
                    socket.Blocking = false;

                    //inviare o ricevere pacchetti broadcast
                    socket.EnableBroadcast = true;

                    passed = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore nell'apertura della porta, probabilmente è già in uso");
                    //se non sono riuscito ad aprire la socket faccio riloggare l'utente
                    passed = false;
                }

                #endregion
            } while (!passed);

           
            //faccio partire il thread per l'ascolto dei messaggi
            listenProcess = new Thread(listenThread);

            listenProcess.Start();

            //aggiorno ui 
            contattiComb.ItemsSource = null;
            contattiComb.ItemsSource = contatti.Rubrica;

            loginLab.Content = "Loggato come: " + contatti.Rubrica[contatti.idUtente].Nome;
            portaLab.Content = "Porta: " + contatti.Rubrica[contatti.idUtente].Porta;


        }

      


        private void listenThread()
        {
                //interrompo ascolto dei messaggi se sto chiudendo programma       
                while (!closing)
                {
                    try
                    {
                        int nBytes = 0;

                        //nByte pronti per essere letti
                        if ((nBytes = socket.Available) > 0)
                        {
                            
                                //ricezione dei caratteri in attesa
                                byte[] buffer = new byte[nBytes];

                                //verrà memorizzato l'endpoint del mittente, mettiamo dei valori di default
                                EndPoint remoreEndPoint = new IPEndPoint(IPAddress.Any, 0);

                                //leggiamo i datagram in arrivo
                                nBytes = socket.ReceiveFrom(buffer, ref remoreEndPoint);


                                //recuperiamo l'ip del mittente
                                string from = ((IPEndPoint)remoreEndPoint).Address.ToString()+","+ ((IPEndPoint)remoreEndPoint).Port.ToString();

                                string username = contatti.GetNomeFromPort(((IPEndPoint)remoreEndPoint).Port);


                                //recuperiamo il messaggio
                                string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);

                                //messaggio con dettagli
                                incomingMessages.Add(username+","+from + ": " + messaggio);

                                //passo dal thread corrente a quello del main, in modo da poter accedere agli elementi della ui e aggiornarli
                                this.Dispatcher.Invoke(() =>
                                {

                                    //output
                                    lstMessaggi.ItemsSource = null;
                                    lstMessaggi.ItemsSource = incomingMessages;
                                });
                            
                        }

                        //controllo ascolto ogni 250ms
                        Thread.Sleep(250);
                    }
                    catch (Exception ex)
                    {
                        //se si invia un messaggio verso una porta ritornerà indietro un messaggio di errore, se il canale di ricezione è aperto questo genera un'eccezione
                        MessageBox.Show("Errore invio messaggio. Probabilmente si sta tentando di comunicare verso un client non attivo, possibile se inviato un messaggio broadcast");
                    }
                }        
            
        }





       
        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //accedo agli elementi della ui nel main thread
                broadcast = broadcastCheck.IsChecked;

            if (contattiComb.SelectedItem == null)
            {
                throw new Exception("Nessun contatto selezionato");
            }

            messaggioToSend = messaggioT.Text;

            //faccio partire il thread per l'invio del messaggio
            Thread senderProcess = new Thread(Invia);

            senderProcess.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Invia()
        {
            
                

                        if (broadcast== true)
                        {
                            //per l'invio in broadcast ciclo su tutte le porte 
                            for (int i = 0; i < contatti.Rubrica.Count - 1; i++)
                            {
                                if (i != contatti.idUtente)
                                {
                                    IPAddress remote_address = IPAddress.Parse(contatti.Rubrica[i].IndirizzoIP);

                                    IPEndPoint remote_endpoint = new IPEndPoint(remote_address, contatti.Rubrica[i].Porta);

                                    //codifico il messaggio
                                    byte[] messaggio = Encoding.UTF8.GetBytes(messaggioToSend);

                                    //invio il messaggio attraverso la socket verso l'endpoint
                                    socket.SendTo(messaggio, remote_endpoint);
                                }
                            }
                        }
                        else
                        {
                            

                            IPEndPoint remote_endpoint;

                            lock (locker)
                            {
                                //prendo l'ip e la porta dalla rubrica e creo un endpoint
                                IPAddress remote_address = IPAddress.Parse(contatti.Rubrica[contatti.idDestinatario].IndirizzoIP);

                                remote_endpoint = new IPEndPoint(remote_address, contatti.Rubrica[contatti.idDestinatario].Porta);
                            }
                            


                            //codifico il messaggio
                            byte[] messaggio = Encoding.UTF8.GetBytes(messaggioToSend);

                            //invio il messaggio attraverso la socket verso l'endpoint
                            socket.SendTo(messaggio, remote_endpoint);
                        }
                    
               
                             
            
        }

        private void contattiComb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
                if (contattiComb.SelectedItem != null)
                {
                    try
                    {
                        if (contattiComb.SelectedIndex == contatti.idUtente)
                        {
                            throw new Exception("Non si può selezionare l'utente correntemente in uso");
                        }

                        //update del destinatario
                    lock (locker)
                    {
                        contatti.idDestinatario = contattiComb.SelectedIndex;
                    }

                        indirizzoLabel.Content = "Indirizzo: " + contatti.Rubrica[contatti.idDestinatario].IndirizzoIP;
                        portaLabel.Content = "Porta: " + contatti.Rubrica[contatti.idDestinatario].Porta;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        contattiComb.SelectedItem = null;

                    }
                }
            

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            closing = true;
        }
    }
}
