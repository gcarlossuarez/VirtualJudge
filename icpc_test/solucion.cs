using System;
class A {
    static void Main() {
        var p = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine(int.Parse(p[0]) + int.Parse(p[1]));
        Console.WriteLine("Ejecutado!!!");
    }
}
