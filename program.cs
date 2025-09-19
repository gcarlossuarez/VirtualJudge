using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

class Program
{
    static void Main() {
        string val;
        while (!string.IsNullOrEmpty(val = Console.ReadLine())) {
            Console.WriteLine($"valor enviado={val}");
        }
        Console.WriteLine("Hola C#!");
    }
}
