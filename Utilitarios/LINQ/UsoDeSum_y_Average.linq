<Query Kind="Program" />

void Main()
{
    List<int> numeros = new List<int> { 10, 20, 30, 40, 50 };
    var suma = numeros.Sum();
    var promedio = numeros.Average();
    
    $"Suma: {suma}, Promedio: {promedio}".Dump();
}
