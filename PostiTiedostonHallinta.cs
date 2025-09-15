using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;
using final_work.Models;

namespace final_work.Tiedostot;

public static class PostiTiedostonHallinta // Käytetään postitietojen hallintaan, jotta voidaan tallentaa ja ladata postitiedot tiedostosta eikä käyttäjien poistaminen vaikuta tietoihin
{
    private static readonly string tiedostoPolku =
        Path.Combine(FileSystem.Current.AppDataDirectory, "postidata.json");

    public static async Task<List<PostiTieto>> LataaPostitAsync()
    {
        if (!File.Exists(tiedostoPolku))
            return new List<PostiTieto>();

        var json = await File.ReadAllTextAsync(tiedostoPolku);
        return JsonSerializer.Deserialize<List<PostiTieto>>(json) ?? new List<PostiTieto>();
    }

    public static async Task TallennaPostitAsync(List<PostiTieto> postit)
    {
        var json = JsonSerializer.Serialize(postit);
        await File.WriteAllTextAsync(tiedostoPolku, json);
    }

    public static async Task LisaaTaiPaivitaPostiAsync(string postinumero, string postitoimipaikka) //Tärkeä metodi, joka ylläpitää postitiedot ajan tasalla
    {
        var postit = await LataaPostitAsync();

        if (!postit.Any(p => p.Postinumero == postinumero))
        {
            postit.Add(new PostiTieto
            {
                Postinumero = postinumero,
                Postitoimipaikka = postitoimipaikka
            });

            await TallennaPostitAsync(postit);
        }
    }
}
