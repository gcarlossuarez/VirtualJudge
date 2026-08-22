/*
 * La forma natural de resolverlo es utilizando una pila.

Se recorre la expresión de izquierda a derecha. Si el elemento encontrado es un número, se introduce en la pila. Si es un operador,
se extraen los dos últimos valores almacenados.

Hay que tener cuidado con el orden:

operandoDerecho = Pop()
operandoIzquierdo = Pop()

Luego:

resultado = operandoIzquierdo operador operandoDerecho

y el resultado vuelve a introducirse en la pila.
 * */
using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());

        string[] elementos = Console.ReadLine()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Stack<int> pila = new Stack<int>();

        // En prefija se recorre la expresión de derecha a izquierda.
        for (int i = elementos.Length - 1; i >= 0; i--)
        {
            string elemento = elementos[i];

            if (int.TryParse(elemento, out int numero))
            {
                pila.Push(numero);
            }
            else
            {
                int operandoIzquierdo = pila.Pop();
                int operandoDerecho = pila.Pop();

                int resultado = 0;

                switch (elemento)
                {
                    case "+":
                        resultado = operandoIzquierdo + operandoDerecho;
                        break;
                    case "-":
                        resultado = operandoIzquierdo - operandoDerecho;
                        break;
                    case "*":
                        resultado = operandoIzquierdo * operandoDerecho;
                        break;
                    case "/":
                        resultado = operandoIzquierdo / operandoDerecho;
                        break;
                }

                pila.Push(resultado);
            }
        }

        Console.WriteLine(pila.Pop());
    }
}
