# System rezerwacji biletów

Konsolowy program w języku C# do obsługi prostego systemu rezerwacji biletów na wydarzenia.

## Opis projektu

Program pozwala:

- dodawać wydarzenia (nazwa, data, liczba miejsc),
- rezerwować bilety na wydarzenie,
- anulować rezerwacje,
- wyświetlać listę wydarzeń z aktualnym stanem miejsc,
- wyszukiwać wydarzenie po nazwie,
- zapisywać listę wydarzeń do pliku tekstowego i wczytywać ją z pliku.

Program sprawdza poprawność danych: nie pozwala zarezerwować więcej miejsc niż jest wolnych,
anulować więcej miejsc niż zarezerwowano, podać pustej nazwy, ujemnej liczby, błędnej daty itp.
Każda operacja kończy się czytelnym komunikatem `[OK]` lub `[BŁĄD]`, a program nie kończy się błędem.

## Struktura projektu

```
Zadanie/
├── main.cs                    # kod programu (wszystkie klasy)
├── RezerwacjaBiletow.csproj   # plik projektu .NET (kompilacja przez dotnet)
├── README.md                  # dokumentacja
└── .gitignore
```

Cały kod znajduje się w jednym pliku `main.cs`, dzięki czemu można go wkleić bezpośrednio do
[OnlineGDB](https://www.onlinegdb.com/) (język C#).

## Instrukcja uruchomienia

### OnlineGDB

1. Wybierz język **C#**.
2. Wklej zawartość pliku `main.cs` do edytora.
3. Kliknij **Run**.
4. Aby uruchomić testy, wpisz `--testy` w polu *Command line arguments* i kliknij **Run**.

### .NET SDK (Windows / Linux / macOS)

```bash
dotnet run                # uruchomienie programu
dotnet run -- --testy     # uruchomienie testów automatycznych
```

### Mono

```bash
mcs main.cs -out:Rezerwacja.exe
mono Rezerwacja.exe
mono Rezerwacja.exe --testy
```

## Opis klas i metod

### `Wydarzenie`

Przechowuje informacje o pojedynczym wydarzeniu.

| Właściwość | Opis |
|---|---|
| `Nazwa` | nazwa wydarzenia |
| `Data` | data i godzina (`DateTime`) |
| `LiczbaMiejsc` | łączna liczba miejsc |
| `LiczbaZarezerwowanych` | liczba zarezerwowanych już miejsc |

| Metoda | Parametry | Zwraca | Opis |
|---|---|---|---|
| konstruktor | `nazwa`, `data`, `liczbaMiejsc` (opcjonalnie `liczbaZarezerwowanych`) | – | tworzy wydarzenie; przy błędnych danych zgłasza `ArgumentException` |
| `Zarezerwuj` | `int liczba` | `WynikOperacji` | rezerwuje miejsca, jeżeli są dostępne; inaczej komunikat „Brak wystarczającej liczby miejsc” |
| `Anuluj` | `int liczba` | `WynikOperacji` | zwalnia miejsca, jeżeli rezerwacje istnieją; inaczej komunikat „Nie można anulować więcej miejsc niż zarezerwowano” |
| `LiczbaWolnychMiejsc` | – | `int` | liczba wolnych miejsc |
| `ToString` | – | `string` | opis wydarzenia w postaci tekstowej |
| `DoLiniiPliku` | – | `string` | linia do zapisu w pliku (`nazwa;data;miejsca;zarezerwowane`) |
| `ZLiniiPliku` (statyczna) | `string linia` | `Wydarzenie` lub `null` | odtwarza wydarzenie z linii pliku; `null` gdy linia jest błędna |

### `SystemRezerwacji`

Zarządza kolekcją obiektów typu `Wydarzenie`.

| Metoda | Parametry | Zwraca | Opis |
|---|---|---|---|
| `DodajWydarzenie` | `string nazwa`, `DateTime data`, `int liczbaMiejsc` | `WynikOperacji` | dodaje wydarzenie (nazwy nie mogą się powtarzać) |
| `WyswietlWydarzenia` | – | `string` | lista wydarzeń posortowana po dacie |
| `ZnajdzWydarzenie` | `string nazwa` | `Wydarzenie` lub `null` | wyszukuje po nazwie (bez rozróżniania wielkości liter) |
| `Zarezerwuj` | `string nazwa`, `int liczba` | `WynikOperacji` | rezerwacja miejsc na wskazane wydarzenie |
| `Anuluj` | `string nazwa`, `int liczba` | `WynikOperacji` | anulowanie rezerwacji na wskazane wydarzenie |
| `ZapiszDoPliku` | `string sciezka` | `WynikOperacji` | zapisuje listę wydarzeń do pliku tekstowego |
| `WczytajZPliku` | `string sciezka` | `WynikOperacji` | wczytuje wydarzenia z pliku (zastępuje bieżącą listę, pomija błędne linie) |

### `WynikOperacji`

Informacja zwrotna z operacji: `Sukces` (`bool`) i `Komunikat` (`string`).
Tworzona metodami `WynikOperacji.Ok(...)` i `WynikOperacji.Blad(...)`.

### `MenuKonsoli`

Odpowiada za komunikację z użytkownikiem: wyświetla menu, wczytuje i sprawdza dane
(liczby, daty, nazwy) i wypisuje komunikaty. Nie zawiera logiki rezerwacji.

### `TestyAutomatyczne`

Sprawdza 8 scenariuszy z „Zakresu testów” i wypisuje wynik każdego z nich.

### `Program`

Punkt wejścia (`Main`). Uruchamia menu albo – z argumentem `--testy` – testy.

## Format pliku

Plik `wydarzenia.txt` (domyślna nazwa) zawiera jedno wydarzenie w linii:

```
Koncert Rockowy;12.12.2026 19:00;100;20
Spektakl;31.12.2026 20:00;50;0
```

Pola: nazwa; data (`dd.MM.yyyy HH:mm`); liczba miejsc; liczba zarezerwowanych miejsc.

## Przykładowe użycie

```
=== SYSTEM REZERWACJI BILETÓW ===

1. Dodaj wydarzenie
2. Zarezerwuj bilety
3. Anuluj rezerwację
4. Wyświetl listę wydarzeń
5. Wyszukaj wydarzenie po nazwie
6. Zapisz wydarzenia do pliku
7. Wczytaj wydarzenia z pliku
0. Wyjście
Wybierz opcję: 1
Nazwa wydarzenia: Koncert Rockowy
Data (dd.MM.yyyy HH:mm, np. 24.12.2026 18:00): 12.12.2026 19:00
Liczba miejsc: 100
[OK] Dodano wydarzenie: Koncert Rockowy | 12.12.2026 19:00 | miejsca: 100, zarezerwowane: 0, wolne: 100

Wybierz opcję: 2
Nazwa wydarzenia: Koncert Rockowy
Liczba miejsc do rezerwacji: 30
[OK] Zarezerwowano 30 miejsc(a) na "Koncert Rockowy". Pozostało wolnych: 70.

Wybierz opcję: 2
Nazwa wydarzenia: Koncert Rockowy
Liczba miejsc do rezerwacji: 80
[BŁĄD] Brak wystarczającej liczby miejsc. Wolnych miejsc: 70.

Wybierz opcję: 3
Nazwa wydarzenia: Koncert Rockowy
Liczba miejsc do anulowania: 50
[BŁĄD] Nie można anulować więcej miejsc niż zarezerwowano. Zarezerwowanych miejsc: 30.

Wybierz opcję: 3
Nazwa wydarzenia: Koncert Rockowy
Liczba miejsc do anulowania: 10
[OK] Anulowano 10 miejsc(a) na "Koncert Rockowy". Wolnych miejsc: 80.

Wybierz opcję: 4
1. Koncert Rockowy | 12.12.2026 19:00 | miejsca: 100, zarezerwowane: 20, wolne: 80

Wybierz opcję: 6
Nazwa pliku (Enter = wydarzenia.txt):
[OK] Zapisano wydarzenia do pliku "wydarzenia.txt" (liczba wydarzeń: 1).
```

(Menu wyświetla się przed każdym wyborem – w przykładzie pokazano je tylko raz.)

## Testy

Wynik `dotnet run -- --testy`:

```
=== TESTY ===
[ZALICZONY]    1. Dodanie wydarzenia
[ZALICZONY]    2. Rezerwacja miejsc
[ZALICZONY]    3. Nadmierna rezerwacja
[ZALICZONY]    4. Anulowanie rezerwacji
[ZALICZONY]    5. Anulowanie zbyt wielu miejsc
[ZALICZONY]    6. Zapis do pliku
[ZALICZONY]    7. Wczytanie z pliku
[ZALICZONY]    8. Wyświetlenie listy
Wynik: 8 zaliczonych, 0 niezaliczonych.
```

## Autor

- Imię i nazwisko: Franek Osinczuk
- Data wykonania zadania: 25.11.2026
