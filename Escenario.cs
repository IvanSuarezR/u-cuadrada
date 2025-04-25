
namespace U3DObjeto
{
    public class Escenario
    {
        private List<Objeto3D> objetos;

        public Escenario()
        {
            objetos = new List<Objeto3D>();
        }

        // Agrega un objeto al escenario
        public void AddObjeto(Objeto3D obj)
        {
            obj.Initialize(); // Asegura que esté listo para ser dibujado
            objetos.Add(obj);
        }

        // Dibuja todos los objetos del escenario
        public void Draw()
        {
            foreach (var obj in objetos)
            {
                obj.Draw();
            }
        }

        // Limpia los recursos de GPU de todos los objetos
        public void Cleanup()
        {
            foreach (var obj in objetos)
            {
                obj.Cleanup();
            }
        }

        // Retorna la lista de objetos (por si necesitas acceder externamente)
        public List<Objeto3D> GetObjetos()
        {
            return objetos;
        }

        // Reemplaza todos los objetos actuales (por ejemplo, al cargar desde JSON)
        public void SetObjetos(List<Objeto3D> nuevosObjetos)
        {
            Cleanup(); // Limpia los anteriores
            objetos = nuevosObjetos;
            if (objetos == null) return; // Si no hay objetos, no hay nada que inicializar
            foreach (var obj in objetos)
                obj.Initialize(); // Asegura que los nuevos estén listos para dibujarse
        }

        public void Guardar()
        {
            JsonEscenario.Guardar(objetos);
        }

        // Carga el estado del escenario desde JSON
        public void Cargar()
        {
            var nuevosObjetos = JsonEscenario.Cargar();
            SetObjetos(nuevosObjetos);
        }
    }
}
