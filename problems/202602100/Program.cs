/*
 *
 * Problema 202602100
Para evaluar una expresión postfija se pueden procesar sus elementos de izquierda a derecha utilizando una pila.

Cada vez que se encuentra un número, este se almacena en la pila. Cuando se encuentra un operador, se extraen de la pila los dos
últimos operandos, se realiza la operación correspondiente y el resultado se vuelve a almacenar en la pila.

 * */

using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        int n = int.Parse(Console.ReadLine());

        string[] elementos = Console.ReadLine().Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries
        );

        Stack<int> pila = new Stack<int>();

        foreach (string elemento in elementos)
        {
            if (int.TryParse(elemento, out int numero))
            {
                pila.Push(numero);
            }
            else
            {
                int operandoDerecho = pila.Pop();
                int operandoIzquierdo = pila.Pop();

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
