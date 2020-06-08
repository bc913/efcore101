param([string]$ContextName="", [string]$RelativeMigrationDir="")
# Runs the migration

if([string]::IsNullOrEmpty($ContextName))
{
    # One to many
    dotnet ef migrations add InitialCreate -p Db.Sqlite.csproj -s Db.Sqlite.csproj --context Db.Sqlite.Contexts.OtmContext -o "Migrations\OneToMany"
    dotnet ef database update -p Db.Sqlite.csproj -s Db.Sqlite.csproj --context Db.Sqlite.Contexts.OtmContext

    # one to one
    dotnet ef migrations add InitialCreate -p Db.Sqlite.csproj -s Db.Sqlite.csproj --context Db.Sqlite.Contexts.OtoContext -o "Migrations\OneToOne"
    dotnet ef database update -p Db.Sqlite.csproj -s Db.Sqlite.csproj --context Db.Sqlite.Contexts.OtoContext

    # Many to many
    dotnet ef migrations add InitialCreate -p Db.Sqlite.csproj -s Db.Sqlite.csproj --context Db.Sqlite.Contexts.MtmContext -o "Migrations\ManyToMany"
    dotnet ef database update -p Db.Sqlite.csproj -s Db.Sqlite.csproj --context Db.Sqlite.Contexts.MtmContext
}
else
{
    if([string]::IsNullOrEmpty($RelativeMigrationDir))
    {
        $RelativeMigrationDir = "Migrations"
    }

    dotnet ef migrations add InitialCreate -p Db.Sqlite.csproj -s Db.Sqlite.csproj --context $ContextName -o $RelativeMigrationDir
    dotnet ef database update -p Db.Sqlite.csproj -s Db.Sqlite.csproj --context $ContextName
}

