using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using final_work.Models;

namespace final_work.Tiedostot
{
    public static class HenkiloTiedostonHallinta //Käytetään henkilötietojen hallintaan, eli tiedostojen lukemiseen ja kirjoittamiseen
    {
        private static readonly string tiedostoPolku =
            Path.Combine(FileSystem.Current.AppDataDirectory, "henkilot.json");
        //Lataa henkilöt tiedostosta
        public static async Task<List<Henkilo>> LataaHenkilotAsync()
        {
            if (!File.Exists(tiedostoPolku))
                return new List<Henkilo>();
            try
            {
                var salattu = await File.ReadAllTextAsync(tiedostoPolku);
                var purettu = Salaus.Pura(salattu); //Purkaa salauksen
                return JsonSerializer.Deserialize<List<Henkilo>>(purettu) ?? new List<Henkilo>();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Lukuvirhe", "Tiedostoa ei voitu lukea: " + ex.Message, "OK");
                return new List<Henkilo>();
            }
        }
        //Tallenna henkilöt tiedostoon
        public static async Task TallennaHenkilotAsync(List<Henkilo> henkilot)
        {
            var json = JsonSerializer.Serialize(henkilot);
            var salattu = Salaus.Salaa(json); //Salataan tiedot ennen tallentamista
            await File.WriteAllTextAsync(tiedostoPolku, salattu);
        }
    }
}