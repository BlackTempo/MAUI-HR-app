using System.Text;

namespace final_work.Tiedostot
{
    public static class Lokitus //Luodaan lokitusluokka, joka tallentaa tapahtumatiedot selkokielisenä tiedostoon
    {
        private static readonly string lokiPolku = Path.Combine(FileSystem.AppDataDirectory, "tapahtumaloki.txt");

        public static async Task KirjaaAsync(string kayttaja, string toiminto, string lisatieto = "")
        {
            string rivi = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {kayttaja} | {toiminto} | {lisatieto}";
            try
            {
                await File.AppendAllTextAsync(lokiPolku, rivi + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Lokitus epäonnistui – vaihtoehtoisesti käsittele
                System.Diagnostics.Debug.WriteLine($"Lokitusvirhe: {ex.Message}");
            }
        }

        public static string LokiPolku => lokiPolku;
    }
}
