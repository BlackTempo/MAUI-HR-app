using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace final_work.Tiedostot
{
    class Salaus //Luodaan Caesar salausluokka, joka salaa ja purkaa merkkijonot käyttämällä arvoa 4, eli A -> E, B -> F jne.
    {
        private const int Siirto = 4; //luodaan pysyvä arvo, joka on 4. Näin se on helppo muuttaa, jos halutaan

        public static string Salaa(string teksti) //Salausmetodi
        {
            return new string(teksti.Select(m => SiirraMerkki(m, Siirto)).ToArray());
        }

        public static string Pura(string teksti) //Purkamismetodi
        {
            return new string(teksti.Select(m => SiirraMerkki(m, -Siirto)).ToArray());
        }

        private static char SiirraMerkki(char merkki, int siirto = Siirto) //Siirtää merkkiä annetun arvon (Siirto) mukaan
        {
            if (char.IsLetter(merkki))
            {
                char perus = char.IsUpper(merkki) ? 'A' : 'a';
                int uusi = (merkki - perus + siirto + 26) % 26;
                return (char)(perus + uusi);
            }

            // ei muuteta muuta kuin kirjaimia, koska muuten tällä salauksella data rikkoontuu JSON-tiedostossa
            return merkki;
        }
    }
}