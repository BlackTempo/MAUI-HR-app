using final_work.Models;
using final_work.Tiedostot;

namespace final_work;

[QueryProperty(nameof(Henkilotunnus), "hetu")]
public partial class MuokkaaHenkilo : ContentPage
{
    private Henkilo? muokattavaHenkilo;
    private string henkilotunnus = string.Empty;


    public MuokkaaHenkilo()
    {
        InitializeComponent();
    }
    public string Henkilotunnus
    {
        get => henkilotunnus;
        set
        {
            henkilotunnus = value;
            _ = AlustaTiedot(value);
        }
    }
    //Tarkistetaan onko käyttäjä kirjautunut sisään ja ladataan postitunnukset tiedostosta
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!App.OnKirjautunut)
        {
            await DisplayAlert("Estetty", "Kirjaudu sisään käyttääksesi sovellusta.", "OK");
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }
        var ladatut = await PostiTiedostonHallinta.LataaPostitAsync();
        kaikkiPostit = ladatut
            .Where(p => !string.IsNullOrWhiteSpace(p.Postinumero) && !string.IsNullOrWhiteSpace(p.Postitoimipaikka))
            .Select(p => (p.Postinumero, p.Postitoimipaikka))
            .ToList();
    }

    private async Task AlustaTiedot(string hetu)
    {
        var kaikki = await HenkiloTiedostonHallinta.LataaHenkilotAsync();
        muokattavaHenkilo = kaikki.FirstOrDefault(i => i.Henkilötunnus == hetu);

        if (muokattavaHenkilo != null)
        {
            etunimetEntry.Text = muokattavaHenkilo.Etunimet;
            sukunimiEntry.Text = muokattavaHenkilo.Sukunimi;
            kutsumanimiEntry.Text = muokattavaHenkilo.Kutsumanimi;
            hetuEntry.Text = muokattavaHenkilo.Henkilötunnus;

            katuosoiteEntry.Text = muokattavaHenkilo.Katuosoite;
            postinumeroEntry.Text = muokattavaHenkilo.Postinumero;
            postitoimipaikkaEntry.Text = muokattavaHenkilo.Postitoimipaikka;

            alkamispaivaPicker.Date = muokattavaHenkilo.Alkamispäivä;

            if (muokattavaHenkilo.Päättymispäivä.HasValue)
            {
                paattumispaivaPicker.Date = muokattavaHenkilo.Päättymispäivä.Value;
                toistaiseksiCheck.IsChecked = false;
                paattumispaivaPicker.IsVisible = true;
            }
            else
            {
                toistaiseksiCheck.IsChecked = true;
                paattumispaivaPicker.IsVisible = false;
            }

            nimikeEntry.Text = muokattavaHenkilo.Nimike;
            yksikkoPicker.SelectedItem = muokattavaHenkilo.Yksikkö;
        }
        else
        {
            await DisplayAlert("Virhe", "Henkilön tietoja ei löytynyt.", "OK");
            await Shell.Current.GoToAsync("//HenkiloLista");
        }
    }

    private void ToistaiseksiCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        bool piilotetaan = e.Value;

        paattumisLabel.IsVisible = !piilotetaan;
        paattumispaivaPicker.IsVisible = !piilotetaan;
    }

    private List<(string Postinumero, string Postitoimipaikka)> kaikkiPostit = new();



    private async void PostinumeroChanged(object sender, TextChangedEventArgs e) //Ehdottaa postinumeroa olemassa olevasta tiedosta
    {
        string syote = e.NewTextValue?.Trim() ?? "";

        if (syote.Length < 2)
        {
            postinumeroEhdotukset.IsVisible = false;
            return;
        }

        var henkilot = await HenkiloTiedostonHallinta.LataaHenkilotAsync();

        kaikkiPostit = henkilot
            .Where(h => !string.IsNullOrWhiteSpace(h.Postinumero) && !string.IsNullOrWhiteSpace(h.Postitoimipaikka))
            .Select(h => (h.Postinumero, h.Postitoimipaikka))
            .Distinct()
            .ToList();

        var ehdotukset = kaikkiPostit
            .Where(p => p.Postinumero.StartsWith(syote))
            .Select(p => p.Postinumero)
            .Distinct()
            .ToList();

        postinumeroEhdotukset.ItemsSource = ehdotukset;
        postinumeroEhdotukset.IsVisible = ehdotukset.Any();
    }

    private void PostinumeroValittu(object sender, SelectionChangedEventArgs e) //Valitaan ehdotettu postinumero
    {
        string valittu = e.CurrentSelection.FirstOrDefault() as string;

        if (!string.IsNullOrWhiteSpace(valittu))
        {
            postinumeroEntry.Text = valittu;

            // Hae postitoimipaikka samaan aikaan
            var toimipaikka = kaikkiPostit
                .FirstOrDefault(p => p.Postinumero == valittu).Postitoimipaikka;

            if (!string.IsNullOrWhiteSpace(toimipaikka))
                postitoimipaikkaEntry.Text = toimipaikka;

            postinumeroEhdotukset.IsVisible = false;
        }

        postinumeroEhdotukset.SelectedItem = null;
    }

    private List<string> kaikkiPostitoimipaikat = new();

    private void PostitoimipaikkaChanged(object sender, TextChangedEventArgs e) //Ehdottaa postitoimipaikkaa olemassa olevasta tiedosta
    {
        string syote = e.NewTextValue?.ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(syote) || syote.Length < 2)
        {
            postitoimipaikkaEhdotukset.IsVisible = false;
            return;
        }

        var ehdotukset = kaikkiPostit
            .Select(p => p.Postitoimipaikka)
            .Where(p => !string.IsNullOrWhiteSpace(p) && p.ToLower().StartsWith(syote))
            .Distinct()
            .OrderBy(p => p)
            .ToList();

        postitoimipaikkaEhdotukset.ItemsSource = ehdotukset;
        postitoimipaikkaEhdotukset.IsVisible = ehdotukset.Any();
    }

    private void PostitoimipaikkaValittu(object sender, SelectionChangedEventArgs e) //Valitaan ehdotettu postitoimipaikka
    {
        string valittu = e.CurrentSelection.FirstOrDefault() as string;
        if (!string.IsNullOrWhiteSpace(valittu))
        {
            postitoimipaikkaEntry.Text = valittu;
            postitoimipaikkaEhdotukset.IsVisible = false;
        }

        postitoimipaikkaEhdotukset.SelectedItem = null;
    }

    private async void TallennaKlikattu(object sender, EventArgs e)
    {

        if (muokattavaHenkilo == null)
        {
            await DisplayAlert("Virhe", "Muokattavaa henkilöä ei ole ladattu.", "OK");
            return;
        }

        // Luetaan kentistä tiedot
        string etunimet = etunimetEntry.Text ?? "";
        string sukunimi = sukunimiEntry.Text ?? "";
        string kutsumanimi = kutsumanimiEntry.Text ?? "";
        string hetu = hetuEntry.Text ?? "";
        string katuosoite = katuosoiteEntry.Text ?? "";
        string postinumero = postinumeroEntry.Text ?? "";
        string postitoimipaikka = postitoimipaikkaEntry.Text ?? "";
        DateTime alkaminen = alkamispaivaPicker.Date;
        DateTime? paattyminen = toistaiseksiCheck.IsChecked ? (DateTime?)null : paattumispaivaPicker.Date;
        string nimike = nimikeEntry.Text ?? "";
        string yksikko = yksikkoPicker.SelectedItem?.ToString() ?? "";

        //Tästä alkaa tarkistukset

        //sallitaan OnkoValidiNimellä myös välilyönnit ja yhdismerkit tarkistuksessa
        bool OnkoValidiNimi(string nimi) =>
            nimi.All(c => char.IsLetter(c) || c == ' ' || c == '-');

        // Pakolliset kentät
        if (string.IsNullOrWhiteSpace(etunimet) ||
            string.IsNullOrWhiteSpace(sukunimi) ||
            string.IsNullOrWhiteSpace(hetu) ||
            string.IsNullOrWhiteSpace(kutsumanimi) ||
            string.IsNullOrWhiteSpace(postitoimipaikka))
        {
            await DisplayAlert("Virhe", "Nimet, hetu ja postitoimipaikka ovat pakollisia.", "OK");
            return;
        }

        //Tarkistetaan, että nimien kohdalla on vain kirjaimia
        if (!OnkoValidiNimi(etunimet) || !OnkoValidiNimi(sukunimi) || !OnkoValidiNimi(kutsumanimi))
        {
            await DisplayAlert("Virhe", "Etunimi ja sukunimi voivat sisältää vain kirjaimia, välilyöntejä tai yhdysmerkkejä.", "OK");
            return;
        }

        // Käytetään erillistä tiedostoa tarkistamiseen, jotta koodi pysyy siistinä
        if (!Hetutarkistus.OnkoHetuValidi(hetu))
        {
            await DisplayAlert("Virhe", "Henkilötunnuksen tarkistusmerkki ei täsmää tai muoto on virheellinen.", "OK");
            return;
        }

        // Postinumero: 5 numeroa
        if (postinumero.Length != 5 || !postinumero.All(char.IsDigit))
        {
            await DisplayAlert("Virhe", "Postinumeron tulee olla 5-numeroinen.", "OK");
            return;
        }

        // Päättymispäivän logiikka
        if (!toistaiseksiCheck.IsChecked && paattyminen < alkaminen)
        {
            await DisplayAlert("Virhe", "Päättymispäivä ei voi olla ennen alkamispäivää.", "OK");
            return;
        }

        // Tallennetaan henkilön muutokset poistetun tiedon tilalle, HETU perusteella tunnistetaan henkilö
        var kaikki = await HenkiloTiedostonHallinta.LataaHenkilotAsync();
        kaikki.RemoveAll(h => h.Henkilötunnus == muokattavaHenkilo.Henkilötunnus);


        var paivitetty = new Henkilo
        {
            Etunimet = etunimet,
            Sukunimi = sukunimi,
            Kutsumanimi = kutsumanimi,
            Henkilötunnus = hetu,
            Katuosoite = katuosoite,
            Postinumero = postinumero,
            Postitoimipaikka = postitoimipaikka,
            Alkamispäivä = alkaminen,
            Päättymispäivä = paattyminen,
            Nimike = nimike,
            Yksikkö = yksikko
        };

        kaikki.Add(paivitetty);
        await HenkiloTiedostonHallinta.TallennaHenkilotAsync(kaikki);
        await Lokitus.KirjaaAsync(App.KirjautunutKayttaja, "Muokkasi henkilöä", $"{etunimet} {sukunimi} ({hetu})");
        // Tallennetaan postipari tiedostoon
        await PostiTiedostonHallinta.LisaaTaiPaivitaPostiAsync(postinumero, postitoimipaikka);

        await DisplayAlert("Tallennettu", "Muutokset tallennettu!", "OK");

        // Siirrytään takaisin henkilölistaan
        await Shell.Current.GoToAsync("//HenkiloLista");
    }

    private async void PoistaKlikattu(object sender, EventArgs e)
    {
        if (muokattavaHenkilo == null)
            return;

        bool vahvistus = await DisplayAlert("Poista henkilö", $"Haluatko poistaa henkilön '{muokattavaHenkilo.Etunimet} {muokattavaHenkilo.Sukunimi}'?", "Kyllä", "Ei");
        if (vahvistus)
        {
            var kaikki = await HenkiloTiedostonHallinta.LataaHenkilotAsync();
            kaikki.RemoveAll(h => h.Etunimet == muokattavaHenkilo.Etunimet);
            await HenkiloTiedostonHallinta.TallennaHenkilotAsync(kaikki);

            await Lokitus.KirjaaAsync(App.KirjautunutKayttaja, "Poisti henkilön", $"{muokattavaHenkilo.Etunimet} {muokattavaHenkilo.Sukunimi} ({muokattavaHenkilo.Henkilötunnus})");

            await DisplayAlert("Poistettu", "Henkilö poistettu.", "OK");
            await Shell.Current.GoToAsync("//HenkiloLista");
        }
    }

    private async void PalaaEtusivulleKlikattu(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HenkiloLista");
    }
}
