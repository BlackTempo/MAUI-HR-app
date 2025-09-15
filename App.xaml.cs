namespace final_work;

public partial class App : Application
{

    public static bool OnKirjautunut { get; set; } = false;
    public static string KirjautunutKayttaja { get; set; } = "hruser";

    public App()
	{
		InitializeComponent();
		MainPage = new AppShell();
    }
}
