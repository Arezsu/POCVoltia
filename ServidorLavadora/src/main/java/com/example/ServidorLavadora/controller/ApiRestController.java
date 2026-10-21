package com.example.ServidorLavadora.controller;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.LavadoraInput;
import com.example.ServidorLavadora.model.LavadoraUpdateInput;
import com.example.ServidorLavadora.model.Mantenimiento;
import com.example.ServidorLavadora.model.MantenimientoInput;
import com.example.ServidorLavadora.model.MantenimientoUpdateInput;
import com.example.ServidorLavadora.servicios.ServicioLavadora;
import com.example.ServidorLavadora.servicios.ServicioMantenimiento;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.CrossOrigin;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

/** API REST paralela a GraphQL; ambas delegan en los mismos servicios de negocio. */
@RestController
@RequestMapping("/api")
@CrossOrigin(origins = "*")
public class ApiRestController {

    private final ServicioLavadora lavadoras;
    private final ServicioMantenimiento mantenimientos;

    public ApiRestController(ServicioLavadora lavadoras, ServicioMantenimiento mantenimientos) {
        this.lavadoras = lavadoras;
        this.mantenimientos = mantenimientos;
    }

    @GetMapping("/lavadoras")
    public List<Lavadora> listarLavadoras(
            @RequestParam(required = false) String marca,
            @RequestParam(required = false) Boolean tecnologiaInverter,
            @RequestParam(required = false) Boolean conMantenimientos) {
        return lavadoras.listarPorFiltro(marca, tecnologiaInverter, conMantenimientos);
    }

    @GetMapping("/lavadoras/buscar")
    public ResponseEntity<Lavadora> buscarLavadora(
            @RequestParam(required = false) Integer codigo,
            @RequestParam(required = false) String marca) {
        return lavadoras.buscarPorCriterios(codigo, marca)
                .map(ResponseEntity::ok).orElseGet(() -> ResponseEntity.notFound().build());
    }

    @GetMapping("/lavadoras/{codigo}")
    public ResponseEntity<Lavadora> lavadoraPorCodigo(@PathVariable int codigo) {
        return lavadoras.buscarPorCodigo(codigo)
                .map(ResponseEntity::ok).orElseGet(() -> ResponseEntity.notFound().build());
    }

    @PostMapping("/lavadoras")
    public ResponseEntity<Lavadora> agregarLavadora(@RequestBody LavadoraInput entrada) {
        return ResponseEntity.status(HttpStatus.CREATED).body(lavadoras.agregar(entrada));
    }

    @PutMapping("/lavadoras/{codigo}")
    public Lavadora actualizarLavadora(@PathVariable int codigo,
                                       @RequestBody LavadoraUpdateInput entrada) {
        return lavadoras.actualizar(codigo, entrada);
    }

    @DeleteMapping("/lavadoras/{codigo}")
    public ResponseEntity<Void> eliminarLavadora(@PathVariable int codigo) {
        return lavadoras.eliminar(codigo)
                ? ResponseEntity.noContent().build() : ResponseEntity.notFound().build();
    }

    @GetMapping("/lavadoras/{codigo}/mantenimientos")
    public List<Mantenimiento> listarMantenimientos(
            @PathVariable int codigo,
            @RequestParam(required = false) String tecnico,
            @RequestParam(required = false) Boolean completado) {
        return mantenimientos.listarPorFiltro(codigo, tecnico, completado);
    }

    @GetMapping("/lavadoras/{codigo}/mantenimientos/{id}")
    public ResponseEntity<Mantenimiento> mantenimientoPorId(@PathVariable int codigo,
                                                             @PathVariable int id) {
        return mantenimientos.buscarPorId(codigo, id)
                .map(ResponseEntity::ok).orElseGet(() -> ResponseEntity.notFound().build());
    }

    @PostMapping("/lavadoras/{codigo}/mantenimientos")
    public ResponseEntity<Mantenimiento> agregarMantenimiento(@PathVariable int codigo,
                                                               @RequestBody MantenimientoInput entrada) {
        return ResponseEntity.status(HttpStatus.CREATED).body(mantenimientos.agregar(codigo, entrada));
    }

    @PutMapping("/lavadoras/{codigo}/mantenimientos/{id}")
    public Mantenimiento actualizarMantenimiento(@PathVariable int codigo, @PathVariable int id,
                                                  @RequestBody MantenimientoUpdateInput entrada) {
        return mantenimientos.actualizar(codigo, id, entrada);
    }

    @DeleteMapping("/lavadoras/{codigo}/mantenimientos/{id}")
    public ResponseEntity<Void> eliminarMantenimiento(@PathVariable int codigo, @PathVariable int id) {
        return mantenimientos.eliminar(codigo, id)
                ? ResponseEntity.noContent().build() : ResponseEntity.notFound().build();
    }

    @GetMapping("/version")
    public int versionDatos() {
        return lavadoras.version();
    }
}
