using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hulki.Web.Models;

public class PointTransaction
{
    [Key]
    public Guid Id { get; set; }

    public int Amount { get; set; }
    public string Description { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.Now;

    public Guid WalletId { get; set; }
    [ForeignKey("WalletId")]
    public virtual Wallet Wallet { get; set; }
}