namespace ClienteLavadora.vista
{
    /// <summary>Vistas especializadas: cada ventana abre un solo caso de uso.</summary>
    public sealed class VentanaListarMantenimientos : VentanaMantenimientos
    {
        public VentanaListarMantenimientos(int codigoLavadora = 0) : base(codigoLavadora, "listar") { }
    }

    public sealed class VentanaAgregarMantenimiento : VentanaMantenimientos
    {
        public VentanaAgregarMantenimiento(int codigoLavadora = 0) : base(codigoLavadora, "agregar") { }
    }

    public sealed class VentanaConsultarMantenimiento : VentanaMantenimientos
    {
        public VentanaConsultarMantenimiento(int codigoLavadora = 0) : base(codigoLavadora, "consultar") { }
    }

    public sealed class VentanaActualizarMantenimiento : VentanaMantenimientos
    {
        public VentanaActualizarMantenimiento(int codigoLavadora = 0) : base(codigoLavadora, "actualizar") { }
    }

    public sealed class VentanaEliminarMantenimiento : VentanaMantenimientos
    {
        public VentanaEliminarMantenimiento(int codigoLavadora = 0) : base(codigoLavadora, "eliminar") { }
    }
}
