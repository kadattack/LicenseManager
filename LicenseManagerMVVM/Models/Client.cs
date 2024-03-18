using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LicenseManagerMVVM.Models;

using Microsoft.EntityFrameworkCore;

[Table("client")]
public class Client
{
    [Key]
    public int id { get; set; }
    public string client_name { get; set; }
    public string address { get; set; }
    public string phonenumber { get; set; }
    public string payment_account { get; set; }
    public string client_key { get; set; }
    public string authorization_token { get; set; }
    public DateTime client_date_added { get; set; }
   
    public List<License> licenses { get; set; }
}