using System;
using System.Diagnostics;

var process = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = "sudo",
        Arguments = "bash /home/vboxuser/VirtualJudge/nsjail-runner/run_single.sh /tmp/temp-workdir /tmp/temp-workdir/id456/in 6 dotnet \"/home/vboxuser/VirtualJudge/problems/8001/Validator8001/Validator.cs\"",
        UseShellExecute = false,  // No interactivo, no pide pass
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    }
};

process.Start();
string output = process.StandardOutput.ReadToEnd();  // Captura RUN/SUMMARY
string error = process.StandardError.ReadToEnd();    // Captura DETAILS/errores
process.WaitForExit(int.MaxValue);  // Espera fin (o timeout si necesitas)

Console.WriteLine("Output: " + output);
if (!string.IsNullOrEmpty(error)) Console.WriteLine("Error: " + error);

// Parsea output para build/run/DETAILS (e.g., split por ===)
int buildIndex = output.IndexOf("===BUILD===");
int runIndex = output.IndexOf("===RUN===");
int summaryIndex = output.IndexOf("===SUMMARY===");
// ... lógica para extraer
