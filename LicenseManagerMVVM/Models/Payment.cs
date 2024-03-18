using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseManagerMVVM.Models;

namespace LicenseManagerMVVM.Models;


[Table("payment")]
public class Payment
{
    [Key]
    public int payment_id { get; set; }
    public DateTime payment_date { get; set; }
    public Client client { get; set; }
    public Product product { get; set; }
    public License license { get; set; }


}
