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
    //Tarkistetaan onko k�ytt�j� kirjautunut sis��n ja ladataan postitunnukset tiedostosta
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!App.OnKirjautunut)
        {
            await DisplayAlert("Estetty", "Kirjaudu sis��n k�ytt��ksesi sovellusta.", "OK");
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
        muokattavaHenkilo = kaikki.FirstOrDefault(i => i.Henkil�tunnus == hetu);

        if (muokattavaHenkilo != null)
        {
            etunimetEntry.Text = muokattavaHenkilo.Etunimet;
            sukunimiEntry.Text = muokattavaHenkilo.Sukunimi;
            kutsumanimiEntry.Text = muokattavaHenkilo.Kutsumanimi;
            hetuEntry.Text = muokattavaHenkilo.Henkil�tunnus;

            katuosoiteEntry.Text = muokattavaHenkilo.Katuosoite;
            postinumeroEntry.Text = muokattavaHenkilo.Postinumero;
            postitoimipaikkaEntry.Text = muokattavaHenkilo.Postitoimipaikka;

            alkamispaivaPicker.Date = muokattavaHenkilo.Alkamisp�iv�;

            if (muokattavaHenkilo.P��ttymisp�iv�.HasValue)
            {
                paattumispaivaPicker.Date = muokattavaHenkilo.P��ttymisp�iv�.Value;
                toistaiseksiCheck.IsChecked = false;
                paattumispaivaPicker.IsVisible = true;
            }
            else
            {
                toistaiseksiCheck.IsChecked = true;
                paattumispaivaPicker.IsVisible = false;
            }

            nimikeEntry.Text = muokattavaHenkilo.Nimike;
            yksikkoPicker.SelectedItem = muokattavaHenkilo.Yksikk�;
        }
        else
        {
            await DisplayAlert("Virhe", "Henkil�n tietoja ei l�ytynyt.", "OK");
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
            await DisplayAlert("Virhe", "Muokattavaa henkil�� ei ole ladattu.", "OK");
            return;
        }

        // Luetaan kentist� tiedot
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

        //T�st� alkaa tarkistukset

        //sallitaan OnkoValidiNimell� my�s v�lily�nnit ja yhdismerkit tarkistuksessa
        bool OnkoValidiNimi(string nimi) =>
            nimi.All(c => char.IsLetter(c) || c == ' ' || c == '-');

        // Pakolliset kent�t
        if (string.IsNullOrWhiteSpace(etunimet) ||
            string.IsNullOrWhiteSpace(sukunimi) ||
            string.IsNullOrWhiteSpace(hetu) ||
            string.IsNullOrWhiteSpace(kutsumanimi) ||
            string.IsNullOrWhiteSpace(postitoimipaikka))
        {
            await DisplayAlert("Virhe", "Nimet, hetu ja postitoimipaikka ovat pakollisia.", "OK");
            return;
        }

        //Tarkistetaan, ett� nimien kohdalla on vain kirjaimia
        if (!OnkoValidiNimi(etunimet) || !OnkoValidiNimi(sukunimi) || !OnkoValidiNimi(kutsumanimi))
        {
            await DisplayAlert("Virhe", "Etunimi ja sukunimi voivat sis�lt�� vain kirjaimia, v�lily�ntej� tai yhdysmerkkej�.", "OK");
            return;
        }

        // K�ytet��n erillist� tiedostoa tarkistamiseen, jotta koodi pysyy siistin�
        if (!Hetutarkistus.OnkoHetuValidi(hetu))
        {
            await DisplayAlert("Virhe", "Henkil�tunnuksen tarkistusmerkki ei t�sm�� tai muoto on virheellinen.", "OK");
            return;
        }

        // Postinumero: 5 numeroa
        if (postinumero.Length != 5 || !postinumero.All(char.IsDigit))
        {
            await DisplayAlert("Virhe", "Postinumeron tulee olla 5-numeroinen.", "OK");
            return;
        }

        // P��ttymisp�iv�n logiikka
        if (!toistaiseksiCheck.IsChecked && paattyminen < alkaminen)
        {
            await DisplayAlert("Virhe", "P��ttymisp�iv� ei voi olla ennen alkamisp�iv��.", "OK");
            return;
        }

        // Tallennetaan henkil�n muutokset poistetun tiedon tilalle, HETU perusteella tunnistetaan henkil�
        var kaikki = await HenkiloTiedostonHallinta.LataaHenkilotAsync();
        kaikki.RemoveAll(h => h.Henkil�tunnus == muokattavaHenkilo.Henkil�tunnus);


        var paivitetty = new Henkilo
        {
            Etunimet = etunimet,
            Sukunimi = sukunimi,
            Kutsumanimi = kutsumanimi,
            Henkil�tunnus = hetu,
            Katuosoite = katuosoite,
            Postinumero = postinumero,
            Postitoimipaikka = postitoimipaikka,
            Alkamisp�iv� = alkaminen,
            P��ttymisp�iv� = paattyminen,
            Nimike = nimike,
            Yksikk� = yksikko
        };

        kaikki.Add(paivitetty);
        await HenkiloTiedostonHallinta.TallennaHenkilotAsync(kaikki);
        await Lokitus.KirjaaAsync(App.KirjautunutKayttaja, "Muokkasi henkil��", $"{etunimet} {sukunimi} ({hetu})");
        // Tallennetaan postipari tiedostoon
        await PostiTiedostonHallinta.LisaaTaiPaivitaPostiAsync(postinumero, postitoimipaikka);

        await DisplayAlert("Tallennettu", "Muutokset tallennettu!", "OK");

        // Siirryt��n takaisin henkil�listaan
        await Shell.Current.GoToAsync("//HenkiloLista");
    }

    private async void PoistaKlikattu(object sender, EventArgs e)
    {
        if (muokattavaHenkilo == null)
            return;

        bool vahvistus = await DisplayAlert("Poista henkil�", $"Haluatko poistaa henkil�n '{muokattavaHenkilo.Etunimet} {muokattavaHenkilo.Sukunimi}'?", "Kyll�", "Ei");
        if (vahvistus)
        {
            var kaikki = await HenkiloTiedostonHallinta.LataaHenkilotAsync();
            kaikki.RemoveAll(h => h.Etunimet == muokattavaHenkilo.Etunimet);
            await HenkiloTiedostonHallinta.TallennaHenkilotAsync(kaikki);

            await Lokitus.KirjaaAsync(App.KirjautunutKayttaja, "Poisti henkil�n", $"{muokattavaHenkilo.Etunimet} {muokattavaHenkilo.Sukunimi} ({muokattavaHenkilo.Henkil�tunnus})");

            await DisplayAlert("Poistettu", "Henkil� poistettu.", "OK");
            await Shell.Current.GoToAsync("//HenkiloLista");
        }
    }

    private async void PalaaEtusivulleKlikattu(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HenkiloLista");
    }
}
