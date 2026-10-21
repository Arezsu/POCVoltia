using System.Collections.Generic;
using System.Threading.Tasks;
using ClienteLavadora.modelo;

namespace ClienteLavadora.servicios
{
    /// <summary>
    /// Contrato de los servicios de LAVADORA (el objeto MAESTRO).
    ///
    /// SEGREGACION DE INTERFACES (la "I" de SOLID):
    /// las operaciones de la lavadora estan en esta interfaz y las de mantenimiento en
    /// IServicioMantenimientos. Ninguna clase esta obligada a implementar metodos que
    /// no usa: ServicioLavadoras no sabe nada de mantenimientos y al contrario.
    ///
    /// INVERSION DE DEPENDENCIAS (la "D" de SOLID):
    /// los controladores piden esta INTERFAZ, no la clase concreta. Eso permite
    /// cambiar la implementacion (por ejemplo, una que lea de un archivo para hacer
    /// pruebas) sin tocar ni el controlador ni las ventanas.
    ///
    /// Todas las operaciones son async porque viajan por la red.
    /// </summary>
    public interface IServicioLavadoras
    {
        /// <summary>
        /// Lista lavadoras con los filtros que aplica el SERVIDOR.
        /// Un filtro en null significa "todos".
        ///  - marca: texto que debe aparecer en la marca
        ///  - tecnologiaInverter: true = con inverter, false = sin inverter, null = todos
        ///  - conMantenimientos: true = solo las que tienen mantenimientos,
        ///                       false = solo las que no tienen, null = todas
        /// </summary>
        Task<List<LavadoraData>> ListarAsync(string marca, bool? tecnologiaInverter, bool? conMantenimientos);

        /// <summary>Busca UNA lavadora por codigo y/o marca. Devuelve null si no existe.</summary>
        Task<LavadoraData> BuscarPorCriteriosAsync(int? codigo, string marca);

        /// <summary>Crea una lavadora nueva y devuelve la que quedo guardada en el servidor.</summary>
        Task<LavadoraData> AgregarAsync(LavadoraInput input);

        /// <summary>Actualiza una lavadora existente. Los campos en null no se cambian.</summary>
        Task<LavadoraData> ActualizarAsync(int codigo, LavadoraUpdateInput input);

        /// <summary>Elimina una lavadora (con todos sus mantenimientos). Devuelve true si la borro.</summary>
        Task<bool> EliminarAsync(int codigo);

        /// <summary>
        /// Numero que sube cada vez que CUALQUIER cliente cambia algo en el servidor.
        /// El VigilanteServidor lo consulta cada 3 segundos para detectar cambios ajenos.
        /// </summary>
        Task<int> VersionDatosAsync();
    }
}
