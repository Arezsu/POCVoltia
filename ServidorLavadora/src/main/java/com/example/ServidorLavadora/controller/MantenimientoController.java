package com.example.ServidorLavadora.controller;

import com.example.ServidorLavadora.model.Mantenimiento;
import com.example.ServidorLavadora.model.MantenimientoInput;
import com.example.ServidorLavadora.model.MantenimientoUpdateInput;
import com.example.ServidorLavadora.servicios.ServicioMantenimiento;
import com.example.ServidorLavadora.util.Validaciones;
import org.springframework.graphql.data.method.annotation.Argument;
import org.springframework.graphql.data.method.annotation.MutationMapping;
import org.springframework.graphql.data.method.annotation.QueryMapping;
import org.springframework.graphql.data.method.annotation.SchemaMapping;
import org.springframework.stereotype.Controller;

import java.util.List;
import java.util.Optional;

/**
 * CONTROLADOR (la C de MVC) de los mantenimientos, que son el DETALLE de la lavadora.
 *
 * Todas las operaciones piden el codigo de la lavadora porque un mantenimiento no
 * existe solo: siempre esta dentro de una lavadora.
 */
@Controller
public class MantenimientoController {

    private final ServicioMantenimiento servicio;

    public MantenimientoController(ServicioMantenimiento servicio) {
        this.servicio = servicio;
    }

    // ================= Queries (solo leen datos) =================

    /** Mantenimientos de una lavadora. Puede devolver una lista vacia (0..*). */
    @QueryMapping
    public List<Mantenimiento> mantenimientos(@Argument int codigoLavadora) {
        return servicio.listar(codigoLavadora);
    }

    /** Un mantenimiento por su id dentro de la lavadora. */
    @QueryMapping
    public Optional<Mantenimiento> mantenimientoPorId(@Argument int codigoLavadora, @Argument int id) {
        return servicio.buscarPorId(codigoLavadora, id);
    }

    /** Listado de mantenimientos con filtros aplicados en el servidor. */
    @QueryMapping
    public List<Mantenimiento> mantenimientosPorFiltro(@Argument int codigoLavadora,
                                                       @Argument String tecnico,
                                                       @Argument Boolean completado) {
        return servicio.listarPorFiltro(codigoLavadora, tecnico, completado);
    }

    // ================= Mutations (cambian datos) =================

    /** Agrega un mantenimiento a una lavadora que primero se busca. */
    @MutationMapping
    public Mantenimiento addMantenimiento(@Argument int codigoLavadora,
                                          @Argument("input") MantenimientoInput input) {
        return servicio.agregar(codigoLavadora, input);
    }

    /** Actualiza un mantenimiento que primero se busca. */
    @MutationMapping
    public Mantenimiento actualizarMantenimiento(@Argument int codigoLavadora,
                                                 @Argument int id,
                                                 @Argument("input") MantenimientoUpdateInput input) {
        return servicio.actualizar(codigoLavadora, id, input);
    }

    /** Elimina un mantenimiento que primero se busca. */
    @MutationMapping
    public boolean eliminarMantenimiento(@Argument int codigoLavadora, @Argument int id) {
        return servicio.eliminar(codigoLavadora, id);
    }

    // ================= Campos con formato propio =================

    /** La fecha del detalle tambien viaja como texto yyyy-MM-ddTHH:mm:ss. */
    @SchemaMapping(typeName = "Mantenimiento", field = "fecha")
    public String fecha(Mantenimiento mantenimiento) {
        return mantenimiento.getFecha().format(Validaciones.FORMATO_FECHA);
    }
}
