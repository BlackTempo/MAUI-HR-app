namespace final_work;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    // Tämä metodi suoritetaan, kun käyttäjä painaa "Kirjaudu" -nappia
    private async void KirjauduPainettu(object sender, EventArgs e)
    {
        string tunnus = kayttajatunnusEntry.Text;
        string salasana = salasanaEntry.Text;

        if (tunnus == "hruser" && salasana == $"{tunnus}")

        {
            virheIlmoitusLabel.IsVisible = false;
            App.OnKirjautunut = true; //Käyttäjä on kirjautunut sisään, ja saa pääsyn sovellukseen
            await Navigation.PushAsync(new Etusivu());

        }
        else
        {
            virheIlmoitusLabel.Text = "Virheellinen käyttäjätunnus tai salasana.";
            virheIlmoitusLabel.IsVisible = true;
        }
    }
}