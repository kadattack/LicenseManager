using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseValidationServer.Models;

using Microsoft.EntityFrameworkCore;

[Table("license")]
public class License
{
    [Key]
    public int license_id { get; set; }
    public bool active { get; set; }
    public DateTime activation_refreshed { get; set; }
    public DateTime first_activation { get; set; }
    [StringLength(255)]
    public string license { get; set; }
    [StringLength(255)]
    public string? hardware_id { get; set; }
    [StringLength(255)]
    public bool hardware_lock { get; set; }
    public Client client { get; set; }
    
    public List<Payment> payments { get; set; }

    public Product product { get; set; }
    public byte[] server_private_ecdh_key { get; set; }
    public byte[] server_public_ecdh_key { get; set; }
    public byte[] server_private_ecdsa_key { get; set; }
    public byte[] server_public_ecdsa_key { get; set; }
    public byte[] client_private_ecdh_key { get; set; }
    public byte[] client_public_ecdh_key { get; set; }
    public byte[] client_private_ecdsa_key { get; set; }
    public byte[] client_public_ecdsa_key { get; set; }
    
}