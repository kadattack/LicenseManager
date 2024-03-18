using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LicenseValidationServer.Models;

namespace LicenseValidationServer.Models;


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
