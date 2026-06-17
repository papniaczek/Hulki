using System;
using System.ComponentModel.DataAnnotations;

namespace Hulki.Web.Models;

/// <summary>
/// Własna tabela użytkowników – niezależna od ASP.NET Identity (AspNetUsers).
/// Hasła hashowane ręcznie przez CustomPasswordHasher (PBKDF2-SHA256, 100k iteracji).
/// Nazwa "CustomUser" celowo różna od "User", by uniknąć kolizji z typami Identity.
/// </summary>
public class CustomUser
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Format: base64(sól):base64(hash PBKDF2-SHA256).
    /// Nigdy plaintext.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsTherapist { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Opcjonalny link do konta Identity w AspNetUsers.
    /// </summary>
    [MaxLength(450)]
    public string? AspNetUserId { get; set; }
}
