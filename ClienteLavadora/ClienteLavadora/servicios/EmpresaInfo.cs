namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Datos generales de la aplicacion (nombre, version e integrantes).
    /// Es un Singleton, igual que EmpresaInfo del proyecto en Java:
    /// solo existe UNA instancia y todas las ventanas usan esa misma.
    /// La usa la ventana "Acerca de...".
    /// </summary>
    public sealed class EmpresaInfo
    {
        // La unica instancia (privada, nadie mas puede crearla)
        private static EmpresaInfo instancia;
        private static readonly object candado = new object();

        public string NombreApp { get; private set; }
        public string Version { get; private set; }
        public string Fecha { get; private set; }
        public string Desarrolladores { get; private set; }

        // Constructor privado: no se puede hacer "new EmpresaInfo()" desde afuera
        private EmpresaInfo()
        {
            NombreApp = "Sistema de Gestión de Electrodomésticos - Voltia";
            Version = "4.0.0 (cliente C# + servidor GraphQL)";
            Fecha = "Octubre 2026";
            Desarrolladores =
                "Juan Andres Bermeo Alvarez (2220241053)\n" +
                "Alejandro Sanchez Quimbayo (2220241077)\n" +
                "David Arredondo (2220241062)";
        }

        /// <summary>Punto de acceso a la unica instancia.</summary>
        public static EmpresaInfo Instancia
        {
            get
            {
                lock (candado) // Un hilo solamente
                {
                    if (instancia == null)
                    {
                        instancia = new EmpresaInfo();
                    }
                    return instancia;
                }
            }
        }
    }
}
