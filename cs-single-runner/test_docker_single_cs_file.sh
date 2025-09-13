cat > ~/icpc_test/solucion.cs <<'CS'
using System;
class A {
    static void Main() {
        var p = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Console.WriteLine(int.Parse(p[0]) + int.Parse(p[1]));
        Console.WriteLine("Ejecutado!!!");
    }
}
CS

printf "7 35\n" | docker run --rm \
  --network=none \
  --cpus=1 \
  --memory=1536m \
  --pids-limit=256 \
  -i \
  -v ~/icpc_test:/home/sandbox/in:ro \
  cs-single-runner:1 /home/sandbox/in 6

