using System;

namespace PortMonitor.Installer
{
    /// <summary>
    /// Proyecto de instalador - Para usar con WiX Toolset
    /// Este proyecto no contiene código ejecutable, solo define la estructura del instalador
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Este proyecto es para generar el instalador MSI con WiX Toolset.");
            Console.WriteLine("Use 'dotnet build' para compilar y generar el instalador.");
        }
    }
}
