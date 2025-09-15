using final_work.Models;
using final_work.Tiedostot;

namespace final_work;

[QueryProperty(nameof(Henkilotunnus), "hetu")]
public partial class HenkiloTiedot : ContentPage
{
    public HenkiloTiedot()
    {
        InitializeComponent();
    }

    //Tarkistetaan onko käyttäjä kirjautunut sisään
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!App.OnKirjautunut)
        {
            await DisplayAlert("Estetty", "Kirjaudu sisään käyttääksesi sovellusta.", "OK");
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }
    }

    private string henkilotunnus = string.Empty;

    //Annetaan henkilön tiedot näkyviin
    public string Henkilotunnus
    {
        get => henkilotunnus;
        set
        {
            henkilotunnus = value;
            _ = NaytaTiedot(value);
        }
    }
    //Käytetään hetua henkilötietojen etsimiseen
    private async Task NaytaTiedot(string hetu)
    {
        var kaikki = await HenkiloTiedostonHallinta.LataaHenkilotAsync();
        var henkilo = kaikki.FirstOrDefault(h => h.Henkilötunnus == hetu);

        if (henkilo != null)
        {
            BindingContext = henkilo;
        }
        else
        {
            await DisplayAlert("Virhe", "Henkilöä ei löytynyt.", "OK");
            await Shell.Current.GoToAsync("//HenkiloLista");
        }
    }
    //Palataan takaisin henkilölistaan
    private async void PalaaKlikattu(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HenkiloLista");
    }
}