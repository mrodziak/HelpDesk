# HelpDesk – System zgłoszeń IT

Projekt zaliczeniowy (poprawkowy) z przedmiotu Programowanie zaawansowane.

Aplikacja webowa w architekturze klient–serwer wykonana w technologii:

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- SQL Server (LocalDB)
- ASP.NET Identity (system użytkowników i ról)

## Opis projektu

HelpDesk to system do obsługi zgłoszeń IT w firmie.
Użytkownicy mogą tworzyć zgłoszenia problemów technicznych, które następnie są obsługiwane przez dział Support oraz Administratorów.
Projekt implementuje pełną logikę biznesową, system ról oraz relacje między modelami danych.

## Architektura

Projekt został wykonany w architekturze MVC:

- **Models** – modele danych (Ticket, Category, Priority, TicketComment, Notification)
- **Views** – warstwa prezentacji (Razor)
- **Controllers** – logika aplikacji
- **Entity Framework Core** – ORM
- **SQL Server** – baza danych

Aplikacja wykorzystuje podejście Code First z migracjami.

## Modele i relacje

W projekcie wykorzystano wiele modeli połączonych relacjami:

- Ticket
- Category
- Priority
- TicketComment
- Notification
- IdentityUser

Relacje:
- Ticket → Category (wiele do jednego)
- Ticket → Priority (wiele do jednego)
- Ticket → User (właściciel zgłoszenia)
- Ticket → Assigned Support (opcjonalne przypisanie)
- Ticket → Comments (jeden do wielu)
- Ticket → Notifications (logika powiązana)

## Role w systemie

### Admin
- Zarządzanie kategoriami i priorytetami
- Zmiana statusów i priorytetów zgłoszeń
- Przypisywanie zgłoszeń do Support
- Podgląd wszystkich zgłoszeń

### Support
- Podgląd wszystkich zgłoszeń
- Przejmowanie nieprzypisanych zgłoszeń
- Zmiana statusu i priorytetu przypisanych zgłoszeń
- Komentowanie zgłoszeń

### User
- Tworzenie zgłoszeń
- Podgląd własnych zgłoszeń
- Komentowanie własnych zgłoszeń

## Powiadomienia

System generuje powiadomienia w sytuacjach:

- Dodanie komentarza
- Zmiana statusu
- Zmiana priorytetu
- Przypisanie zgłoszenia do Support

Powiadomienia trafiają do:
- Właściciela zgłoszenia
- Przypisanego pracownika Support
- Administratora

## Funkcjonalności

- Rejestracja i logowanie użytkowników
- System ról (Admin / Support / User)
- Tworzenie zgłoszeń
- Automatyczne ustawianie statusu i priorytetu
- Komentarze do zgłoszeń
- Przypisywanie zgłoszeń
- Filtrowanie zgłoszeń (wszystkie / moje przypisane / nieprzypisane)
- Powiadomienia
- Walidacja danych
- Autoryzacja dostępu do danych

## Baza danych

Projekt wykorzystuje SQL Server LocalDB.
Connection string znajduje się w pliku: appsettings.json

```json
 "ConnectionStrings": {
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HelpDesk;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

## Uruchomienie projektu

1. Otwórz projekt w Visual Studio 2022+
2. Otwórz:
   Tools → NuGet Package Manager → Package Manager Console
3. Wpisz:
   Update-Database
4. Uruchom projekt (HTTPS)

Baza danych zostanie utworzona automatycznie z migracji.

