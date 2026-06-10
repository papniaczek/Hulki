# Klinika Hulki – Aplikacja Webowa wspierająca walkę z uzależnieniami

Projekt zaliczeniowy z Programowania aplikacji WWW w technologii .NET realizowany przez zespół w składzie:
* Kacper S
* Kacper H
* Szymon N

## Opis Projektu
Aplikacja ma na celu wsparcie pacjentów w procesie wychodzenia z uzależnień poprzez grywalizację, system raportów dziennych, wsparcie społeczności (forum) oraz grupy terapeutyczne. Za codzienne meldunki pacjenci zdobywają punkty, które mogą wymieniać w wewnętrznym sklepie aplikacji.

## Architektura i Technologie
Projekt został zbudowany w oparciu o architekturę **BCE (Boundary-Control-Entity)** i wykorzystuje:
* **Backend:** .NET 10 / C#
* **Frontend:** ASP.NET Core MVC (Razor Views, HTML, CSS, Bootstrap)
* **Baza danych:** Microsoft SQL Server (Entity Framework Core)
* **Autentykacja:** ASP.NET Core Identity
* **Generowanie PDF:** QuestPDF

## Funkcjonalności systemu
1. **Moduł Pacjenta:** Składanie codziennych raportów, przeglądanie stanu portfela, historia transakcji punktowych.
2. **Grupy Terapeutyczne:** Czat grupowy, wspólne zadania (Questy), ankiety/quizy oraz współdzielone zasoby pomocowe.
3. **Sklep i Grywalizacja:** Kupowanie nagród (kart, zdrapek, ruletki) za punkty zdobyte z raportów.
4. **Forum:** Wymiana doświadczeń, kategorie, wątki oraz posty społeczności.
5. **Panel Administratora:** Zarządzanie kontami pacjentów, edycja przedmiotów w sklepie, weryfikacja przesłanych zgłoszeń i raportów, nadawanie roli Terapeuty.
6. **Konsultacje:** Umawianie wizyt między pacjentem a terapeutą, podgląd szczegółów wizyty i notatek.
7. **Cele Terapeutyczne:** Dodawanie celów z kamieniami milowymi, śledzenie postępów.
8. **Odznaki i Osiągnięcia:** Automatyczne przyznawanie odznak za aktywność (np. ukończone cele, złożone raporty).
9. **Nastrój:** Codzienne logowanie nastroju przez pacjenta.
10. **Ankiety:** Tworzenie i wypełnianie ankiet w ramach grup terapeutycznych.
11. **Powiadomienia:** System powiadomień wewnętrznych dla użytkowników.

## Integracje z zewnętrznymi API
System w klasie `QuoteService` wykorzystuje zewnętrzne serwisy przez dedykowane klienty HTTP:
* **ZenQuotes API** (`https://zenquotes.io/api/`) – pobieranie motywacyjnych cytatów dnia (keszowane w pamięci aplikacji).
* **MyMemory API** (`https://api.mymemory.translated.net/`) – automatyczne tłumaczenie pobranych cytatów z języka angielskiego na polski.

