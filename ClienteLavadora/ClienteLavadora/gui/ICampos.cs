using System.Collections.Generic;
using System.Windows;
using ClienteLavadora.model;

namespace ClienteLavadora.gui
{
    /// <summary>
    /// Contrato de los formularios (CamposLavadora y CamposPrograma).
    /// Las ventanas de Adicionar, Consultar, Actualizar y Eliminar solo conocen esta
    /// interface, por eso son las mismas ventanas para los dos objetos
    /// (principio de inversion de dependencias).
    /// </summary>
    public interface ICampos
    {
        /// <summary>El control que se pone dentro de la ventana.</summary>
        UIElement Vista { get; }

        /// <summary>Las filas del formulario (para animarlas una tras otra).</summary>
        IEnumerable<UIElement> Filas { get; }

        /// <summary>Todo editable y sin datos calculados.</summary>
        void ConfigurarParaAgregar();

        /// <summary>Datos calculados visibles y todo bloqueado hasta que se busque algo.</summary>
        void ConfigurarParaBuscar();

        /// <summary>Bloquea o desbloquea los campos que se pueden cambiar (el codigo nunca cambia).</summary>
        void PermitirEdicion(bool permitir);

        void Limpiar();

        /// <summary>Muestra TODA la informacion del objeto en el formulario.</summary>
        void Mostrar(DatoBase dato);

        /// <summary>Datos para CREAR (ProgramaInput o LavadoraInput). Si algo esta mal lanza un error con mensaje claro.</summary>
        object ObtenerInput();

        /// <summary>Datos para ACTUALIZAR (ProgramaUpdateInput o LavadoraUpdateInput).</summary>
        object ObtenerUpdateInput();
    }
}
