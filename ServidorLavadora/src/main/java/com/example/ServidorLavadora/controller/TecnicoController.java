package com.example.ServidorLavadora.controller;

import com.example.ServidorLavadora.model.TecnicoDto;
import com.example.ServidorLavadora.model.TecnicoConLavadoraDto;
import com.example.ServidorLavadora.model.TecnicoInput;
import com.example.ServidorLavadora.servicios.ClienteTecnicos;
import com.example.ServidorLavadora.servicios.ServicioMantenimiento;
import com.example.ServidorLavadora.model.Lavadora;
import java.util.List;
import org.springframework.graphql.data.method.annotation.Argument;
import org.springframework.graphql.data.method.annotation.MutationMapping;
import org.springframework.graphql.data.method.annotation.QueryMapping;
import org.springframework.stereotype.Controller;

@Controller
public class TecnicoController {
    private final ClienteTecnicos cliente;
    private final ServicioMantenimiento mantenimientos;
    public TecnicoController(ClienteTecnicos cliente, ServicioMantenimiento mantenimientos) {
        this.cliente = cliente; this.mantenimientos = mantenimientos;
    }
    @QueryMapping public List<TecnicoDto> tecnicos(@Argument String texto, @Argument String especialidad, @Argument Boolean activo) {
        return cliente.listar(texto, especialidad, activo);
    }
    @QueryMapping public TecnicoDto tecnicoPorCodigo(@Argument int codigo) { return cliente.buscar(codigo); }
    @QueryMapping public List<TecnicoConLavadoraDto> tecnicosConLavadoras(@Argument String texto, @Argument String especialidad, @Argument Boolean activo) {
        return cliente.listar(texto, especialidad, activo).stream().flatMap(t -> {
            List<Lavadora> relacionadas = mantenimientos.lavadorasDeTecnico(t.codigo());
            if (relacionadas.isEmpty()) return java.util.stream.Stream.of(fila(t, null, null));
            return relacionadas.stream().map(l -> fila(t, l.getCodigo(), l.getMarca()));
        }).toList();
    }
    @MutationMapping public TecnicoDto addTecnico(@Argument("input") TecnicoInput input) {
        TecnicoDto creado = cliente.crear(input); mantenimientos.registrarCambio("Tecnico " + creado.codigo() + " agregado"); return creado;
    }
    @MutationMapping public TecnicoDto actualizarTecnico(@Argument int codigo, @Argument("input") TecnicoInput input) {
        TecnicoDto actualizado = cliente.actualizar(codigo, input);
        mantenimientos.actualizarNombreTecnico(codigo, actualizado.nombre());
        mantenimientos.registrarCambio("Tecnico " + codigo + " actualizado"); return actualizado;
    }
    @MutationMapping public boolean eliminarTecnico(@Argument int codigo) {
        if (mantenimientos.tieneTecnico(codigo)) throw new IllegalArgumentException("No se puede eliminar el tecnico porque tiene mantenimientos asociados.");
        cliente.eliminar(codigo); mantenimientos.registrarCambio("Tecnico " + codigo + " eliminado"); return true;
    }
    private TecnicoConLavadoraDto fila(TecnicoDto t, Integer codigoLavadora, String marcaLavadora) {
        return new TecnicoConLavadoraDto(t.codigo(), t.documento(), t.nombre(), t.especialidad(),
                t.telefono(), t.fechaIngreso(), t.activo(), codigoLavadora, marcaLavadora);
    }
}