## Własne API
Aplikacja udostępnia publiczne endpointy REST pod prefiksem `/api/`:
* **GET `/api/quotes/random`** – zwraca losowy cytat motywacyjny (używany przez przycisk „Nowy cytat" na stronie głównej).
* **GET `/api/shop/items`** – zwraca aktualną ofertę przedmiotów sklepu w formacie JSON.

## Obsługa załączników
* Pacjent może dołączyć plik (obraz lub PDF) do każdego raportu dziennego.
* Terapeuta może wgrywać zasoby pomocowe (pliki) do grup terapeutycznych.
* Pliki są przechowywane w folderze `wwwroot/uploads/` i dostępne do pobrania.

## Generowanie dokumentów PDF
Aplikacja generuje raporty PDF przy użyciu biblioteki QuestPDF:
* **Raport raportów dziennych** – lista wpisów pacjenta z opcjonalnym filtrowaniem po datach.
* **Raport postępów** – podsumowanie celów terapeutycznych, zdobytych odznak, punktów i konsultacji.
* **Raport konsultacji** – szczegóły pojedynczej wizyty (dane pacjenta, terapeuta, notatki, zalecenia).

Pliki PDF można pobrać bezpośrednio z poziomu panelu pacjenta.

## Baza danych – Encje

Aplikacja posiada łącznie **35 encji** (+ tabele Identity): **8 słownikowych** i **27 niesłownikowych**.

### Encje słownikowe (8)
Encje pomocnicze zawierające stałe wartości/typy używane przez resztę systemu.

| Encja | Opis |
|---|---|
| `TherapyType` | Rodzaj terapii (np. indywidualna, grupowa) |
| `ReportStatus` | Status raportu dziennego (np. oczekujący, zatwierdzony) |
| `ItemRarity` | Rzadkość przedmiotu w sklepie (np. pospolity, rzadki) |
| `GameType` | Typ gry dostępnej w sklepie (np. karta, zdrapka, ruletka) |
| `ForumCategory` | Kategoria wątku na forum |
| `FileType` | Typ pliku załącznika |
| `MoodType` | Typ nastroju (np. dobry, neutralny, zły) |
| `ConsultationStatus` | Status konsultacji (np. zaplanowana, zakończona) |

### Encje niesłownikowe (27)
Encje przechowujące właściwe dane aplikacji.

| Encja | Opis |
|---|---|
| `TherapyGroup` | Grupa terapeutyczna |
| `PatientGroup` | Powiązanie pacjenta z grupą (tabela łącząca) |
| `DailyReport` | Raport dzienny składany przez pacjenta |
| `ReportAttachment` | Załącznik (plik) dołączony do raportu |
| `Wallet` | Portfel punktowy pacjenta |
| `PointTransaction` | Historia transakcji punktowych |
| `RewardItem` | Przedmiot do kupienia w sklepie |
| `PatientInventory` | Ekwipunek pacjenta (zakupione nagrody) |
| `Game` | Gra dostępna w sklepie |
| `GameSession` | Sesja rozgrywki pacjenta |
| `ForumTopic` | Wątek na forum |
| `ForumPost` | Post/odpowiedź w wątku forum |
| `GroupMessage` | Wiadomość na czacie grupowym |
| `GroupQuest` | Zadanie (quest) przypisane do grupy |
| `QuestSubmission` | Odpowiedź pacjenta na zadanie grupowe |
| `GroupResource` | Zasób/plik wgrany do grupy przez terapeutę |
| `Consultation` | Konsultacja między pacjentem a terapeutą |
| `VisitDetails` | Szczegóły wizyty (diagnoza, zalecenia, notatki) |
| `Survey` | Ankieta tworzona w grupie |
| `SurveyQuestion` | Pytanie w ankiecie |
| `SurveyAnswer` | Odpowiedź pacjenta na pytanie ankiety |
| `SurveySubmission` | Zgłoszenie wypełnienia ankiety |
| `MoodLog` | Dzienny wpis nastroju pacjenta |
| `Notification` | Powiadomienie wewnętrzne dla użytkownika |
| `AchievementBadge` | Definicja odznaki do zdobycia |
| `UserBadge` | Odznaka zdobyta przez pacjenta |
| `TherapyGoal` | Cel terapeutyczny pacjenta |
| `GoalMilestone` | Kamień milowy w ramach celu terapeutycznego |

## Pierwsze uruchomienie
1. **Konfiguracja Bazy Danych:** Upewnij się, że masz poprawnie skonfigurowany `ConnectionString` w pliku `appsettings.json`.
2. **Automatyczne Migracje:** Przy starcie aplikacja samoczynnie uruchamia `context.Database.MigrateAsync()`, tworząc potrzebne tabele oraz dodając brakujące kolumny.
3. **Dane Startowe (Seeding):**
   * System automatycznie tworzy rolę `Admin` oraz konto administratora.
   * Dane logowania admina: **Email:** `admin@admin.com` | **Hasło:** `admin123`.
   * Sklep jest automatycznie zasilany początkowymi przedmiotami przez `StoreDataSeeder`.
