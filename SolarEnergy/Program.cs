using CsvHelper;
using SolarEnergy;
using System.Globalization;

string projectDirectory = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory));

// Combine the project directory with the file name
string filePath = Path.Combine(projectDirectory, "Gradovi_Proizvodnost.csv");

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
    Console.ReadLine();
    return;
}

using (var reader = new StreamReader(filePath))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<GradProizvodnost>();
    var gradovi = records.ToList();

    while (true)
    {
        for (var i = 0; i < gradovi.Count; i++)
        {
            Console.WriteLine($"{i + 1}: {gradovi[i].Grad} | {gradovi[i].PostanskiBroj} | {gradovi[i].Proizvodnost}");
        }
        Console.WriteLine();
        Console.WriteLine("Molimo odaberite redni broj pored grada za nastavak kalkulacije: ");
        var odabir = Console.ReadLine();
        if (!int.TryParse(odabir, out var index) || index < 1 || index > gradovi.Count)
        {
            Console.Clear();
            Console.WriteLine("Neispravan unos, molimo pokušajte ponovno.");
            continue;
        }

        var odabraniGrad = gradovi[index - 1];
        Console.WriteLine($"Vaš odabir je grad: {odabraniGrad.Grad} | {odabraniGrad.PostanskiBroj} | {odabraniGrad.Proizvodnost}");
        Console.WriteLine();
        Console.WriteLine("Ukoliko želite nastaviti, napišite DA. Ukoliko želite odabrati drugi grad, napišite NAZAD.");
        odabir = Console.ReadLine();

        if (odabir is not null && odabir.Trim().Equals("da", StringComparison.CurrentCultureIgnoreCase))
        {
            NastavakUpisa(odabraniGrad);
            break;
        }
        else
        {
            Console.Clear();
            Console.WriteLine("Povratak na odabir grada...");
        }
    }
    Console.ReadLine();
}

static void NastavakUpisa(GradProizvodnost odabraniGrad)
{
    Console.WriteLine();
    Console.WriteLine("Unesite snagu elektranu u kW [3-30kW] ");
    var snagaElektrane = double.Parse(Console.ReadLine()!);

    Console.WriteLine("Unesite orijentaciju (Sjever, Jug, Istok, Zapad): ");
    var orijentacija = Console.ReadLine();

    var ukupnaProizvodnja = snagaElektrane * odabraniGrad.Proizvodnost;
    Console.WriteLine($"Ukupna proizvodnja elektrane za {odabraniGrad.Grad} prema zadanim parametrima iznosi: {Math.Round(ukupnaProizvodnja, 2)} [kW]");

    Console.WriteLine("Koliko je korisnik ukupno energije potrošio?");
    var ukupnaPotrosnjaKorisnika = double.Parse(Console.ReadLine()!);

    var visakEnergije = ukupnaProizvodnja - ukupnaPotrosnjaKorisnika;
    Console.WriteLine($"Višak predane električne energije u mrežu iznosi: {Math.Round(visakEnergije, 2)}");

    var dozvoljenoOdstupanje = ukupnaPotrosnjaKorisnika * 0.05 * (-1);
    Console.WriteLine($"Odstupanje iznosi: {Math.Round(dozvoljenoOdstupanje, 2)}");

    double baznaCijena = 4000;
    double cijenaElektrane = baznaCijena;

    if (snagaElektrane == 3)
    {
        cijenaElektrane = baznaCijena;
    }
    else
    {
        for (int i = 4; i <= snagaElektrane; i++)
        {
            cijenaElektrane += cijenaElektrane * 0.30;
        }
    }

    Console.WriteLine();
    Console.WriteLine($"TOTALNA CIJENA ELEKTRANE IZNOSI: {Math.Round(cijenaElektrane, 2)} EUR");

    Console.WriteLine();
    Console.WriteLine("Unesite vasu email adresu: ");
    var emailAdresa = Console.ReadLine();

    if (visakEnergije < dozvoljenoOdstupanje)
    {
        Console.WriteLine("Elektrana ispravno dimenzionirana");
    }
    else
    {
        Console.WriteLine("Elektrana NIJE ispravno dimenzionirana");
    }

    var emailServis = new EmailServis();
    var izvjestaj = new Izvjestaj
    {
        DozvoljenoOdstupanje = dozvoljenoOdstupanje,
        IspravnoDimenzionirana = visakEnergije < dozvoljenoOdstupanje,
        SnagaElektrane = snagaElektrane,
        UkupnaPotrosnjaKorisnika = ukupnaPotrosnjaKorisnika,
        UkupnaProizvodnja = ukupnaProizvodnja,
        VisakEnergije = visakEnergije,
    };

    emailServis.PosaljiEmail(emailAdresa!, izvjestaj);

    Console.WriteLine($"Ponuda je poslana na email adresu: {emailAdresa}");
    Console.ReadLine();
}