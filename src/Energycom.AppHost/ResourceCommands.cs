namespace Energycom.AppHost;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public static class ResourceCommands
{
    
    public static IResourceBuilder<T> WithDashboardResource<T>(this IResourceBuilder<T> builder,
        string name,
        string displayName,
        string openApiUiPath

    ) where T : IResourceWithEndpoints
    => builder.WithCommand(
            name,
            displayName,
            executeCommand:  _ =>
            {
                try
                {
                    var endpoint = builder.GetEndpoint("https");
                    var url = $"{endpoint.Url}/{openApiUiPath}";
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                
                    return Task.FromResult(CommandResults.Success()) ;
                }catch (Exception e)
                {
                    return Task.FromResult(new ExecuteCommandResult( ){ Success = false, ErrorMessage = e.Message });
                }
               
                
            },
            updateState: context => context.ResourceSnapshot.HealthStatus == HealthStatus.Healthy? ResourceCommandState.Enabled : ResourceCommandState.Disabled,
            iconName: "DocumentDataLink",
            iconVariant: IconVariant.Filled
            );
    
    
    
    public static IResourceBuilder<T> WithCreateCommand<T>(
        this IResourceBuilder<T> dbBuilder,
        bool autocreate = false
    ) where T : PostgresDatabaseResource
    {
        var commandName = "create-database";
        dbBuilder.WithCommand(
            commandName,
            "Create Database",
            async ctx =>
            {
                var connectionString = await dbBuilder.Resource
                    .ConnectionStringExpression
                    .GetValueAsync(ctx.CancellationToken);

                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSnakeCaseNamingConvention();
                optionsBuilder.UseNpgsql(connectionString);
                var dbCtx = new DbContext(optionsBuilder.Options);
                
                await dbCtx.Database.MigrateAsync();

                return CommandResults.Success();
            },
            ctx => ResourceCommandState.Enabled
        );

        if (autocreate)
        {
            dbBuilder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(
                dbBuilder.Resource.Parent, async (ctx, cancel) =>
                {
                    var commandAnnotation = dbBuilder.Resource.Annotations
                                                .OfType<ResourceCommandAnnotation>()
                                                .SingleOrDefault(a => a.Name == commandName)
                                            ?? throw new InvalidOperationException("cannot autocreate the database -  the 'create-database' command is not registered'");

                    await commandAnnotation.ExecuteCommand.Invoke(new ExecuteCommandContext()
                    {
                        ServiceProvider = ctx.Services,
                        ResourceName = dbBuilder.Resource.Name,
                        CancellationToken = cancel
                    });
                }
            );
        }

        return dbBuilder;
    }
    
    
        

     
}
    

