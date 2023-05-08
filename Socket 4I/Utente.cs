using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket_4I
{
    public class Utente
    {
        public string Nome
        {
            set;
            get;
        }

        public string IndirizzoIP { set; get; }

        public int Porta { set; get; }


        public Utente(string nome,string indirizzo,int porta)
        {
            Nome = nome;
            IndirizzoIP = indirizzo;
            Porta = porta;
        }


        public override string ToString()
        {
            return Nome;
        }
    }
}
