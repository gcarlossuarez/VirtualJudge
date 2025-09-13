<Query Kind="Program">
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
</Query>

class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Employee(int id, string name)
    {
        this.Id = id;
        this.Name = name;
    }

    public override string ToString()
    {
        return $"ID: {Id}, Name: {Name}";
    }
}

// Comparador personalizado para Employee
class EmployeeComparer : IComparer<Employee>
{
    public int Compare(Employee x, Employee y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentException("Ambos empleados no pueden ser nulos");
        }

        return x.Id.CompareTo(y.Id);
    }
}

class Program
{
    static void Main()
    {
        // Crear un SortedSet con el comparador personalizado
        SortedSet<Employee> employees = new SortedSet<Employee>(new EmployeeComparer())
        {
            new Employee(3, "Alice"),
            new Employee(1, "Bob"),
            new Employee(2, "Charlie"),
            new Employee(4, "David")
        };

        // Mostrar los elementos del SortedSet
        Console.WriteLine("Empleados ordenados por ID:");
        foreach (var employee in employees)
        {
            Console.WriteLine(employee);
        }

        // Intentar añadir un empleado con ID duplicado
        Employee newEmployee = new Employee(2, "Eve");
        bool added = employees.Add(newEmployee);

        Console.WriteLine("\nIntentando añadir un empleado con ID duplicado (ID 2, Eve):");
        Console.WriteLine(added ? "Empleado añadido." : "Empleado no añadido, ID duplicado.");

        // Mostrar nuevamente los elementos del SortedSet
        Console.WriteLine("\nEmpleados después de intentar añadir un duplicado:");
        foreach (var employee in employees)
        {
            Console.WriteLine(employee);
        }
    }
}
