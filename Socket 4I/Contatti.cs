using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Socket_4I
{
    public class Contatti
    {
        public List<Utente> Rubrica {set; get;}
        public int idUtente { set; get; }
        public int idDestinatario { set; get; }


        public Contatti()
        {
            Rubrica = new List<Utente>();
            idUtente = 0;
            idDestinatario = 0;
        }

        public void AggiungiContatto(string nome,string indirizzo,int porta)
        {
            Rubrica.Add(new Utente(nome, indirizzo, porta));
        }

        public string GetNomeFromPort(int port)
        {
            foreach(Utente u in Rubrica)
            {
                if(port == u.Porta)
                {
                    return u.Nome;
                }
            }

            return "not found";
        }
        
    }
}
