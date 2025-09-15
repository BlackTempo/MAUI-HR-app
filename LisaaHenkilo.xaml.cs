using final_work.Models;
using final_work.Tiedostot;
using Microsoft.WindowsAppSDK.Runtime.Packages;

namespace final_work;


public partial class LisaaHenkilo : ContentPage
{
    public LisaaHenkilo()
    {
        InitializeComponent();
        paattumispaivaPicker.IsVisible = true;
    }
    //Tarkistetaan onko k�ytt�j� kirjautunut sis��n ja ladataan postitunnukset tiedostosta sek� tyhjennet��n tiedot mahdollisesti kesken j��neen lis�yksen j�ljilt�
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

            // Tyhjennet��n kent�t
            etunimetEntry.Text = "";
            sukunimiEntry.Text = "";
            kutsumanimiEntry.Text = "";
            hetuEntry.Text = "";
            katuosoiteEntry.Text = "";
            postinumeroEntry.Text = "";
            postitoimipaikkaEntry.Text = "";
            alkamispaivaPicker.Date = DateTime.Today;
            paattumispaivaPicker.Date = DateTime.Today;
            toistaiseksiCheck.IsChecked = false;
            nimikeEntry.Text = "";
            yksikkoPicker.SelectedIndex = -1;
    }


    private async void TallennaKlikattu(object sender, EventArgs e)
    {
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



        // Tarkistukset alkaa t�st�

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

        // Ladataan vanhat henkil�t
        var henkilot = await HenkiloTiedostonHallinta.LataaHenkilotAsync();

        // Tarkistetaan onko sama HETU jo olemassa
        if (henkilot.Any(h => h.Henkil�tunnus == hetu))
        {
            await DisplayAlert("Virhe", "Sama henkil�tunnus on jo olemassa. Et voi tallentaa kahta samaa HETU:a.", "OK");
            return;
        }

        // Luodaan uusi Henkilo
        var uusi = new Henkilo
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

        // Tallennetaan henkil�
        henkilot.Add(uusi);
        await HenkiloTiedostonHallinta.TallennaHenkilotAsync(henkilot);

        // Tallennetaan postipari tiedostoon
        await PostiTiedostonHallinta.LisaaTaiPaivitaPostiAsync(postinumero, postitoimipaikka);

        //Lokitetaan tieto lis��misest�
        await Lokitus.KirjaaAsync(App.KirjautunutKayttaja, "Lis�si henkil�n", $"{etunimet} {sukunimi} ({hetu})");


        // Popup: lis�t��nk� uusi vai palataanko?
        bool lisaaUusi = await DisplayAlert(
            "Tallennettu",
            $"Henkil� {etunimet} {sukunimi} tallennettu.\n\nHaluatko lis�t� toisen henkil�n?",
            "Kyll�",
            "Palaa takaisin henkil�listaukseen");

        if (lisaaUusi)
        {
            // Tyhjennet��n kent�t uutta henkil�� varten
            etunimetEntry.Text = "";
            sukunimiEntry.Text = "";
            kutsumanimiEntry.Text = "";
            hetuEntry.Text = "";
            katuosoiteEntry.Text = "";
            postinumeroEntry.Text = "";
            postitoimipaikkaEntry.Text = "";
            alkamispaivaPicker.Date = DateTime.Today;
            paattumispaivaPicker.Date = DateTime.Today;
            toistaiseksiCheck.IsChecked = false;
            nimikeEntry.Text = "";
            yksikkoPicker.SelectedIndex = -1;
        }
        else
        {
            await Shell.Current.GoToAsync("//HenkiloLista");
        }
    }

    //Lista, johon tallennetaan kaikki postitiedot, jotta voidaan ehdottaa k�ytt�j�lle postinumeroita ja postitoimipaikkoja vaikka poistettaisiin kaikki k�ytt�j�t
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
    private void ToistaiseksiCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        bool piilotetaan = e.Value;

        paattumisLabel.IsVisible = !piilotetaan;
        paattumispaivaPicker.IsVisible = !piilotetaan;
    }

    private async void PalaaTakaisinKlikattu(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HenkiloLista");
    }
}