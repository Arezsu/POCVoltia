package com.example.ServidorLavadora.controller;

import com.example.ServidorLavadora.model.ProgramaInput;
import com.example.ServidorLavadora.model.ProgramaLavado;
import com.example.ServidorLavadora.model.ProgramaUpdateInput;
import com.example.ServidorLavadora.servicios.ServicioProgramaLavado;
import com.example.ServidorLavadora.util.Validaciones;
import org.springframework.graphql.data.method.annotation.Argument;
import org.springframework.graphql.data.method.annotation.MutationMapping;
import org.springframework.graphql.data.method.annotation.QueryMapping;
import org.springframework.graphql.data.method.annotation.SchemaMapping;
import org.springframework.stereotype.Controller;

import java.util.List;
import java.util.Optional;

/**
 * CONTROLADOR (la C de MVC) de los programas de lavado. Funciona igual que LavadoraController.
 */
@Controller // Recibe peticiones GraphQL
public class ProgramaLavadoController {

    private final ServicioProgramaLavado servicio;

    public ProgramaLavadoController(ServicioProgramaLavado servicio) {
        this.servicio = servicio;
    }

    // ---------------- Queries (solo leen datos) ----------------

    @QueryMapping
    public List<ProgramaLavado> programas() {
        return servicio.listar();
    }

    @QueryMapping
    public Optional<ProgramaLavado> programaPorCodigo(@Argument int codigo) {
        return servicio.buscarPorCodigo(codigo);
    }

    @QueryMapping
    public Optional<ProgramaLavado> programaPorCriterios(@Argument Integer codigo, @Argument String nombre) {
        return servicio.buscarPorCriterios(codigo, nombre);
    }

    @QueryMapping
    public List<ProgramaLavado> programasPorFiltro(@Argument String nombre, @Argument Boolean aguaCaliente) {
        return servicio.listarPorFiltro(nombre, aguaCaliente);
    }

    // ---------------- Mutations (cambian datos) ----------------

    @MutationMapping
    public ProgramaLavado addPrograma(@Argument("input") ProgramaInput input) {
        return servicio.agregar(input);
    }

    @MutationMapping
    public ProgramaLavado actualizarPrograma(@Argument int codigo, @Argument("input") ProgramaUpdateInput input) {
        return servicio.actualizar(codigo, input);
    }

    @MutationMapping
    public boolean eliminarPrograma(@Argument int codigo) {
        return servicio.eliminar(codigo);
    }

    // ---------------- Campos calculados ----------------

    /** Entrega la fecha siempre como yyyy-MM-ddTHH:mm:ss (con segundos). */
    @SchemaMapping(typeName = "ProgramaLavado", field = "fechaCreacion")
    public String fechaCreacion(ProgramaLavado programa) {
        return programa.getFechaCreacion().format(Validaciones.FORMATO_FECHA);
    }

    /** Cuantas lavadoras usan este programa (la parte 0..* de la relacion maestro-detalle). */
    @SchemaMapping(typeName = "ProgramaLavado", field = "cantidadLavadoras")
    public int cantidadLavadoras(ProgramaLavado programa) {
        return (int) servicio.contarLavadoras(programa.getCodigo());
    }
}
