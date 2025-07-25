using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SalahStreakApp.Data;

namespace SalahStreakApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var models = typeof(ApplicationDbContext)
                .GetProperties()
                .Where(p => p.PropertyType.Name.Contains("DbSet"))
                .Select(p => p.PropertyType.GenericTypeArguments[0].Name);

            foreach (var model in models)
            {
                Process.Start("dotnet", 
                    $"aspnet-codegenerator controller -name {model}Controller " +
                    $"-m {model} -dc ApplicationDbContext " +
                    $"--relativeFolderPath Controllers").WaitForExit();
                Console.WriteLine($"Generated {model}Controller");
            }
        }
    }
}