using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
 using U3DObjeto;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTK
{
    public static class Program
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "U cuadrada TresDe",
                // This is needed to run on macos
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var window = new UWindow(GameWindowSettings.Default, nativeWindowSettings))
            {

                window.Run();
            }
        }
    }
}