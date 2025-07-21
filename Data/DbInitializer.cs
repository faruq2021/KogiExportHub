using KogiExportHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KogiExportHub.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Create default roles if they don't exist
            if (!context.Roles.Any())
            {
                var roles = new[] 
                { 
                    "Admin", 
                    "Farmer", 
                    "Miner", 
                    "Aggregator", 
                    "Investor", 
                    "Technician", 
                    "GovernmentOfficial" 
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Add infrastructure categories if none exist
            if (!context.InfrastructureCategories.Any())
            {
                var infrastructureCategories = new[]
                {
                    new InfrastructureCategory { 
                        Name = "Transportation", 
                        Description = "Roads, bridges, railways, and transportation infrastructure" 
                    },
                    new InfrastructureCategory { 
                        Name = "Water & Sanitation", 
                        Description = "Water supply systems, sewage treatment, and sanitation facilities" 
                    },
                    new InfrastructureCategory { 
                        Name = "Energy & Power", 
                        Description = "Power generation, transmission lines, and renewable energy projects" 
                    },
                    new InfrastructureCategory { 
                        Name = "Healthcare", 
                        Description = "Hospitals, clinics, and medical facilities" 
                    },
                    new InfrastructureCategory { 
                        Name = "Education", 
                        Description = "Schools, universities, and educational facilities" 
                    },
                    new InfrastructureCategory { 
                        Name = "Telecommunications", 
                        Description = "Internet infrastructure, mobile towers, and communication networks" 
                    },
                    new InfrastructureCategory { 
                        Name = "Agriculture", 
                        Description = "Irrigation systems, storage facilities, and agricultural infrastructure" 
                    },
                    new InfrastructureCategory { 
                        Name = "Housing", 
                        Description = "Residential developments and housing projects" 
                    }
                };

                context.InfrastructureCategories.AddRange(infrastructureCategories);
                await context.SaveChangesAsync();
            }

            // Add product categories if none exist
            if (!context.ProductCategories.Any())
            {
                var categories = new[]
                {
                    new ProductCategory { 
                        Name = "Agricultural Products", 
                        Description = "Farm produce and agricultural goods",
                        DefaultUnit = "kg" // Kilograms for agricultural products
                    },
                    new ProductCategory { 
                        Name = "Minerals", 
                        Description = "Mined resources and minerals",
                        DefaultUnit = "ton" // Tons for minerals
                    },
                    new ProductCategory { 
                        Name = "Crafts", 
                        Description = "Handmade crafts and artisanal products",
                        DefaultUnit = "piece" // Individual pieces for crafts
                    },
                    new ProductCategory { 
                        Name = "Services", 
                        Description = "Professional services offered",
                        DefaultUnit = "hour" // Hours for services
                    }
                };

                context.ProductCategories.AddRange(categories);
                await context.SaveChangesAsync(); // Save categories immediately
            }

            // Add locations if none exist
            if (!context.Locations.Any())
            {
                var locations = new[]
                {
                    // All 21 Local Government Areas of Kogi State with approximate coordinates
                    new Location { Name = "Adavi", City = "Adavi", State = "Kogi", Country = "Nigeria", Latitude = "7.6167", Longitude = "6.4667" },
                    new Location { Name = "Ajaokuta", City = "Ajaokuta", State = "Kogi", Country = "Nigeria", Latitude = "7.5667", Longitude = "6.6667" },
                    new Location { Name = "Ankpa", City = "Ankpa", State = "Kogi", Country = "Nigeria", Latitude = "7.4000", Longitude = "7.6333" },
                    new Location { Name = "Bassa", City = "Bassa", State = "Kogi", Country = "Nigeria", Latitude = "7.9333", Longitude = "6.5333" },
                    new Location { Name = "Dekina", City = "Dekina", State = "Kogi", Country = "Nigeria", Latitude = "7.6833", Longitude = "7.0500" },
                    new Location { Name = "Ibaji", City = "Ibaji", State = "Kogi", Country = "Nigeria", Latitude = "7.0000", Longitude = "6.7500" },
                    new Location { Name = "Idah", City = "Idah", State = "Kogi", Country = "Nigeria", Latitude = "7.1167", Longitude = "6.7333" },
                    new Location { Name = "Igalamela-Odolu", City = "Igalamela-Odolu", State = "Kogi", Country = "Nigeria", Latitude = "7.3000", Longitude = "6.9000" },
                    new Location { Name = "Ijumu", City = "Ijumu", State = "Kogi", Country = "Nigeria", Latitude = "7.8833", Longitude = "5.9667" },
                    new Location { Name = "Kabba/Bunu", City = "Kabba", State = "Kogi", Country = "Nigeria", Latitude = "7.8167", Longitude = "6.0833" },
                    new Location { Name = "Kogi", City = "Kogi", State = "Kogi", Country = "Nigeria", Latitude = "7.7833", Longitude = "6.7667" },
                    new Location { Name = "Lokoja", City = "Lokoja", State = "Kogi", Country = "Nigeria", Latitude = "7.8000", Longitude = "6.7333" },
                    new Location { Name = "Mopa-Muro", City = "Mopa-Muro", State = "Kogi", Country = "Nigeria", Latitude = "7.9500", Longitude = "5.9000" },
                    new Location { Name = "Ofu", City = "Ofu", State = "Kogi", Country = "Nigeria", Latitude = "7.3500", Longitude = "6.9500" },
                    new Location { Name = "Ogori/Magongo", City = "Ogori/Magongo", State = "Kogi", Country = "Nigeria", Latitude = "7.7500", Longitude = "5.8500" },
                    new Location { Name = "Okehi", City = "Okehi", State = "Kogi", Country = "Nigeria", Latitude = "7.5500", Longitude = "6.2500" },
                    new Location { Name = "Okene", City = "Okene", State = "Kogi", Country = "Nigeria", Latitude = "7.5500", Longitude = "6.2333" },
                    new Location { Name = "Olamaboro", City = "Olamaboro", State = "Kogi", Country = "Nigeria", Latitude = "7.5000", Longitude = "7.1500" },
                    new Location { Name = "Omala", City = "Omala", State = "Kogi", Country = "Nigeria", Latitude = "7.2500", Longitude = "7.2000" },
                    new Location { Name = "Yagba East", City = "Yagba East", State = "Kogi", Country = "Nigeria", Latitude = "8.1000", Longitude = "5.8000" },
                    new Location { Name = "Yagba West", City = "Yagba West", State = "Kogi", Country = "Nigeria", Latitude = "8.2000", Longitude = "5.7500" }
                };

                context.Locations.AddRange(locations);
                await context.SaveChangesAsync(); // Save locations immediately
            }
        }
    }
}