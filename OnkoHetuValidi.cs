using final_work.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace final_work.Tiedostot
{
    public class Hetutarkistus //Luodaan henkilötunnuksen tarkistusluokka, joka tarkistaa henkilötunnuksen oikeellisuuden
    {
        public static bool OnkoHetuValidi(string hetu)
        {
            if (string.IsNullOrWhiteSpace(hetu) || hetu.Length != 11)
                return false;

            // Vain sallitut merkit ja jako-osa
            string syntyosa = hetu.Substring(0, 6);
            char erotin = hetu[6];
            string yksilo = hetu.Substring(7, 3);
            char tarkistusMerkki = char.ToUpper(hetu[10]);

            if (!"+-A".Contains(erotin) || !syntyosa.All(char.IsDigit) || !yksilo.All(char.IsDigit))
                return false;

            string yhdeksanNumeroa = syntyosa + yksilo;

            if (!long.TryParse(yhdeksanNumeroa, out long luku))
                return false;

            int jakojaannos = (int)(luku % 31);

            // Tarkistusmerkkitaulukko
            string tarkistusmerkit = "0123456789ABCDEFHJKLMNPRSTUVWXY";

            char oikeaMerkki = tarkistusmerkit[jakojaannos];

            return tarkistusMerkki == oikeaMerkki;
        }
    }
}