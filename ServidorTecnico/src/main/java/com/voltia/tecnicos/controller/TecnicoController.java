package com.voltia.tecnicos.controller;

import com.voltia.tecnicos.model.Tecnico;
import com.voltia.tecnicos.model.TecnicoInput;
import com.voltia.tecnicos.servicios.ServicioTecnico;
import java.time.format.DateTimeFormatter;
import java.util.List;
import org.springframework.graphql.data.method.annotation.Argument;
import org.springframework.graphql.data.method.annotation.MutationMapping;
import org.springframework.graphql.data.method.annotation.QueryMapping;
import org.springframework.graphql.data.method.annotation.SchemaMapping;
import org.springframework.stereotype.Controller;

@Controller
public class TecnicoController {
    private static final DateTimeFormatter FECHA = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss");
    private final ServicioTecnico servicio;

    public TecnicoController(ServicioTecnico servicio) { this.servicio = servicio; }

    @QueryMapping
    public List<Tecnico> tecnicos(@Argument String texto, @Argument String especialidad, @Argument Boolean activo) {
        return servicio.listar(texto, especialidad, activo);
    }

    @QueryMapping
    public Tecnico tecnicoPorCodigo(@Argument int codigo) { return servicio.buscarSiExiste(codigo); }

    @MutationMapping
    public Tecnico addTecnico(@Argument("input") TecnicoInput input) { return servicio.crear(input); }

    @MutationMapping
    public Tecnico actualizarTecnico(@Argument int codigo, @Argument("input") TecnicoInput input) {
        return servicio.actualizar(codigo, input);
    }

    @MutationMapping
    public boolean eliminarTecnico(@Argument int codigo) {
        servicio.eliminar(codigo);
        return true;
    }

    @SchemaMapping(typeName = "Tecnico", field = "fechaIngreso")
    public String fechaIngreso(Tecnico tecnico) { return tecnico.getFechaIngreso().format(FECHA); }
}
