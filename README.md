# Klinika Hulki – Aplikacja Webowa wspierająca walkę z uzależnieniami

Projekt zaliczeniowy z Programowania aplikacji WWW w technologii .NET realizowany przez zespół w składzie:
* Kacper S
* Kacper H
* Szymon N

## Opis Projektu
Aplikacja ma na celu wsparcie pacjentów w procesie wychodzenia z uzależnień poprzez grywalizację, system raportów dziennych, wsparcie społeczności (forum) oraz grupy terapeutyczne. Za codzienne meldunki pacjenci zdobywają punkty, które mogą wymieniać w wewnętrznym sklepie aplikacji.

## Architektura i Technologie
Projekt został zbudowany w oparciu o architekturę **BCE (Boundary-Control-Entity)** i wykorzystuje:
* **Backend:** .NET Core 8 / C#
* **Frontend:** ASP.NET Core MVC (Razor Pages, HTML, CSS, Bootstrap)
* **Baza danych:** Microsoft SQL Server (Entity Framework Core)
* **Autentykacja:** ASP.NET Core Identity

## Funkcjonalności systemu
1. **Moduł Pacjenta:** Składanie codziennych raportów, przeglądanie stanu portfela, historia transakcji punktowych.
2. **Grupy Terapeutyczne:** Czat grupowy, wspólne zadania (Questy), ankiety/quizy oraz współdzielone zasoby pomocowe.
3. **Sklep i Grywalizacja:** Kupowanie nagród (kart, zdrapek, ruletki) za punkty zdobyte z raportów.
4. **Forum:** Wymiana doświadczeń, kategorie, wątki oraz posty społeczności.
5. **Panel Administratora:** Zarządzanie kontami pacjentów, edycja przedmiotów w sklepie, weryfikacja przesłanych zgłoszeń i raportów.

## Integracje z zewnętrznymi API
System w klasie `QuoteService` wykorzystuje zewnętrzne serwisy przez dedykowane klienty HTTP:
* **ZenQuotes API** (`https://zenquotes.io/api/`) – pobieranie motywacyjnych cytatów dnia (keszowane w pamięci aplikacji).
* **MyMemory API** (`https://api.mymemory.translated.net/`) – automatyczne tłumaczenie pobranych cytatów z języka angielskiego na polski.

## Pierwsze uruchomienie
1. **Konfiguracja Bazy Danych:** Upewnij się, że masz poprawnie skonfigurowany `ConnectionString` w pliku `appsettings.json`.
2. **Automatyczne Migracje:** Przy starcie aplikacja samoczynnie uruchamia `context.Database.MigrateAsync()`, tworząc potrzebne tabele oraz dodając brakujące kolumny.
3. **Dane Startowe (Seeding):**
   * System automatycznie tworzy rolę `Admin` oraz konto administratora.
   * Dane logowania admina: **Email:** `admin@admin.com` | **Hasło:** `admin123`.
   * Sklep jest automatycznie zasilany początkowymi przedmiotami przez `StoreDataSeeder`.
