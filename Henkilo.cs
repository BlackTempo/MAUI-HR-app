using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace final_work.Models
{
    public class Henkilo
    {
        // Henkilön perustiedot
        public string Etunimet { get; set; } = string.Empty;
        public string Sukunimi { get; set; } = string.Empty;
        public string Kutsumanimi { get; set; } = string.Empty;
        public string Henkilötunnus { get; set; } = string.Empty;

        // Kotiosoite
        public string Katuosoite { get; set; } = string.Empty;
        public string Postinumero { get; set; } = string.Empty;
        public string Postitoimipaikka { get; set; } = string.Empty;

        // Toimisuhde
        public DateTime Alkamispäivä { get; set; }
        public DateTime? Päättymispäivä { get; set; } // null = toistaiseksi voimassa oleva
        public string Nimike { get; set; } = string.Empty;
        public string Yksikkö { get; set; } = string.Empty;
        public string PaattymispaivaTeksti
        {
            get
            {
                return Päättymispäivä.HasValue
                    ? Päättymispäivä.Value.ToString("dd.MM.yyyy")
                    : "Toistaiseksi voimassa oleva";
            }
        }
    }
}
