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


public class SqlObjectsService
{
    private readonly ApplicationDbContext _db;

    public SqlObjectsService(ApplicationDbContext db)
    {
        _db = db;
    }

    // PROCEDURY

    // sp_GetUserStats
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

    // sp_AddPatientToGroup
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

    // sp_PurgeOldNotifications
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

    // sp_PurchaseRewardItem
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

    // sp_AwardBadge
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

    // FUNKCJE 

    // fn_GetFullName
    public async Task<string> GetFullNameAsync(string userId)
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_GetFullName(@userId)",
            ("@userId", userId));
        return result as string ?? "Nieznany użytkownik";
    }

    // fn_CountUserConsultations
    public async Task<int> CountUserConsultationsAsync(string userId, int statusId)
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_CountUserConsultations(@userId, @statusId)",
            ("@userId", userId), ("@statusId", statusId));
        return result is int i ? i : 0;
    }

    // fn_AverageWalletBalance
    public async Task<decimal> GetAverageWalletBalanceAsync()
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_AverageWalletBalance()");
        return result is decimal d ? d : 0m;
    }

    // fn_GetTotalEarnedPoints
    public async Task<int> GetTotalEarnedPointsAsync(string userId)
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_GetTotalEarnedPoints(@userId)",
            ("@userId", userId));
        return result is int i ? i : 0;
    }

    // fn_CountUserBadges
    public async Task<int> CountUserBadgesAsync(string userId)
    {
        var result = await ExecuteScalarFunctionAsync(
            "SELECT dbo.fn_CountUserBadges(@userId)",
            ("@userId", userId));
        return result is int i ? i : 0;
    }


    
    // helper
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