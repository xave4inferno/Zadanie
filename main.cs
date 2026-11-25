/*
 * System rezerwacji biletów - program konsolowy.
 *
 * Plik zawiera wszystkie klasy programu, dzięki czemu można go wkleić
 * bezpośrednio do OnlineGDB (main.cs) albo skompilować lokalnie.
 *
 *   Wydarzenie          - pojedyncze wydarzenie i operacje na jego miejscach
 *   WynikOperacji       - informacja zwrotna (sukces / błąd + komunikat)
 *   SystemRezerwacji    - kolekcja wydarzeń, wyszukiwanie, zapis i odczyt z pliku
 *   MenuKonsoli         - komunikacja z użytkownikiem (menu tekstowe)
 *   TestyAutomatyczne   - sprawdzenie scenariuszy z "Zakresu testów"
 *   Program             - punkt wejścia (Main)
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RezerwacjaBiletow
{
    /// <summary>
    /// Wynik operacji: czy się powiodła oraz komunikat dla użytkownika.
    /// </summary>
    public class WynikOperacji
    {
        public bool Sukces { get; private set; }
        public string Komunikat { get; private set; }

        private WynikOperacji(bool sukces, string komunikat)
        {
            Sukces = sukces;
            Komunikat = komunikat;
        }

        public static WynikOperacji Ok(string komunikat)
        {
            return new WynikOperacji(true, komunikat);
        }

        public static WynikOperacji Blad(string komunikat)
        {
            return new WynikOperacji(false, komunikat);
        }
    }

    /// <summary>
    /// Pojedyncze wydarzenie: nazwa, data, liczba miejsc i liczba zarezerwowanych miejsc.
    /// </summary>
    public class Wydarzenie
    {
        public const string FormatDaty = "dd.MM.yyyy HH:mm";

        public string Nazwa { get; private set; }
        public DateTime Data { get; private set; }
        public int LiczbaMiejsc { get; private set; }
        public int LiczbaZarezerwowanych { get; private set; }

        public Wydarzenie(string nazwa, DateTime data, int liczbaMiejsc)
            : this(nazwa, data, liczbaMiejsc, 0)
        {
        }

        public Wydarzenie(string nazwa, DateTime data, int liczbaMiejsc, int liczbaZarezerwowanych)
        {
            if (string.IsNullOrWhiteSpace(nazwa))
                throw new ArgumentException("Nazwa wydarzenia nie może być pusta.");
            if (nazwa.Contains(SystemRezerwacji.Separator))
                throw new ArgumentException("Nazwa wydarzenia nie może zawierać znaku '" + SystemRezerwacji.Separator + "'.");
            if (liczbaMiejsc <= 0)
                throw new ArgumentException("Liczba miejsc musi być większa od zera.");
            if (liczbaZarezerwowanych < 0 || liczbaZarezerwowanych > liczbaMiejsc)
                throw new ArgumentException("Liczba zarezerwowanych miejsc musi mieścić się w zakresie 0-" + liczbaMiejsc + ".");

            Nazwa = nazwa.Trim();
            Data = data;
            LiczbaMiejsc = liczbaMiejsc;
            LiczbaZarezerwowanych = liczbaZarezerwowanych;
        }

        /// <summary>Rezerwuje podaną liczbę miejsc, jeżeli są dostępne.</summary>
        public WynikOperacji Zarezerwuj(int liczba)
        {
            if (liczba <= 0)
                return WynikOperacji.Blad("Liczba miejsc do rezerwacji musi być większa od zera.");
            if (liczba > LiczbaWolnychMiejsc())
                return WynikOperacji.Blad("Brak wystarczającej liczby miejsc. Wolnych miejsc: " + LiczbaWolnychMiejsc() + ".");

            LiczbaZarezerwowanych += liczba;
            return WynikOperacji.Ok("Zarezerwowano " + liczba + " miejsc(a) na \"" + Nazwa + "\". Pozostało wolnych: " + LiczbaWolnychMiejsc() + ".");
        }

        /// <summary>Zwalnia podaną liczbę miejsc, jeżeli takie rezerwacje istnieją.</summary>
        public WynikOperacji Anuluj(int liczba)
        {
            if (liczba <= 0)
                return WynikOperacji.Blad("Liczba miejsc do anulowania musi być większa od zera.");
            if (liczba > LiczbaZarezerwowanych)
                return WynikOperacji.Blad("Nie można anulować więcej miejsc niż zarezerwowano. Zarezerwowanych miejsc: " + LiczbaZarezerwowanych + ".");

            LiczbaZarezerwowanych -= liczba;
            return WynikOperacji.Ok("Anulowano " + liczba + " miejsc(a) na \"" + Nazwa + "\". Wolnych miejsc: " + LiczbaWolnychMiejsc() + ".");
        }

        /// <summary>Zwraca liczbę wolnych miejsc.</summary>
        public int LiczbaWolnychMiejsc()
        {
            return LiczbaMiejsc - LiczbaZarezerwowanych;
        }

        /// <summary>Zwraca informacje o wydarzeniu w postaci tekstowej.</summary>
        public override string ToString()
        {
            return string.Format("{0} | {1} | miejsca: {2}, zarezerwowane: {3}, wolne: {4}",
                Nazwa, Data.ToString(FormatDaty, CultureInfo.InvariantCulture),
                LiczbaMiejsc, LiczbaZarezerwowanych, LiczbaWolnychMiejsc());
        }

        /// <summary>Zamienia wydarzenie na jedną linię pliku tekstowego.</summary>
        public string DoLiniiPliku()
        {
            return string.Join(SystemRezerwacji.Separator.ToString(), new[]
            {
                Nazwa,
                Data.ToString(FormatDaty, CultureInfo.InvariantCulture),
                LiczbaMiejsc.ToString(CultureInfo.InvariantCulture),
                LiczbaZarezerwowanych.ToString(CultureInfo.InvariantCulture)
            });
        }

        /// <summary>Tworzy wydarzenie z linii pliku. Zwraca null, jeśli linia jest niepoprawna.</summary>
        public static Wydarzenie ZLiniiPliku(string linia)
        {
            string[] pola = linia.Split(SystemRezerwacji.Separator);
            if (pola.Length != 4)
                return null;

            DateTime data;
            int liczbaMiejsc;
            int zarezerwowane;
            if (!DateTime.TryParseExact(pola[1].Trim(), FormatDaty, CultureInfo.InvariantCulture, DateTimeStyles.None, out data)
                || !int.TryParse(pola[2].Trim(), out liczbaMiejsc)
                || !int.TryParse(pola[3].Trim(), out zarezerwowane))
                return null;

            try
            {
                return new Wydarzenie(pola[0], data, liczbaMiejsc, zarezerwowane);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Zarządza kolekcją obiektów typu Wydarzenie.
    /// </summary>
    public class SystemRezerwacji
    {
        public const char Separator = ';';

        private readonly List<Wydarzenie> wydarzenia = new List<Wydarzenie>();

        public IReadOnlyList<Wydarzenie> Wydarzenia
        {
            get { return wydarzenia.AsReadOnly(); }
        }

        /// <summary>Dodaje nowe wydarzenie (nazwy nie mogą się powtarzać).</summary>
        public WynikOperacji DodajWydarzenie(string nazwa, DateTime data, int liczbaMiejsc)
        {
            if (nazwa != null && ZnajdzWydarzenie(nazwa) != null)
                return WynikOperacji.Blad("Wydarzenie o nazwie \"" + nazwa.Trim() + "\" już istnieje.");

            try
            {
                Wydarzenie wydarzenie = new Wydarzenie(nazwa, data, liczbaMiejsc);
                wydarzenia.Add(wydarzenie);
                return WynikOperacji.Ok("Dodano wydarzenie: " + wydarzenie);
            }
            catch (ArgumentException ex)
            {
                return WynikOperacji.Blad(ex.Message);
            }
        }

        /// <summary>Zwraca listę wydarzeń w postaci tekstowej (posortowaną po dacie).</summary>
        public string WyswietlWydarzenia()
        {
            if (wydarzenia.Count == 0)
                return "Brak wydarzeń na liście.";

            StringBuilder tekst = new StringBuilder();
            int numer = 1;
            foreach (Wydarzenie wydarzenie in wydarzenia.OrderBy(w => w.Data))
            {
                tekst.AppendLine(numer + ". " + wydarzenie);
                numer++;
            }
            return tekst.ToString().TrimEnd();
        }

        /// <summary>Wyszukuje wydarzenie po nazwie (bez rozróżniania wielkości liter). Zwraca null, gdy brak.</summary>
        public Wydarzenie ZnajdzWydarzenie(string nazwa)
        {
            if (string.IsNullOrWhiteSpace(nazwa))
                return null;

            string szukana = nazwa.Trim();
            return wydarzenia.FirstOrDefault(w => string.Equals(w.Nazwa, szukana, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Rezerwuje miejsca na wydarzenie o podanej nazwie.</summary>
        public WynikOperacji Zarezerwuj(string nazwa, int liczba)
        {
            Wydarzenie wydarzenie = ZnajdzWydarzenie(nazwa);
            if (wydarzenie == null)
                return WynikOperacji.Blad("Nie znaleziono wydarzenia o nazwie \"" + nazwa + "\".");
            return wydarzenie.Zarezerwuj(liczba);
        }

        /// <summary>Anuluje rezerwację miejsc na wydarzenie o podanej nazwie.</summary>
        public WynikOperacji Anuluj(string nazwa, int liczba)
        {
            Wydarzenie wydarzenie = ZnajdzWydarzenie(nazwa);
            if (wydarzenie == null)
                return WynikOperacji.Blad("Nie znaleziono wydarzenia o nazwie \"" + nazwa + "\".");
            return wydarzenie.Anuluj(liczba);
        }

        /// <summary>Zapisuje listę wydarzeń do pliku tekstowego (jedno wydarzenie w linii).</summary>
        public WynikOperacji ZapiszDoPliku(string sciezka)
        {
            try
            {
                File.WriteAllLines(sciezka, wydarzenia.Select(w => w.DoLiniiPliku()), new UTF8Encoding(false));
                return WynikOperacji.Ok("Zapisano wydarzenia do pliku \"" + sciezka + "\" (liczba wydarzeń: " + wydarzenia.Count + ").");
            }
            catch (Exception ex)
            {
                return WynikOperacji.Blad("Nie udało się zapisać pliku \"" + sciezka + "\": " + ex.Message);
            }
        }

        /// <summary>
        /// Wczytuje wydarzenia z pliku tekstowego. Wczytane wydarzenia zastępują bieżącą listę.
        /// Niepoprawne linie są pomijane.
        /// </summary>
        public WynikOperacji WczytajZPliku(string sciezka)
        {
            if (!File.Exists(sciezka))
                return WynikOperacji.Blad("Plik \"" + sciezka + "\" nie istnieje.");

            string[] linie;
            try
            {
                linie = File.ReadAllLines(sciezka, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                return WynikOperacji.Blad("Nie udało się odczytać pliku \"" + sciezka + "\": " + ex.Message);
            }

            List<Wydarzenie> wczytane = new List<Wydarzenie>();
            int pominiete = 0;
            foreach (string linia in linie)
            {
                if (string.IsNullOrWhiteSpace(linia))
                    continue;

                Wydarzenie wydarzenie = Wydarzenie.ZLiniiPliku(linia);
                bool duplikat = wydarzenie != null && wczytane.Any(w =>
                    string.Equals(w.Nazwa, wydarzenie.Nazwa, StringComparison.OrdinalIgnoreCase));

                if (wydarzenie == null || duplikat)
                    pominiete++;
                else
                    wczytane.Add(wydarzenie);
            }

            wydarzenia.Clear();
            wydarzenia.AddRange(wczytane);

            string komunikat = "Wczytano wydarzenia z pliku \"" + sciezka + "\" (liczba wydarzeń: " + wczytane.Count + ").";
            if (pominiete > 0)
                komunikat += " Pominięto niepoprawnych linii: " + pominiete + ".";
            return WynikOperacji.Ok(komunikat);
        }
    }

    /// <summary>
    /// Menu tekstowe - odpowiada wyłącznie za komunikację z użytkownikiem.
    /// </summary>
    public class MenuKonsoli
    {
        private const string DomyslnyPlik = "wydarzenia.txt";

        private readonly SystemRezerwacji system;

        public MenuKonsoli(SystemRezerwacji system)
        {
            this.system = system;
        }

        public void Uruchom()
        {
            Console.WriteLine("=== SYSTEM REZERWACJI BILETÓW ===");

            while (true)
            {
                WyswietlMenu();
                string wybor = Console.ReadLine();
                if (wybor == null)          // koniec danych wejściowych
                    return;

                Console.WriteLine();
                switch (wybor.Trim())
                {
                    case "1": DodajWydarzenie(); break;
                    case "2": Rezerwuj(); break;
                    case "3": AnulujRezerwacje(); break;
                    case "4": Console.WriteLine(system.WyswietlWydarzenia()); break;
                    case "5": WyszukajWydarzenie(); break;
                    case "6": ZapiszDoPliku(); break;
                    case "7": WczytajZPliku(); break;
                    case "0":
                        Console.WriteLine("Do widzenia!");
                        return;
                    default:
                        Console.WriteLine("Nieznana opcja. Wybierz numer od 0 do 7.");
                        break;
                }
            }
        }

        private static void WyswietlMenu()
        {
            Console.WriteLine();
            Console.WriteLine("1. Dodaj wydarzenie");
            Console.WriteLine("2. Zarezerwuj bilety");
            Console.WriteLine("3. Anuluj rezerwację");
            Console.WriteLine("4. Wyświetl listę wydarzeń");
            Console.WriteLine("5. Wyszukaj wydarzenie po nazwie");
            Console.WriteLine("6. Zapisz wydarzenia do pliku");
            Console.WriteLine("7. Wczytaj wydarzenia z pliku");
            Console.WriteLine("0. Wyjście");
            Console.Write("Wybierz opcję: ");
        }

        private void DodajWydarzenie()
        {
            string nazwa = WczytajTekst("Nazwa wydarzenia: ");
            if (nazwa == null) return;

            DateTime? data = WczytajDate("Data (dd.MM.yyyy HH:mm, np. 24.12.2026 18:00): ");
            if (data == null) return;

            int? liczbaMiejsc = WczytajLiczbe("Liczba miejsc: ");
            if (liczbaMiejsc == null) return;

            Pokaz(system.DodajWydarzenie(nazwa, data.Value, liczbaMiejsc.Value));
        }

        private void Rezerwuj()
        {
            string nazwa = WczytajTekst("Nazwa wydarzenia: ");
            if (nazwa == null) return;

            int? liczba = WczytajLiczbe("Liczba miejsc do rezerwacji: ");
            if (liczba == null) return;

            Pokaz(system.Zarezerwuj(nazwa, liczba.Value));
        }

        private void AnulujRezerwacje()
        {
            string nazwa = WczytajTekst("Nazwa wydarzenia: ");
            if (nazwa == null) return;

            int? liczba = WczytajLiczbe("Liczba miejsc do anulowania: ");
            if (liczba == null) return;

            Pokaz(system.Anuluj(nazwa, liczba.Value));
        }

        private void WyszukajWydarzenie()
        {
            string nazwa = WczytajTekst("Nazwa wydarzenia: ");
            if (nazwa == null) return;

            Wydarzenie wydarzenie = system.ZnajdzWydarzenie(nazwa);
            Console.WriteLine(wydarzenie != null
                ? "Znaleziono: " + wydarzenie
                : "Nie znaleziono wydarzenia o nazwie \"" + nazwa + "\".");
        }

        private void ZapiszDoPliku()
        {
            Pokaz(system.ZapiszDoPliku(WczytajSciezke()));
        }

        private void WczytajZPliku()
        {
            Pokaz(system.WczytajZPliku(WczytajSciezke()));
        }

        // ---------- metody pomocnicze do wczytywania danych ----------

        private static void Pokaz(WynikOperacji wynik)
        {
            Console.WriteLine((wynik.Sukces ? "[OK] " : "[BŁĄD] ") + wynik.Komunikat);
        }

        private static string WczytajSciezke()
        {
            Console.Write("Nazwa pliku (Enter = " + DomyslnyPlik + "): ");
            string sciezka = Console.ReadLine();
            return string.IsNullOrWhiteSpace(sciezka) ? DomyslnyPlik : sciezka.Trim();
        }

        private static string WczytajTekst(string pytanie)
        {
            Console.Write(pytanie);
            string tekst = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(tekst))
            {
                Console.WriteLine("[BŁĄD] Wartość nie może być pusta.");
                return null;
            }
            return tekst.Trim();
        }

        private static int? WczytajLiczbe(string pytanie)
        {
            Console.Write(pytanie);
            string tekst = Console.ReadLine();
            int liczba;
            if (!int.TryParse(tekst, out liczba) || liczba <= 0)
            {
                Console.WriteLine("[BŁĄD] Podaj liczbę całkowitą większą od zera.");
                return null;
            }
            return liczba;
        }

        private static DateTime? WczytajDate(string pytanie)
        {
            Console.Write(pytanie);
            string tekst = Console.ReadLine();
            DateTime data;
            if (tekst == null || !DateTime.TryParseExact(tekst.Trim(), Wydarzenie.FormatDaty,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out data))
            {
                Console.WriteLine("[BŁĄD] Niepoprawna data. Użyj formatu dd.MM.yyyy HH:mm.");
                return null;
            }
            return data;
        }
    }

    /// <summary>
    /// Proste testy scenariuszy z treści zadania. Uruchomienie: argument "--testy".
    /// </summary>
    public static class TestyAutomatyczne
    {
        private static int zaliczone;
        private static int niezaliczone;

        public static int Uruchom()
        {
            zaliczone = 0;
            niezaliczone = 0;
            string plik = Path.Combine(Path.GetTempPath(), "test_wydarzenia.txt");
            DateTime data = new DateTime(2026, 12, 24, 18, 0, 0);

            Console.WriteLine("=== TESTY ===");

            SystemRezerwacji system = new SystemRezerwacji();
            system.DodajWydarzenie("Koncert", data, 100);
            Sprawdz("1. Dodanie wydarzenia", system.ZnajdzWydarzenie("Koncert") != null && system.Wydarzenia.Count == 1);

            system.Zarezerwuj("Koncert", 30);
            Sprawdz("2. Rezerwacja miejsc", system.ZnajdzWydarzenie("Koncert").LiczbaZarezerwowanych == 30);

            WynikOperacji nadmierna = system.Zarezerwuj("Koncert", 71);
            Sprawdz("3. Nadmierna rezerwacja", !nadmierna.Sukces
                && nadmierna.Komunikat.StartsWith("Brak wystarczającej liczby miejsc")
                && system.ZnajdzWydarzenie("Koncert").LiczbaZarezerwowanych == 30);

            system.Anuluj("Koncert", 10);
            Sprawdz("4. Anulowanie rezerwacji", system.ZnajdzWydarzenie("Koncert").LiczbaZarezerwowanych == 20);

            WynikOperacji zaDuzo = system.Anuluj("Koncert", 21);
            Sprawdz("5. Anulowanie zbyt wielu miejsc", !zaDuzo.Sukces
                && zaDuzo.Komunikat.StartsWith("Nie można anulować więcej miejsc niż zarezerwowano")
                && system.ZnajdzWydarzenie("Koncert").LiczbaZarezerwowanych == 20);

            system.DodajWydarzenie("Spektakl", data.AddDays(7), 50);
            system.ZapiszDoPliku(plik);
            Sprawdz("6. Zapis do pliku", File.Exists(plik)
                && File.ReadAllLines(plik).Contains("Koncert;24.12.2026 18:00;100;20"));

            SystemRezerwacji wczytany = new SystemRezerwacji();
            wczytany.WczytajZPliku(plik);
            Wydarzenie koncert = wczytany.ZnajdzWydarzenie("Koncert");
            Sprawdz("7. Wczytanie z pliku", wczytany.Wydarzenia.Count == 2
                && koncert != null && koncert.LiczbaZarezerwowanych == 20 && koncert.Data == data);

            string lista = wczytany.WyswietlWydarzenia();
            Sprawdz("8. Wyświetlenie listy", lista.Contains("Koncert") && lista.Contains("wolne: 80")
                && lista.Contains("Spektakl") && lista.Contains("wolne: 50"));

            File.Delete(plik);
            Console.WriteLine("Wynik: " + zaliczone + " zaliczonych, " + niezaliczone + " niezaliczonych.");
            return niezaliczone == 0 ? 0 : 1;
        }

        private static void Sprawdz(string nazwaTestu, bool warunek)
        {
            if (warunek) zaliczone++; else niezaliczone++;
            Console.WriteLine((warunek ? "[ZALICZONY]    " : "[NIEZALICZONY] ") + nazwaTestu);
        }
    }

    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Console.OutputEncoding = new UTF8Encoding(false);
            }
            catch (Exception)
            {
                // niektóre konsole nie pozwalają zmienić kodowania - nie jest to błąd krytyczny
            }

            try
            {
                if (args.Length > 0 && args[0] == "--testy")
                    return TestyAutomatyczne.Uruchom();

                new MenuKonsoli(new SystemRezerwacji()).Uruchom();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wystąpił nieoczekiwany błąd: " + ex.Message);
                return 1;
            }
        }
    }
}
