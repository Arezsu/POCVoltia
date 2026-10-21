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
 *
 * Cada metodo se une con una operacion del archivo schema.graphqls por el NOMBRE:
 * el metodo y la operacion se llaman igual. El controlador no hace logica de
 * negocio, solo recibe los argumentos y delega en ServicioLavadora.
 */
@Controller // Spring lo registra como controlador de GraphQL
public class LavadoraController {

    private final ServicioLavadora servicio;

    public LavadoraController(ServicioLavadora servicio) {
        this.servicio = servicio;
    }

    // ================= Queries (solo leen datos) =================

    /** Todas las lavadoras. */
    @QueryMapping
    public List<Lavadora> lavadoras() {
        return servicio.listar();
    }

    /** Una lavadora por codigo. */
    @QueryMapping
    public Optional<Lavadora> lavadoraPorCodigo(@Argument int codigo) {
        return servicio.buscarPorCodigo(codigo);
    }

    /** Una lavadora por uno o dos criterios: codigo y/o marca. */
    @QueryMapping
    public Optional<Lavadora> lavadoraPorCriterios(@Argument Integer codigo, @Argument String marca) {
        return servicio.buscarPorCriterios(codigo, marca);
    }

    /** Listado con filtros aplicados en el servidor. */
    @QueryMapping
    public List<Lavadora> lavadorasPorFiltro(@Argument String marca,
                                             @Argument Boolean tecnologiaInverter,
                                             @Argument Boolean conMantenimientos) {
        return servicio.listarPorFiltro(marca, tecnologiaInverter, conMantenimientos);
    }

    /** Version de los datos: cambia cuando cualquier cliente modifica algo. */
    @QueryMapping
    public int versionDatos() {
        return servicio.version();
    }

    // ================= Mutations (cambian datos) =================

    /** Crea una lavadora. */
    @MutationMapping
    public Lavadora addLavadora(@Argument("input") LavadoraInput input) {
        return servicio.agregar(input);
    }

    /** Actualiza una lavadora que primero se busca. */
    @MutationMapping
    public Lavadora actualizarLavadora(@Argument int codigo,
                                       @Argument("input") LavadoraUpdateInput input) {
        return servicio.actualizar(codigo, input);
    }

    /** Elimina una lavadora que primero se busca. */
    @MutationMapping
    public boolean eliminarLavadora(@Argument int codigo) {
        return servicio.eliminar(codigo);
    }

    // ================= Campos con formato propio =================

    /**
     * Entrega la fecha siempre como texto yyyy-MM-ddTHH:mm:ss (con segundos).
     * GraphQL no tiene un tipo de dato "fecha" propio, por eso se manda como String.
     */
    @SchemaMapping(typeName = "Lavadora", field = "fechaFabricacion")
    public String fechaFabricacion(Lavadora lavadora) {
        return lavadora.getFechaFabricacion().format(Validaciones.FORMATO_FECHA);
    }
}
