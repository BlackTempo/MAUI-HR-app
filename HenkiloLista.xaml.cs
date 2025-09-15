using final_work.Models;
using final_work.Tiedostot;
using System.Collections.ObjectModel;

namespace final_work;

public partial class HenkiloLista : ContentPage
{
    private ObservableCollection<Henkilo> henkiloObservableLista = new();

    public HenkiloLista()
    {
        InitializeComponent();
        henkiloListaView.ItemsSource = henkiloObservableLista;
    }

    //Tarkistetaan onko käyttäjä kirjautunut sisään ja ladataan henkilöt tiedostosta
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!App.OnKirjautunut)
        {
            await DisplayAlert("Estetty", "Kirjaudu sisään käyttääksesi sovellusta.", "OK");
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }

        var ladatut = await HenkiloTiedostonHallinta.LataaHenkilotAsync();

        henkiloObservableLista.Clear(); // Tyhjennetään vanhat (jos sellaisia on) ja ladataan uudet tilalle
        foreach (var i in ladatut)
            henkiloObservableLista.Add(i);

        // Oletusjärjestys: sukunimi (A–Z)
        onNouseva = true;
        jarjestysSuuntaNappi.Text = "↑";
        jarjestysPicker.SelectedIndex = 1; // laukaisee myös JarjestysValittu
    }

    private async void HenkiloValittu(object sender, SelectionChangedEventArgs e) //Lisätään valinta, kun henkilöä klikataan 
    {
        var valittu = e.CurrentSelection.FirstOrDefault() as Henkilo;
        if (valittu == null)
            return;

        string kokoNimi = $"{valittu.Etunimet} {valittu.Sukunimi}";
        string hetu = valittu.Henkilötunnus;

        string valinta = await DisplayActionSheet( //Luodaan toimintavaihtoehdot käyttäjälle henkilön valittuaan
            $"Toiminnot henkilölle '{valittu.Kutsumanimi}'?",
            "Peruuta",
            null,
            "Näytä",
            "Muokkaa",
            "Poista");
        //Tehdään switch-case valinnalla vaihtoehdot
        switch (valinta)
        {
            case "Näytä":
                await Shell.Current.GoToAsync($"///HenkiloTiedot?hetu={Uri.EscapeDataString(hetu)}");
                break;
            case "Poista":
                bool vahvistus = await DisplayAlert(
                    "Vahvista poisto",
                    $"Poistetaanko henkilö '{kokoNimi}'?",
                    "Kyllä", "Ei");

                if (vahvistus)
                {
                    henkiloObservableLista.Remove(valittu);
                    var kaikki = await HenkiloTiedostonHallinta.LataaHenkilotAsync();
                    kaikki.RemoveAll(h => h.Henkilötunnus == hetu);
                    await HenkiloTiedostonHallinta.TallennaHenkilotAsync(kaikki);
                }
                break;

            case "Muokkaa":
                try
                {
                    if (!string.IsNullOrWhiteSpace(hetu))
                    {
                        await Shell.Current.GoToAsync($"///MuokkaaHenkilo?hetu={Uri.EscapeDataString(hetu)}");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Virhe", ex.ToString(), "OK");
                }
                break;

            default:
                break;
        }

        henkiloListaView.SelectedItem = null; //Nollataan valinta
    }
    //Siirrytään Lisää henkilö -sivulle
    private async void LisaaHenkiloKlikattu(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LisaaHenkilo");
    }

    //Luodaan valitsin järjestyksen valitsemiseksi
    private bool onNouseva = true;

    private async void JarjestysValittu(object sender, EventArgs e)
    {
        await JarjestaLista();
    }

    private void VaihdaJarjestysSuunta(object sender, EventArgs e)
    {
        onNouseva = !onNouseva;
        jarjestysSuuntaNappi.Text = onNouseva ? "↑" : "↓";
        _ = JarjestaLista();
    }
    //Luodaan ehtoja, joiden mukaan listaa voidaan järjestellä
    private async Task JarjestaLista()
    {
        if (henkiloObservableLista == null)
            return;

        var jarjestys = jarjestysPicker.SelectedItem?.ToString();

        IEnumerable<Henkilo> jarjestetty = henkiloObservableLista;

        switch (jarjestys)
        {
            case "Kutsumanimi":
                jarjestetty = onNouseva
                    ? henkiloObservableLista.OrderBy(h => h.Kutsumanimi)
                    : henkiloObservableLista.OrderByDescending(h => h.Kutsumanimi);
                break;

            case "Sukunimi":
                jarjestetty = onNouseva
                    ? henkiloObservableLista.OrderBy(h => h.Sukunimi)
                    : henkiloObservableLista.OrderByDescending(h => h.Sukunimi);
                break;

            case "Nimike":
                jarjestetty = onNouseva
                    ? henkiloObservableLista.OrderBy(h => h.Nimike)
                    : henkiloObservableLista.OrderByDescending(h => h.Nimike);
                break;
        }

        henkiloObservableLista = new ObservableCollection<Henkilo>(jarjestetty);
        henkiloListaView.ItemsSource = henkiloObservableLista;
    }
}