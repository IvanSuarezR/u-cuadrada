using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using System;
using System.Threading;
using System.Collections.Concurrent;
using U3DObjeto;
class Program
{
    private static UWindow _window3D;
    private static BlockingCollection<Action> _colaAcciones = new BlockingCollection<Action>();

    [STAThread]
    static void Main()
    {
        // Configuración de la ventana 3D
        var nativeSettings = new NativeWindowSettings()
        {
            ClientSize = new Vector2i(800, 600),
            Title = "Visualizador 3D + Consola",
            APIVersion = new Version(3, 3)
        };

        _window3D = new UWindow(GameWindowSettings.Default, nativeSettings);

        // Hilo de consola
        var consoleThread = new Thread(ConsoleInputLoop)
        {
            IsBackground = true
        };
        consoleThread.Start();

        // Bucle principal para procesar acciones desde la consola
        _window3D.UpdateFrame += (args) =>
        {
            while (_colaAcciones.TryTake(out var action))
            {
                action.Invoke();
            }
        };

        _window3D.Run();
    }

    private static void ConsoleInputLoop()
    {
        Console.WriteLine("=== CONTROLES ===");
        Console.WriteLine("1 - Agregar objeto");
        Console.WriteLine("ESC - Salir");

        while (!_window3D.IsExiting)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.D1)
                {
                    _colaAcciones.Add(() => AgregarObjeto());
                    Console.WriteLine("Objeto agregado");
                }
                else if (key == ConsoleKey.Escape)
                {
                    _colaAcciones.Add(() => _window3D.Close());
                }
            }
            Thread.Sleep(50);
        }
    }

    private static void AgregarObjeto()
    {
        // var (vertices, indices) = ObjLoader.LoadObj("letraU.obj", 0.1f);
        // var random = new Random();
        // var nuevoObj = new Objeto3D(
        //     position: new Vector3(0, 0, -3),
        //     CentroM: Vector3.Zero,
        //     vertices: vertices,
        //     indices: indices,
        //     color: new Vector3(
        //         (float)random.NextDouble(),
        //         (float)random.NextDouble(),
        //         (float)random.NextDouble())
        // );
        _window3D.CrearObjetoEnCamara();
    }
}