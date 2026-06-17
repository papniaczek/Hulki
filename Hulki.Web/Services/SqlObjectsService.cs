using System;
using System.Data;
using System.Threading.Tasks;
using Hulki.Web.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hulki.Web.Services;

// ── DTOs ──────────────────────────────────────────────────────────────────────

public record UserStatsDto(
    string FullName,
    int    CompletedConsultations,
    int    MoodLogCount,
    int    DailyReportCount,
    int    CompletedGoals,
    int    WalletBalance
);

public record GroupOperationResult(bool Success, string Message);

// ── Serwis ────────────────────────────────────────────────────────────────────

/// <summary>
/// Wywołuje triggery, funkcje i procedury składowane bezpośrednio z C#
/// przez ADO.NET / EF Core. Wstrzyknij przez DI jako Scoped.
/// </summary>
public class SqlObjectsService
{
    private readonly ApplicationDbContext _db;

    public SqlObjectsService(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── PROCEDURY ────────────────────────────────────────────────────────

    /// <summary>Wywołuje sp_GetUserStats i zwraca statystyki użytkownika.</summary>
    public async Task<UserStatsDto?> GetUserStatsAsync(string userId)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC dbo.sp_GetUserStats @userId";
        cmd.CommandType = CommandType.Text;

        var p = cmd.CreateParameter();
        p.ParameterName = "@userId";
        p.Value         = userId;
        cmd.Parameters.Add(p);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new UserStatsDto(
            FullName:                reader.GetString(0),
            CompletedConsultations:  reader.GetInt32(1),
            MoodLogCount:            reader.GetInt32(2),
            DailyReportCount:        reader.GetInt32(3),
            CompletedGoals:          reader.GetInt32(4),
            WalletBalance:           reader.GetInt32(5)
        );
    }

    /// <summary>Wywołuje sp_AddPatientToGroup i zwraca wynik operacji.</summary>
    public async Task<GroupOperationResult> AddPatientToGroupAsync(
        string userId, int groupId, bool isApproved = false)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC dbo.sp_AddPatientToGroup @userId, @groupId, @isApproved";

        AddParam(cmd, "@userId",     userId);
        AddParam(cmd, "@groupId",    groupId);
        AddParam(cmd, "@isApproved", isApproved ? 1 : 0);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return new GroupOperationResult(false, "Brak odpowiedzi z procedury.");

        return new GroupOperationResult(
            reader.GetInt32(0) == 1,
            reader.GetString(1)
        );
    }

    /// <summary>Wywołuje sp_PurgeOldNotifications i zwraca liczbę usuniętych rekordów.</summary>
    public async Task<int> PurgeOldNotificationsAsync(int olderThanDays = 90)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC dbo.sp_PurgeOldNotifications @olderThanDays";
        AddParam(cmd, "@olderThanDays", olderThanDays);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return 0;
        return reader.GetInt32(0);
    }

    /// <summary>Wywołuje sp_PurchaseRewardItem – zakup przedmiotu ze sklepu.</summary>
    public async Task<GroupOperationResult> PurchaseRewardItemAsync(string userId, Guid rewardItemId)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC dbo.sp_PurchaseRewardItem @userId, @rewardItemId";

        AddParam(cmd, "@userId",       userId);
        AddParam(cmd, "@rewardItemId", rewardItemId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return new GroupOperationResult(false, "Brak odpowiedzi z procedury.");

        return new GroupOperationResult(
            reader.GetInt32(0) == 1,
            reader.GetString(1)
        );
    }

    /// <summary>Wywołuje sp_AwardBadge – przyznaje odznakę użytkownikowi.</summary>
    public async Task<GroupOperationResult> AwardBadgeAsync(string userId, Guid badgeId)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "EXEC dbo.sp_AwardBadge @userId, @badgeId";

        AddParam(cmd, "@userId",  userId);
        AddParam(cmd, "@badgeId", badgeId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return new GroupOperationResult(false, "Brak odpowiedzi z procedury.");

        return new GroupOperationResult(
            reader.GetInt32(0) == 1,
            reader.GetString(1)
        );
    }

    // ── FUNKCJE ──────────────────────────────────────────────────────────
    // Uwaga: funkcje skalarne wywołujemy przez surowy ADO.NET (ExecuteScalarAsync),
    // nie przez SqlQueryRaw<T>().FirstOrDefaultAsync() – w EF Core 10 ta druga
    // forma czasem gubi nazwę kolumny przy wewnętrznej kompozycji LINQ
    // (błąd: "No column name was specified for column 1 of 's'"), mimo że
    // SQL ma poprawny alias AS Value. ExecuteScalarAsync nie ma tego problemu,
    // bo nie przechodzi przez warstwę LINQ-nad-SQL.

    /// <summary>Wywołuje fn_GetFullName – zwraca imię i nazwisko użytkownika.</summary>
    public async Task<string> GetFullNameAsync(string userId)
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_GetFullName(@userId)",
            ("@userId", userId));
        return result as string ?? "Nieznany użytkownik";
    }

    /// <summary>Wywołuje fn_CountUserConsultations – zlicza konsultacje w statusie.</summary>
    public async Task<int> CountUserConsultationsAsync(string userId, int statusId)
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_CountUserConsultations(@userId, @statusId)",
            ("@userId", userId), ("@statusId", statusId));
        return result is int i ? i : 0;
    }

    /// <summary>Wywołuje fn_AverageWalletBalance – zwraca średnie saldo portfela.</summary>
    public async Task<decimal> GetAverageWalletBalanceAsync()
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_AverageWalletBalance()");
        return result is decimal d ? d : 0m;
    }

    /// <summary>Wywołuje fn_GetTotalEarnedPoints – suma zdobytych punktów (dodatnich transakcji).</summary>
    public async Task<int> GetTotalEarnedPointsAsync(string userId)
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_GetTotalEarnedPoints(@userId)",
            ("@userId", userId));
        return result is int i ? i : 0;
    }

    /// <summary>Wywołuje fn_CountUserBadges – liczba zdobytych odznak.</summary>
    public async Task<int> CountUserBadgesAsync(string userId)
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_CountUserBadges(@userId)",
            ("@userId", userId));
        return result is int i ? i : 0;
    }

    // ── HELPER ───────────────────────────────────────────────────────────

    /// <summary>
    /// Wykonuje SELECT funkcji skalarnej przez ADO.NET i zwraca surową wartość
    /// (lub null/DBNull jeśli funkcja zwróciła NULL).
    /// </summary>
    private async Task<object?> ExecuteScalarFunctionAsync(
        string sql, params (string Name, object? Value)[] parameters)
    {
        var conn = _db.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;

        foreach (var (name, value) in parameters)
            AddParam(cmd, name, value ?? DBNull.Value);

        var result = await cmd.ExecuteScalarAsync();
        return result is DBNull ? null : result;
    }

    private static void AddParam(IDbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value         = value ?? DBNull.Value;
        cmd.Parameters.Add(p);
    }
}