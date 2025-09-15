using final_work.Tiedostot;

namespace final_work;

public partial class SalausTesti : ContentPage //K‰ytt‰‰ksesi t‰t‰ testi‰, muuta Appshell.xaml Salaustesti osalta (ota kommentti pois)
{
    public SalausTesti()
    {
        InitializeComponent();
    }
    //Testataan miten merkkijono muuttuu salatessa ja palautuuko se samaksi purkuvaiheessa
    private void SalausKlikattu(object sender, EventArgs e)
    {
        string syote = syoteEditor.Text ?? "";

        string salattu = Salaus.Salaa(syote);
        string purettu = Salaus.Pura(salattu);

        salattuLabel.Text = salattu;
        purettuLabel.Text = purettu;
    }
}