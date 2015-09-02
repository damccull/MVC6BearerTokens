using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity;
using Microsoft.Data.Entity;
using Microsoft.Framework.Logging;
using MVC6BearerToken.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MVC6BearerToken.DAL {
    public class DbSeeder {
        private readonly IHostingEnvironment env;
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILoggerFactory logger;

        public DbSeeder(IHostingEnvironment env, ApplicationDbContext db, UserManager<ApplicationUser> userManager, ILoggerFactory logger) {
            this.env = env;
            this.db = db;
            this.userManager = userManager;
            this.logger = logger;
        }
        public async void Seed() {
            await db.Database.EnsureCreatedAsync();
            if(env.IsDevelopment()) {
                await createClients();
            }
        }

        private async Task createClients() {
            List<Client> clients = new List<Client> {
                new Client {
                    Name = "angularWebClient",
                    Active = true,
                    AllowedOrigin = "http://localhost:33938",
                    ApplicationType = ApplicationTypes.Javascript,
                    Id = "ngAngularApp",
                    RefreshTokenLifeTime = 5,
                    Secret = "ce6958c2-fa41-4e72-83cb-65662c472fb6" //Not used for this client, but here anyways
                },
                new Client {
                    Name = "nativeClient",
                    Active = true,
                    AllowedOrigin = "*",
                    ApplicationType = ApplicationTypes.Native,
                    Id = "nativeClient",
                    RefreshTokenLifeTime = 5,
                    Secret = "c8a53f0a-543f-4deb-ac31-c5a0154b18e2" //Not used for this client, but here anyways
                }
            };

            foreach(var client in clients) {
                var existing = await db.Clients.SingleOrDefaultAsync(c => c.Id == client.Id);
                if(existing == null) {
                    db.Clients.Add(client);
                } else {
                    db.Clients.Remove(existing);
                    await db.SaveChangesAsync();
                    db.Clients.Add(client);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
