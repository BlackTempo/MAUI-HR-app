namespace final_work;

public partial class Etusivu : ContentPage
{
    public Etusivu()
    {
        InitializeComponent();
    }
    //Sivu, johon tulla kirjautumisen j‰lkeen ja milt‰ p‰‰st‰‰n k‰siksi kaikkiin sovelluksen toimintoihin
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!App.OnKirjautunut)
        {
            await DisplayAlert("Estetty", "Kirjaudu sis‰‰n k‰ytt‰‰ksesi sovellusta.", "OK");
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }
    }
    //Sovelluksen sulkeminen tarvittaessa napista Windows-laitteilla
    private void SuljeSovellusKlikattu(object sender, EventArgs e)
    {
        Application.Current.Quit();
    }

    private async void NaytaHenkiloLista(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//HenkiloLista");
    }

    private async void LisaaHenkiloKlikattu(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//LisaaHenkilo");
    }
}