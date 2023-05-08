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

namespace Socket_4I
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {

        Contatti ContattiLogin = null;
        public Login(Contatti contatti)
        {
            InitializeComponent();

            ContattiLogin = contatti;

            utentiComb.ItemsSource = null;
            utentiComb.ItemsSource = ContattiLogin.Rubrica;
            utentiComb.SelectedIndex = 0;
        }

        private void utentiComb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (utentiComb.SelectedItem != null)
            {
                Utente selected = utentiComb.SelectedItem as Utente;

                indirizzoLabel.Content = "Indirizzo: " + selected.IndirizzoIP;
                portaLabel.Content = "Porta: " + selected.Porta;
            }
        }

        private void entraB_Click(object sender, RoutedEventArgs e)
        {
            ContattiLogin.idUtente = utentiComb.SelectedIndex;
            this.Close();
        }
    }
}
