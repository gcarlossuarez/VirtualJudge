sudo apt-get update
sudo apt-get install -y ca-certificates curl gnupg lsb-release

# Agregar la clave oficial de Docker
sudo mkdir -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

# Agregar repo oficial
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Instalar Docker Engine
sudo apt-get update

# instala docker y el daemon (dockerd).
sudo apt-get install -y docker.io


# Verificar instalacion
docker --version
sudo systemctl status docker

# Probar la instalacion
sudo docker run hello-world

# Para utilizar docker sin sudo. En este caso, en la maquina en donde se instal ese dia, era la cuenta docente.
sudo usermod -aG docker docente
# NOTA. - Reiniciar cuenta, para usermod, tome efecto


# Probar. Se deberia poder ejectuar sin sudo
docker ps
docker run hello-world


# Probar que el contendiero compila bien. Clonar lo que falta
echo 'using System; class P { static void Main(){ Console.WriteLine("Hola C#"); }}' > P.cs
docker run --rm -v $PWD:/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet new console -n testapp
docker run --rm -v $PWD:/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet build testapp

3. Probar que cada contenedor compila/ejecuta

Ejemplo C#:

echo 'using System; class P { static void Main(){ Console.WriteLine("Hola C#"); }}' > P.cs
docker run --rm -v $PWD:/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet new console -n testapp
docker run --rm -v $PWD:/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 dotnet build testapp


Ejemplo C++:

echo '#include <iostream> 
int main(){ std::cout<<"Hola C++"; }' > p.cpp
docker run --rm -v $PWD:/app -w /app gcc bash -c "g++ p.cpp -o p && ./p"
