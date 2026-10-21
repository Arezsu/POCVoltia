using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;
using ClienteLavadora.servicios;
using ClienteLavadora.util;

namespace ClienteLavadora.controlador
{
    /// <summary>
    /// CONTROLADOR de los casos de uso de LAVADORA (el objeto maestro).
    ///
    /// QUE HACE UN CONTROLADOR EN MVC:
    /// recibe el "quiero listar", "quiero guardar" de la vista, decide QUE servicio
    /// llamar, revisa reglas simples (por ejemplo: no buscar sin criterios) y avisa a
    /// los demas cuando algo cambio. Es el intermediario: la VISTA nunca habla con el
    /// SERVICIO, siempre pasa por aqui.
    ///
    /// POR QUE RECIBE UNA INTERFAZ EN EL CONSTRUCTOR (Inversion de Dependencias):
    /// el controlador no decide cual servicio usar; se lo entregan hecho. Asi se puede
    /// probar con un servicio falso (que no use red) sin cambiar ni una linea.
    ///
    /// Ejemplo de uso desde una ventana:
    ///     ControladorLavadoras controlador = new ControladorLavadoras(new ServicioLavadoras());
    ///     List<LavadoraData> lista = await Task.Run(() => controlador.ListarAsync(null, null, null));
    /// </summary>
    public class ControladorLavadoras
    {
        private readonly IServicioLavadoras servicio;

        public ControladorLavadoras(IServicioLavadoras servicio)
        {
            if (servicio == null)
            {
                throw new ArgumentNullException("servicio");
            }
            this.servicio = servicio;
        }

        // ====================================================================
        //  Consultas
        // ====================================================================

        /// <summary>Lista lavadoras con los filtros que aplica el servidor (null = todos).</summary>
        public async Task<List<LavadoraData>> ListarAsync(string marca, bool? tecnologiaInverter, bool? conMantenimientos)
        {
            return await servicio.ListarAsync(marca, tecnologiaInverter, conMantenimientos);
        }

        /// <summary>
        /// Busca UNA lavadora por codigo y/o marca.
        /// Si no hay criterios o no existe, lanza ArgumentException con un mensaje
        /// listo para mostrar: la ventana solo tiene que atraparlo.
        /// </summary>
        public async Task<LavadoraData> BuscarAsync(int? codigo, string marca)
        {
            bool hayMarca = !string.IsNullOrWhiteSpace(marca);
            if (codigo == null && !hayMarca)
            {
                throw new ArgumentException("Escriba el codigo o la marca para poder buscar.");
            }

            LavadoraData lavadora = await servicio.BuscarPorCriteriosAsync(codigo, hayMarca ? marca.Trim() : null);
            if (lavadora == null)
            {
                throw new ArgumentException("No existe una lavadora con esos criterios.");
            }
            return lavadora;
        }

        /// <summary>Numero de version de los datos del servidor (lo usa el VigilanteServidor).</summary>
        public async Task<int> VersionDatosAsync()
        {
            return await servicio.VersionDatosAsync();
        }

        /// <summary>Devuelve true si el servidor responde. No lanza excepcion: es una prueba.</summary>
        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                await servicio.VersionDatosAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // ====================================================================
        //  Operaciones que cambian datos
        //  Despues de guardar se avisa con NotificadorDatos: asi la ventana Listar
        //  (y cualquier otra suscrita) se recarga sola.
        // ====================================================================

        public async Task<LavadoraData> AgregarAsync(LavadoraInput input)
        {
            LavadoraData creada = await servicio.AgregarAsync(input);
            NotificadorDatos.AvisarDatosCambiaron();
            return creada;
        }

        public async Task<LavadoraData> ActualizarAsync(int codigo, LavadoraUpdateInput input)
        {
            LavadoraData actualizada = await servicio.ActualizarAsync(codigo, input);
            NotificadorDatos.AvisarDatosCambiaron();
            return actualizada;
        }

        public async Task EliminarAsync(int codigo)
        {
            bool eliminada = await servicio.EliminarAsync(codigo);
            if (!eliminada)
            {
                throw new Exception("El servidor no pudo eliminar la lavadora " + codigo + ".");
            }
            NotificadorDatos.AvisarDatosCambiaron();
        }

        // ====================================================================
        //  Vista previa (sin red)
        //  Aqui se ve el patron BUILDER trabajando: se construye una lavadora
        //  "de papel" para mostrar el precio final antes de guardar de verdad.
        // ====================================================================

        public LavadoraData VistaPrevia(LavadoraInput input)
        {
            LavadoraData previa = new ConstructorLavadora()
                .ConCodigo(input.Codigo)
                .ConMarca(input.Marca)
                .ConPrecioBase(input.PrecioBase)
                .ConFechaFabricacion(input.FechaFabricacion)
                .ConCapacidadCarga(input.CapacidadCargaKilos)
                .ConVelocidadCentrifugado(input.VelocidadCentrifugadoRPM)
                .ConInverter(input.TecnologiaInverter)
                .ConMinutosLavado(input.MinutosLavado)
                .Construir();

            return previa;
        }
    }
}
