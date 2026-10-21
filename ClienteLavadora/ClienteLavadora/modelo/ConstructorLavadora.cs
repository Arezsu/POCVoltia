using System.Collections.Generic;

namespace ClienteLavadora.modelo
{
    /// <summary>
    /// Patron BUILDER (constructor paso a paso) para LavadoraData.
    ///
    /// PROBLEMA QUE RESUELVE:
    /// LavadoraData tiene doce campos. Si se creara con un solo constructor del tipo
    /// new LavadoraData(101, "LG", 1450000, "...", 18, 1400, true, 45) nadie sabria
    /// que significa cada numero al leer el codigo, y seria facil equivocarse de orden.
    ///
    /// SOLUCION:
    /// cada metodo pone UN dato y devuelve el mismo constructor, entonces se pueden
    /// encadenar y se lee como una frase:
    ///
    ///     LavadoraData lavadora = new ConstructorLavadora()
    ///         .ConCodigo(101)
    ///         .ConMarca("LG")
    ///         .ConPrecioBase(1450000)
    ///         .ConCapacidadCarga(18)
    ///         .ConInverter(true)
    ///         .Construir();
    ///
    /// Se usa en dos lugares concretos de la aplicacion:
    ///  1. ControladorLavadoras.VistaPrevia: arma una lavadora "de mentira" para que
    ///     el formulario muestre el precio final calculado antes de guardar.
    ///  2. Cuando hay que copiar o completar datos localmente sin volver al servidor.
    /// </summary>
    public class ConstructorLavadora
    {
        // El objeto que se va llenando. Se crea una vez y se devuelve al final.
        private readonly LavadoraData lavadora = new LavadoraData();

        public ConstructorLavadora()
        {
            lavadora.Mantenimientos = new List<MantenimientoData>();
        }

        public ConstructorLavadora ConCodigo(int codigo)
        {
            lavadora.Codigo = codigo;
            return this;
        }

        public ConstructorLavadora ConMarca(string marca)
        {
            lavadora.Marca = marca;
            return this;
        }

        public ConstructorLavadora ConPrecioBase(double precioBase)
        {
            lavadora.PrecioBase = precioBase;
            return this;
        }

        /// <summary>La fecha se recibe como texto en el formato del servidor.</summary>
        public ConstructorLavadora ConFechaFabricacion(string fechaFabricacion)
        {
            lavadora.FechaFabricacion = fechaFabricacion;
            return this;
        }

        public ConstructorLavadora ConCapacidadCarga(double kilos)
        {
            lavadora.CapacidadCargaKilos = kilos;
            return this;
        }

        public ConstructorLavadora ConVelocidadCentrifugado(int rpm)
        {
            lavadora.VelocidadCentrifugadoRPM = rpm;
            return this;
        }

        public ConstructorLavadora ConInverter(bool tecnologiaInverter)
        {
            lavadora.TecnologiaInverter = tecnologiaInverter;
            return this;
        }

        public ConstructorLavadora ConMinutosLavado(int minutos)
        {
            lavadora.MinutosLavado = minutos;
            return this;
        }

        /// <summary>Agrega UN mantenimiento a la lista de detalle.</summary>
        public ConstructorLavadora ConMantenimiento(MantenimientoData mantenimiento)
        {
            if (mantenimiento != null)
            {
                lavadora.Mantenimientos.Add(mantenimiento);
            }
            return this;
        }

        /// <summary>Reemplaza toda la lista de mantenimientos.</summary>
        public ConstructorLavadora ConMantenimientos(List<MantenimientoData> mantenimientos)
        {
            if (mantenimientos != null)
            {
                lavadora.Mantenimientos = new List<MantenimientoData>(mantenimientos);
            }
            return this;
        }

        /// <summary>Copia los datos de una lavadora que ya existe (util para editar).</summary>
        public ConstructorLavadora Desde(LavadoraData original)
        {
            if (original == null)
            {
                return this;
            }
            lavadora.Codigo = original.Codigo;
            lavadora.Marca = original.Marca;
            lavadora.PrecioBase = original.PrecioBase;
            lavadora.FechaFabricacion = original.FechaFabricacion;
            lavadora.CapacidadCargaKilos = original.CapacidadCargaKilos;
            lavadora.VelocidadCentrifugadoRPM = original.VelocidadCentrifugadoRPM;
            lavadora.TecnologiaInverter = original.TecnologiaInverter;
            lavadora.MinutosLavado = original.MinutosLavado;
            lavadora.PrecioFinal = original.PrecioFinal;
            lavadora.Mantenimientos = new List<MantenimientoData>(original.ListaMantenimientos);
            return this;
        }

        /// <summary>
        /// Termina la construccion y devuelve la lavadora lista.
        /// Aqui se completan los campos que normalmente calcula el servidor
        /// (precio final, cantidad de mantenimientos, pendientes y costo total):
        /// asi la vista previa muestra los mismos numeros que va a devolver el servidor.
        /// </summary>
        public LavadoraData Construir()
        {
            lavadora.PrecioFinal = LavadoraData.CalcularPrecioFinal(
                lavadora.PrecioBase, lavadora.CapacidadCargaKilos, lavadora.TecnologiaInverter);

            int cantidad = 0;
            int pendientes = 0;
            double costo = 0;
            foreach (MantenimientoData mantenimiento in lavadora.ListaMantenimientos)
            {
                cantidad++;
                costo += mantenimiento.Costo;
                if (!mantenimiento.Completado)
                {
                    pendientes++;
                }
            }

            lavadora.CantidadMantenimientos = cantidad;
            lavadora.MantenimientosPendientes = pendientes;
            lavadora.CostoMantenimientos = costo;
            return lavadora;
        }
    }
}
