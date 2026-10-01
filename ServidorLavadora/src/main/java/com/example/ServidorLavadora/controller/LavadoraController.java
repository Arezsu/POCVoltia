package com.example.ServidorLavadora.controller;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.LavadoraInput;
import com.example.ServidorLavadora.model.LavadoraUpdateInput;
import com.example.ServidorLavadora.servicios.ServicioLavadora;
import org.springframework.graphql.data.method.annotation.Argument;
import org.springframework.graphql.data.method.annotation.MutationMapping;
import org.springframework.graphql.data.method.annotation.QueryMapping;
import org.springframework.graphql.data.method.annotation.SchemaMapping;
import org.springframework.stereotype.Controller;

import java.util.List;
import java.util.Optional;

@Controller
public class LavadoraController {

    // ---------------- Queries ----------------

    @QueryMapping
    public List<Lavadora> lavadoras() {
        return ServicioLavadora.listarLavadoras();
    }

    @QueryMapping
    public Optional<Lavadora> lavadoraPorCodigo(@Argument int codigo) {
        return ServicioLavadora.buscarPorCodigo(codigo);
    }

    @QueryMapping
    public List<Lavadora> lavadorasPorFiltro(@Argument String marca, @Argument Boolean funcionSecado) {
        return ServicioLavadora.listarPorFiltro(marca, funcionSecado);
    }

    // ---------------- Mutations ----------------

    @MutationMapping
    public Optional<Lavadora> addLavadora(@Argument("input") LavadoraInput input) {
        return ServicioLavadora.addLavadora(input);
    }

    @MutationMapping
    public Optional<Lavadora> actualizarLavadora(@Argument int codigo,
                                                 @Argument("input") LavadoraUpdateInput input) {
        return ServicioLavadora.actualizarLavadora(codigo, input);
    }

    @MutationMapping
    public boolean eliminarLavadora(@Argument int codigo) {
        return ServicioLavadora.eliminarLavadora(codigo);
    }

    // ---------------- Campos calculados ----------------

    /** Entrega la fecha siempre como yyyy-MM-ddTHH:mm:ss (con segundos). */
    @SchemaMapping(typeName = "Lavadora", field = "fechaFabricacion")
    public String fechaFabricacion(Lavadora lavadora) {
        return lavadora.getFechaFabricacion().format(ServicioLavadora.FORMATO_FECHA);
    }
}
