using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hulki.Web.Migrations
{
    /// <summary>
    /// Tworzy 3 role bazodanowe (role serwerowe SQL Server + role bazy danych)
    /// z różnym poziomem uprawnień, do zademonstrowania mechanizmu
    /// GRANT / DENY / REVOKE na obronie z "systemów baz danych".
    ///
    /// Konta:
    ///   1. hulki_admin_login     → rola db_owner-podobna (CRUD na wszystkim)
    ///   2. hulki_therapist_login → SELECT/INSERT/UPDATE na danych klinicznych
    ///                              pacjentów (bez DELETE, bez tabel z hasłami)
    ///   3. hulki_readonly_login  → tylko SELECT, do raportów/statystyk
    ///
    /// UWAGA: to są role na poziomie SQL Server / bazy danych – niezależne
    /// od ról aplikacyjnych Identity (AspNetRoles: Admin/Therapist/Patient).
    /// Te dwa mechanizmy działają na różnych warstwach systemu.
    /// </summary>
    public partial class AddDatabaseSecurityRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ════════════════════════════════════════════════════════════════
            //  1. LOGINY SERWEROWE (poziom instancji SQL Server)
            // ════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_admin_login')
    CREATE LOGIN hulki_admin_login WITH PASSWORD = 'Adm1n_P@ssw0rd_2026!', CHECK_POLICY = ON;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_therapist_login')
    CREATE LOGIN hulki_therapist_login WITH PASSWORD = 'Ther@pist_P@ss_2026!', CHECK_POLICY = ON;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_readonly_login')
    CREATE LOGIN hulki_readonly_login WITH PASSWORD = 'Re@dOnly_P@ss_2026!', CHECK_POLICY = ON;
");

            // ════════════════════════════════════════════════════════════════
            //  2. UŻYTKOWNICY BAZY DANYCH (mapowani na loginy powyżej)
            // ════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_admin_user')
    CREATE USER hulki_admin_user FOR LOGIN hulki_admin_login;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_therapist_user')
    CREATE USER hulki_therapist_user FOR LOGIN hulki_therapist_login;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_readonly_user')
    CREATE USER hulki_readonly_user FOR LOGIN hulki_readonly_login;
");

            // ════════════════════════════════════════════════════════════════
            //  3. ROLE BAZODANOWE (definiują zestawy uprawnień)
            // ════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_admin')
    CREATE ROLE role_hulki_admin;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_therapist')
    CREATE ROLE role_hulki_therapist;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_readonly')
    CREATE ROLE role_hulki_readonly;
");

            // ── Przypisanie użytkowników do ról ─────────────────────────────
            migrationBuilder.Sql("ALTER ROLE role_hulki_admin     ADD MEMBER hulki_admin_user;");
            migrationBuilder.Sql("ALTER ROLE role_hulki_therapist ADD MEMBER hulki_therapist_user;");
            migrationBuilder.Sql("ALTER ROLE role_hulki_readonly  ADD MEMBER hulki_readonly_user;");

            // ════════════════════════════════════════════════════════════════
            //  4. GRANTY – role_hulki_admin: pełny dostęp (CRUD na wszystkim)
            // ════════════════════════════════════════════════════════════════
            // db_owner dałby zbyt dużo (łącznie z zarządzaniem samą bazą),
            // więc nadajemy jawnie SELECT/INSERT/UPDATE/DELETE na schemacie dbo.
            migrationBuilder.Sql(@"
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO role_hulki_admin;
GRANT EXECUTE ON SCHEMA::dbo TO role_hulki_admin;
");

            // ════════════════════════════════════════════════════════════════
            //  5. GRANTY – role_hulki_therapist
            //     SELECT/INSERT/UPDATE na danych klinicznych pacjentów,
            //     ale BEZ DELETE i BEZ dostępu do tabel z hasłami/kontami.
            // ════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
GRANT SELECT, INSERT, UPDATE ON dbo.Consultations      TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.DailyReports       TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.MoodLogs           TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.TherapyGoals       TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.GoalMilestones     TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.TherapyGroups      TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.PatientGroups      TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.GroupMessages      TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.GroupResources     TO role_hulki_therapist;
GRANT SELECT, INSERT, UPDATE ON dbo.Notifications      TO role_hulki_therapist;
GRANT SELECT                 ON dbo.AspNetUsers        TO role_hulki_therapist;

-- Może wywoływać procedury statystyczne i raportowe
GRANT EXECUTE ON dbo.sp_GetUserStats         TO role_hulki_therapist;
GRANT EXECUTE ON dbo.sp_AddPatientToGroup    TO role_hulki_therapist;

-- Jawna blokada – tabele z hasłami i danymi kontowymi pozostają niedostępne
DENY SELECT, INSERT, UPDATE, DELETE ON dbo.CustomUsers TO role_hulki_therapist;
DENY SELECT, INSERT, UPDATE, DELETE ON dbo.AspNetUserClaims TO role_hulki_therapist;
DENY SELECT, INSERT, UPDATE, DELETE ON dbo.AspNetUserLogins TO role_hulki_therapist;
DENY SELECT, INSERT, UPDATE, DELETE ON dbo.AspNetUserTokens TO role_hulki_therapist;
");

            // ════════════════════════════════════════════════════════════════
            //  6. GRANTY – role_hulki_readonly
            //     Wyłącznie SELECT, do raportów / statystyk / dashboardów.
            //     Brak dostępu do haseł i danych osobowych w surowej formie.
            // ════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
GRANT SELECT ON dbo.Consultations       TO role_hulki_readonly;
GRANT SELECT ON dbo.DailyReports        TO role_hulki_readonly;
GRANT SELECT ON dbo.MoodLogs            TO role_hulki_readonly;
GRANT SELECT ON dbo.TherapyGoals        TO role_hulki_readonly;
GRANT SELECT ON dbo.TherapyGroups       TO role_hulki_readonly;
GRANT SELECT ON dbo.PatientGroups       TO role_hulki_readonly;
GRANT SELECT ON dbo.Wallets             TO role_hulki_readonly;
GRANT SELECT ON dbo.PointTransactions   TO role_hulki_readonly;
GRANT SELECT ON dbo.ForumTopics         TO role_hulki_readonly;
GRANT SELECT ON dbo.ForumPosts          TO role_hulki_readonly;

-- Dostęp tylko do funkcji agregujących/statystycznych (nie do surowych haseł)
GRANT EXECUTE ON dbo.fn_AverageWalletBalance     TO role_hulki_readonly;
GRANT EXECUTE ON dbo.fn_CountUserConsultations   TO role_hulki_readonly;
GRANT EXECUTE ON dbo.fn_GetTotalEarnedPoints      TO role_hulki_readonly;
GRANT EXECUTE ON dbo.fn_CountUserBadges           TO role_hulki_readonly;

-- Jawna blokada – żadnych modyfikacji, żadnych danych kontowych
DENY INSERT, UPDATE, DELETE ON SCHEMA::dbo TO role_hulki_readonly;
DENY SELECT ON dbo.CustomUsers          TO role_hulki_readonly;
DENY SELECT ON dbo.AspNetUsers          TO role_hulki_readonly;
DENY SELECT ON dbo.AspNetUserClaims     TO role_hulki_readonly;
DENY SELECT ON dbo.AspNetUserLogins     TO role_hulki_readonly;
DENY SELECT ON dbo.AspNetUserTokens     TO role_hulki_readonly;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── Usuwanie w odwrotnej kolejności: granty -> role -> userzy -> loginy ──
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_admin')
    DROP ROLE role_hulki_admin;
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_therapist')
    DROP ROLE role_hulki_therapist;
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'role_hulki_readonly')
    DROP ROLE role_hulki_readonly;
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_admin_user')
    DROP USER hulki_admin_user;
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_therapist_user')
    DROP USER hulki_therapist_user;
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'hulki_readonly_user')
    DROP USER hulki_readonly_user;
");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_admin_login')
    DROP LOGIN hulki_admin_login;
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_therapist_login')
    DROP LOGIN hulki_therapist_login;
");
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'hulki_readonly_login')
    DROP LOGIN hulki_readonly_login;
");
        }
    }
}