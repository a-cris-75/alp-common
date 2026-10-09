using Alp.Com.VisionSystem.ATProfilerHalcon;
using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Startup!");

        var gestore = new Gestore();

        var result = await gestore.StartUp();
        Console.WriteLine($"Startup result: {result}");

        var prova = await gestore.Execute();

        foreach (var item in prova)
        {
            Console.WriteLine($"{item.Output1}, {item.Output2}, {item.Romboidita}");
        }
    }
}