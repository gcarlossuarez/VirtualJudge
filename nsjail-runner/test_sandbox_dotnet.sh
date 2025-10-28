# Crear test simple
rm -rf /tmp/test_dotnet
mkdir -p /tmp/test_dotnet
cat > /tmp/test_dotnet/Program.cs << 'EOF'
using System;

class Program {
    static void Main() {
        string input = Console.ReadLine();
        Console.WriteLine($"HOLA: {input}");
    }
}
EOF

cat > /tmp/test_dotnet/test_dotnet.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
EOF

# Compilar FUERA del sandbox
cd /tmp/test_dotnet
dotnet build -c Release

# Ejecutar DENTRO del sandbox con RUTA COMPLETA
echo "Mundo" | sudo nsjail \
  -M o \
  --time_limit 5 \
  --rlimit_cpu 3 \
  --rlimit_as 268435456 \
  --user 65534 \
  --group 65534 \
  --bindmount_ro /bin \
  --bindmount_ro /lib \
  --bindmount_ro /lib64 \
  --bindmount_ro /usr \
  --bindmount_ro /etc \
  --bindmount "/tmp/test_dotnet:/app" \
  --quiet \
  -- /usr/bin/dotnet /app/bin/Release/net8.0/test_dotnet.dll
