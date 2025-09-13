#dotnet nuget locals all --clear
#dotnet restore --source /home/vboxuser/nuget-local
#dotnet restore --ignore-failed-source
dotnet build
dotnet run --urls http://localhost:5000
