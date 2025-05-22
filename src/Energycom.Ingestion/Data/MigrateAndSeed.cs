using Energycom.Ingestion.Entities;
using Microsoft.EntityFrameworkCore;

namespace Energycom.Ingestion.Data;



public static class MigrateAndSeed
{
    
    private record LocationInfo(decimal Latitude ,decimal Longitude ,decimal Altitude, Grid AssociatedGrid );


    private static List<LocationInfo> ReadingQueue =
    [
        new LocationInfo(55.704459m, 9.104887m, 0, Grid.DK1),
        new LocationInfo(55.804195m, 9.252219m, 0, Grid.DK1),
        new LocationInfo(55.843865m, 9.231267m, 0, Grid.DK1),
        new LocationInfo(55.753115m, 8.917907m, 0, Grid.DK1),
        new LocationInfo(55.869850m, 9.031852m, 0, Grid.DK1),
        new LocationInfo(55.793754099091m, 9.206863731838936m, 0, Grid.DK1),

        new LocationInfo(55.400354m, 10.271406m, 0, Grid.DK2),
        new LocationInfo(55.584136m, 11.434430m, 0, Grid.DK2),
        new LocationInfo(55.70499052361521m, 12.261875717241127m, 0, Grid.DK2),
        new LocationInfo(55.412463m, 11.956562m, 0, Grid.DK2),
        new LocationInfo(55.294023m, 11.949378m, 0, Grid.DK2),
        
        new LocationInfo(55.704459m, 9.204887m, 0, Grid.DK1),
        new LocationInfo(55.804195m, 9.22219m, 0, Grid.DK1),
        new LocationInfo(55.843865m, 9.231267m, 0, Grid.DK1),
        new LocationInfo(55.553115m, 8.997907m, 0, Grid.DK1),
        new LocationInfo(55.769850m, 9.081852m, 0, Grid.DK1),
        new LocationInfo(55.893754m, 9.105731m, 0, Grid.DK1),
    
        new LocationInfo(55.410354m, 10.261406m, 0, Grid.DK2),
        new LocationInfo(55.594136m, 11.455430m, 0, Grid.DK2),
        new LocationInfo(55.804411m, 12.261331m, 0, Grid.DK2),
        new LocationInfo(55.4133333m, 11.133111m, 0, Grid.DK2),
        new LocationInfo(55.25384023m, 11.1319378m, 0, Grid.DK2)

    ];

    
    public static async Task Execute(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ECOMDbContext>();
      
        await context.Database.MigrateAsync();
        
        //Seed data for the the take home
        
        // - Group   
        //     - Meter 
        //         - Reading
        //
        
        // - Site   
        //     - Meter 
        //         - Reading
        //
        
        //check if groups exist
        if (await context.Groups.AnyAsync())
        {
            return;
        }
        
        //Creating two groups 
        
        var groups = new List<Group>();
        
        var group1 = new Group
        {
            Id = 1,
            Name = "Group 1",
            Meters = []
        };
        
        var group2 = new Group
        {
            Id = 2,
            Name = "Group 2",
            Meters = []
        };
        
        var group3 = new Group
        {
            Id = 3,
            Name = "Group 3",
            Meters = []
        };

        var group4 = new Group
        {
            Id = 4,
            Name = "Group 4",
            Meters = []
        };
        
        groups.Add(group1);
        groups.Add(group2);
        groups.Add(group3);
        groups.Add(group4);
        

        int meterId = 0;
        int siteId = 0;
        foreach (var group in groups)
        {
            
            List<Site> sites =[];
            //generate between 3 and 5 sites
            
            var numberOfSites = Random.Shared.Next(3, 6);
            
            for (var i = 0; i < numberOfSites; i++)
            {
                //random lat lng in denmark
              
                //get the next location from the list
                var listIndex = siteId % ReadingQueue.Count;
                var location = ReadingQueue[listIndex];
                
                var site = new Site
                {
                    Id = ++siteId,
                    Name = $"Site {siteId}",
                    Grid = location.AssociatedGrid,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Altitude = location.Altitude,
                    TimeZone = "Europe/Copenhagen",
                    Meters = []
                };
                
                context.Sites.Add(site);
                sites.Add(site);
            }
            
            //create a random number of meters between 13 and 20
            
            var numberOfMeters = Random.Shared.Next(13, 21);
            
            for (var i = 0; i < numberOfMeters; i++)
            {
                var siteIndex = Random.Shared.Next(0, sites.Count);
                var site = sites[siteIndex];
                
                var meter = new Meter
                {
                    Id = ++meterId,
                    Site = site,
                    GroupId = group.Id,
                    MeterNumber = $"C{meterId.ToString().PadLeft(5, '0')}",
                    Readings = [],
                    Configuration = new MeterConfiguration
                    {
                        Id = meterId,
                        Type = MeterType.Consumption,
                        BaseValue = Random.Shared.Next(50, 201),
                        CanSkipReadings = false,
                        CanDuplicateReadings = false,
                        CanHaveUnparsedReadings = false
                    }
                };
                
                group.Meters.Add(meter);
            }
            
            int numberSkippingMetersPerGroup = Random.Shared.Next(1, 4);
            int numberDuplicatingMeters = Random.Shared.Next(1, 4);
            int numberOfProductionMeters = Random.Shared.Next(1, 6);
            int numberFaultyMeters = Random.Shared.Next(1, 3);
            
            //select ids for skipping  and duplicating meters
            
            var metersToConfigure = group.Meters
                .OrderBy(x => Guid.NewGuid())
                .Take(numberSkippingMetersPerGroup + numberDuplicatingMeters + numberFaultyMeters)
                .ToList();
            
            //configure meters to skip readings
            for(var i = 0; i < numberSkippingMetersPerGroup; i++)
            {
                var meter = metersToConfigure[i];
                meter.Configuration.CanSkipReadings = true;
            }
            
            //configure meters to duplicate readings
            for(var i = numberSkippingMetersPerGroup; i < numberDuplicatingMeters + numberSkippingMetersPerGroup; i++)
            {
                var meter = metersToConfigure[i];
                meter.Configuration.CanDuplicateReadings = true;
            }
            
            //configure meters to have unparsed readings
            for(var i = numberDuplicatingMeters + numberSkippingMetersPerGroup; i < numberDuplicatingMeters + numberSkippingMetersPerGroup + numberFaultyMeters; i++)
            {
                var meter = metersToConfigure[i];
                meter.Configuration.CanHaveUnparsedReadings = true;
            }


            for (var i = 0; i < numberOfProductionMeters; i++)
            {
                // get a random site for this meter
                
                var siteIndex = Random.Shared.Next(0, sites.Count);
                var site = sites[siteIndex];
                var meter = new Meter
                {
                    Id = ++meterId,
                    Site = site,
                    GroupId = group.Id,
                    MeterNumber = $"P{meterId.ToString().PadLeft(5, '0')}",
                    Readings = [],
                    Configuration = new MeterConfiguration
                    {
                        Id = meterId,
                        Type = MeterType.Consumption,
                        BaseValue = -1* Random.Shared.Next(50, 201),
                        CanSkipReadings = false,
                        CanDuplicateReadings = false,
                        CanHaveUnparsedReadings = false
                    }
                };
                
                group.Meters.Add(meter);
            }
            
        }
        
        
        context.Groups.AddRange(groups);        
        await context.SaveChangesAsync();
    }
    
    
}