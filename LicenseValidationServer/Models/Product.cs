using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseValidationServer.Models;

[Table("product")]
public class Product
{
    [Key]
    public int product_id { get; set; }
    public string product_name { get; set; }
}
