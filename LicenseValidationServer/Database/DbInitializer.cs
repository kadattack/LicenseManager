using System.Text;
using System.Text.Json;
using LicenseValidationServer.Models;

namespace LicenseValidationServer.Data;
using System.Collections.Generic;
using System.Threading.Tasks;


    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            
            
            var cli1 = new Client
            {
                address = "address",
                authorization_token = "tokenbearer",
                client_date_added = DateTime.UtcNow,
                client_key = "client_key",
                client_name = "some company d.o.o.",
                licenses = new List<License>(){},
                payment_account = "538242300320",
                phonenumber = "058135812"
                
            };

            var product1 = new Product() {
                product_name = "PdfGenerator"
            };
            
            var product2 = new Product() {
                product_name = "PdfGenerator2"
            };


            
            
            



            
            
            var cli2 = new Client
            {
                address = "address2",
                authorization_token = "tokenbearer2",
                client_date_added = DateTime.UtcNow,
                client_key = "client_key2",
                client_name = "some company d.o.o.2",
                licenses = new List<License>(){},
                payment_account = "45652656",
                phonenumber = "4548464456"
            };
            
           



            using var transaction =  context.Database.BeginTransaction();
                try
                {
                    context.Add(cli1);
                    context.Add(cli2);
                    context.Add(product1);
                    context.Add(product2);
                    
                    context.SaveChanges();
                    
                    Utils.Utils util = new Utils.Utils();
                    var lic1 = util.GenerateLicense("/home/newdev/RiderProjects/Client/Client/License/license.bin", context, cli1, product1);
                    var lic2 = util.GenerateLicense("/home/newdev/RiderProjects/Client/Client/License/license2.bin", context, cli1, product2);
                    var lic3 = util.GenerateLicense("/home/newdev/RiderProjects/Client/Client/License/license3.bin", context, cli2, product2);
                    var lic4 = util.GenerateLicense("/home/newdev/RiderProjects/Client/Client/License/license4.bin", context, cli2, product1);
                    
                    
                    var payment1 = new Payment() {
                        product = product1,
                        payment_date = DateTime.UtcNow,
                        client = cli1,
                        license = lic1
                    };
                    
                    var payment2 = new Payment() {
                        product = product1,
                        payment_date = DateTime.UtcNow,
                        client = cli1,
                        license = lic1
                    };
                    
                    context.Add(payment1);
                    context.Add(payment2);
                    context.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
        }
    }
