package com.example.ServidorLavadora.controller;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.LavadoraInput;
import com.example.ServidorLavadora.model.LavadoraUpdateInput;
import com.example.ServidorLavadora.servicios.ServicioLavadora;
import com.example.ServidorLavadora.util.Validaciones;
import org.springframework.graphql.data.method.annotation.Argument;
import org.springframework.graphql.data.method.annotation.MutationMapping;
import org.springframework.graphql.data.method.annotation.QueryMapping;
import org.springframework.graphql.data.method.annotation.SchemaMapping;
import org.springframework.stereotype.Controller;

import java.util.List;
import java.util.Optional;

/**
 * CONTROLADOR (la C de MVC) de las lavadoras. Recibe las peticiones GraphQL.
 * Cada metodo se une con una operacion del archivo schema.graphqls por el NOMBRE
 * (el metodo y la operacion se llaman igual). No hace la logica: le pasa el trabajo
 * a ServicioLavadora, que recibe por el constructor.
 */
@Controller // Recibe peticiones GraphQL
public class LavadoraController {

    private final ServicioLavadora servicio;

    public LavadoraController(ServicioLavadora servicio) {
        this.servicio = servicio;
    }

    // ---------------- Queries (solo leen datos) ----------------

    @QueryMapping
    public List<Lavadora> lavadoras() {
        return servicio.listar();
    }

    @QueryMapping
    public Optional<Lavadora> lavadoraPorCodigo(@Argument int codigo) {
        return servicio.buscarPorCodigo(codigo);
    }

    @QueryMapping
    public Optional<Lavadora> lavadoraPorCriterios(@Argument Integer codigo, @Argument String marca) {
        return servicio.buscarPorCriterios(codigo, marca);
    }

    @QueryMapping
    public List<Lavadora> lavadorasPorFiltro(@Argument String marca, @Argument Boolean tecnologiaInverter) {
        return servicio.listarPorFiltro(marca, tecnologiaInverter);
    }

    // ---------------- Mutations (cambian datos) ----------------

    @MutationMapping
    public Lavadora addLavadora(@Argument("input") LavadoraInput input) {
        return servicio.agregar(input);
    }

    @MutationMapping
    public Lavadora actualizarLavadora(@Argument int codigo, @Argument("input") LavadoraUpdateInput input) {
        return servicio.actualizar(codigo, input);
    }

    @MutationMapping
    public boolean eliminarLavadora(@Argument int codigo) {
        return servicio.eliminar(codigo);
    }

    // ---------------- Campos calculados ----------------

    /** Entrega la fecha siempre como yyyy-MM-ddTHH:mm:ss (con segundos). */
    @SchemaMapping(typeName = "Lavadora", field = "fechaFabricacion")
    public String fechaFabricacion(Lavadora lavadora) {
        return lavadora.getFechaFabricacion().format(Validaciones.FORMATO_FECHA);
    }
}
