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

    //Tarkistetaan onko k�ytt�j� kirjautunut sis��n
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!App.OnKirjautunut)
        {
            await DisplayAlert("Estetty", "Kirjaudu sis��n k�ytt��ksesi sovellusta.", "OK");
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }
    }

    private string henkilotunnus = string.Empty;

    //Annetaan henkil�n tiedot n�kyviin
    public string Henkilotunnus
    {
        get => henkilotunnus;
        set
        {
            henkilotunnus = value;
            _ = NaytaTiedot(value);
        }
    }
    //K�ytet��n hetua henkil�tietojen etsimiseen
    private async Task NaytaTiedot(string hetu)
    {
        var kaikki = await HenkiloTiedostonHallinta.LataaHenkilotAsync();
        var henkilo = kaikki.FirstOrDefault(h => h.Henkil�tunnus == hetu);

        if (henkilo != null)
        {
            BindingContext = henkilo;
        }
        else
        {
            await DisplayAlert("Virhe", "Henkil�� ei l�ytynyt.", "OK");
            await Shell.Current.GoToAsync("//HenkiloLista");
        }
    }
    //Palataan takaisin henkil�listaan
    private async void PalaaKlikattu(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HenkiloLista");
    }
}