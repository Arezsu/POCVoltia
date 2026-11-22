package com.voltia.tecnicos.controller;

import com.voltia.tecnicos.model.Tecnico;
import com.voltia.tecnicos.model.TecnicoInput;
import com.voltia.tecnicos.servicios.ServicioTecnico;
import java.util.List;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/tecnicos")
public class TecnicoController {
    private final ServicioTecnico servicio;
    public TecnicoController(ServicioTecnico servicio) { this.servicio = servicio; }

    @GetMapping
    public List<Tecnico> listar(@RequestParam(required = false) String texto,
                                @RequestParam(required = false) String especialidad,
                                @RequestParam(required = false) Boolean activo) {
        return servicio.listar(texto, especialidad, activo);
    }
    @GetMapping("/{codigo}") public Tecnico buscar(@PathVariable int codigo) { return servicio.buscar(codigo); }
    @PostMapping @ResponseStatus(HttpStatus.CREATED)
    public Tecnico crear(@RequestBody TecnicoInput input) { return servicio.crear(input); }
    @PutMapping("/{codigo}")
    public Tecnico actualizar(@PathVariable int codigo, @RequestBody TecnicoInput input) { return servicio.actualizar(codigo, input); }
    @DeleteMapping("/{codigo}") @ResponseStatus(HttpStatus.NO_CONTENT)
    public void eliminar(@PathVariable int codigo) { servicio.eliminar(codigo); }
}
