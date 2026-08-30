using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string input = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrEmpty(input)) return;
        
        int n = int.Parse(input);
        NQueenProblem nQueen = new NQueenProblem(n);
        nQueen.Solve();
    }

}


class NQueenProblem
{
    private int[,] board;
    private int size;

    public NQueenProblem(int size) {
        this.size = size;
        board = new int[size, size];
    }

    private bool IsSafe(int row, int col)
	{
        // Verifica esta fila, en el lado izquierdo
        for (int i = 0; i < col; i++)
		{
            if (board[row, i] == 1)
			{
                return false;
			}
		}

        // Verifica la diagonal superior, en el lado izquierdo
        for (int i = row, j = col; i >= 0 && j >= 0; i--, j--)
		{
            if (board[i, j] == 1)
			{
                return false;
			}
		}

        // Verifica la diagonal inferior, en el lado izquierdo
        for (int i = row, j = col; j >= 0 && i < size; i++, j--)
		{
            if (board[i, j] == 1)
            {
				return false;
			}
		}
		
        return true;
    }

    private bool SolveNQueen(int col)
	{
        if (col >= size)
		{
            return true;
		}

        for (int i = 0; i < size; i++)
		{
            if (IsSafe(i, col)) // Verifica si se puede colocar una reina en la fila y columna actual
			{
                board[i, col] = 1;
                if (SolveNQueen(col + 1)) // Intenta ver si se puede colocar una reina, en la siguiente columna; ya que, no se puede en la misma columna
                {
					return true;
				}
                board[i, col] = 0; // Backtracking; ya que, generaba conflicto, con otra reina en la inmediatamente siguiente o en otra subsiguiente columna
            }
        }
        return false;
    }

    public bool Solve() 
	{
        if (SolveNQueen(0)) 
		{
            Console.WriteLine("S");
            PrintSolution();
            return true;
        }
		else 
		{
            Console.WriteLine("N");
            return false;
        }
    }

    private void PrintSolution() 
	{
        for (int i = 0; i < size; i++) 
		{
            for (int j = 0; j < size; j++) 
			{
                Console.Write(board[i, j] == 1 ? "|Q" : "|_");
            }
            Console.WriteLine("|");
        }
    }
}
