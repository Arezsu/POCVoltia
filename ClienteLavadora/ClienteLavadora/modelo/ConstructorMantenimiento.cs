namespace ClienteLavadora.modelo
{
    /// <summary>
    /// Patron BUILDER para MantenimientoData (el objeto DETALLE).
    ///
    /// Es el hermano pequeno de ConstructorLavadora y se usa igual: metodos cortos
    /// encadenados y al final .Construir().
    ///
    ///     MantenimientoData mantenimiento = new ConstructorMantenimiento()
    ///         .ConDescripcion("Cambio de bomba")
    ///         .ConTecnico("Carlos Perez")
    ///         .ConCosto(185000)
    ///         .ConFecha("2024-06-12T10:30:00")
    ///         .ConCompletado(true)
    ///         .Construir();
    ///
    /// Se usa sobre todo en la vista previa del formulario de mantenimientos:
    /// el usuario ve el resumen (descripcion, tecnico, costo y estado) antes de guardar.
    /// </summary>
    public class ConstructorMantenimiento
    {
        private readonly MantenimientoData mantenimiento = new MantenimientoData();

        public ConstructorMantenimiento ConId(int id)
        {
            mantenimiento.Id = id;
            return this;
        }

        public ConstructorMantenimiento ConDescripcion(string descripcion)
        {
            mantenimiento.Descripcion = descripcion;
            return this;
        }

        public ConstructorMantenimiento ConTecnico(string tecnico)
        {
            mantenimiento.Tecnico = tecnico;
            return this;
        }

        public ConstructorMantenimiento ConCosto(double costo)
        {
            mantenimiento.Costo = costo;
            return this;
        }

        /// <summary>La fecha se maneja como texto, en el formato que exige el servidor.</summary>
        public ConstructorMantenimiento ConFecha(string fecha)
        {
            mantenimiento.Fecha = fecha;
            return this;
        }

        public ConstructorMantenimiento ConCompletado(bool completado)
        {
            mantenimiento.Completado = completado;
            return this;
        }

        /// <summary>Copia los datos de un mantenimiento que ya existe (util para editar).</summary>
        public ConstructorMantenimiento Desde(MantenimientoData original)
        {
            if (original == null)
            {
                return this;
            }
            mantenimiento.Id = original.Id;
            mantenimiento.Descripcion = original.Descripcion;
            mantenimiento.Tecnico = original.Tecnico;
            mantenimiento.Costo = original.Costo;
            mantenimiento.Fecha = original.Fecha;
            mantenimiento.Completado = original.Completado;
            return this;
        }

        /// <summary>Devuelve el mantenimiento armado.</summary>
        public MantenimientoData Construir()
        {
            return mantenimiento;
        }
    }
}
