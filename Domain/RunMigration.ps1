param([string]$ContextName="", [string]$RelativeMigrationDir="")
# Runs the migration

if([string]::IsNullOrEmpty($ContextName))
{
    # One to many
    dotnet ef migrations add InitialCreate -p Domain.csproj -s Domain.csproj --context Bcan.Domain.Contexts.OtmContext -o "Migrations\OneToMany"
    dotnet ef database update -p Domain.csproj -s Domain.csproj --context Bcan.Domain.Contexts.OtmContext

    # one to one
    dotnet ef migrations add InitialCreate -p Domain.csproj -s Domain.csproj --context Bcan.Domain.Contexts.OtoContext -o "Migrations\OneToOne"
    dotnet ef database update -p Domain.csproj -s Domain.csproj --context Bcan.Domain.Contexts.OtoContext

    # Many to many
    dotnet ef migrations add InitialCreate -p Domain.csproj -s Domain.csproj --context Bcan.Domain.Contexts.MtmContext -o "Migrations\ManyToMany"
    dotnet ef database update -p Domain.csproj -s Domain.csproj --context Bcan.Domain.Contexts.MtmContext
}
else
{
    if([string]::IsNullOrEmpty($RelativeMigrationDir))
    {
        $RelativeMigrationDir = "Migrations"
    }

    dotnet ef migrations add InitialCreate -p Domain.csproj -s Domain.csproj --context $ContextName -o $RelativeMigrationDir
    dotnet ef database update -p Domain.csproj -s Domain.csproj --context $ContextName
}

