package com.example.ServidorLavadora.servicios;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.Mantenimiento;
import com.example.ServidorLavadora.model.MantenimientoInput;
import com.example.ServidorLavadora.model.MantenimientoUpdateInput;
import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.util.Validaciones;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.List;
import java.util.Optional;

/**
 * Logica de negocio del objeto DETALLE: el mantenimiento.
 *
 * Cada mantenimiento conserva la referencia a su lavadora, pero se consulta
 * globalmente por su id unico.
 *
 * Este servicio reutiliza ServicioLavadora para buscar al maestro y no repetir
 * el mismo codigo de busqueda en dos clases (principio de responsabilidad unica).
 */
@Service
public class ServicioMantenimiento {

    private final AlmacenDatos almacen;
    private final ServicioLavadora servicioLavadora;

    public ServicioMantenimiento(AlmacenDatos almacen, ServicioLavadora servicioLavadora) {
        this.almacen = almacen;
        this.servicioLavadora = servicioLavadora;
    }

    // ==================================================================
    //  Consultas (queries)
    // ==================================================================

    /** Lista los mantenimientos de una lavadora (puede ser una lista vacia). */
    public List<Mantenimiento> listar(int codigoLavadora) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = servicioLavadora.exigirPorCodigo(codigoLavadora);
            List<Mantenimiento> resultado = new ArrayList<>(lavadora.getMantenimientos());
            resultado.forEach(mantenimiento -> enriquecer(mantenimiento, lavadora));
            return resultado;
        }
    }

    /** Lista todos los mantenimientos sin exigir primero el código del maestro. */
    public List<Mantenimiento> listarTodos() {
        synchronized (almacen.candado()) {
            return almacen.lavadoras().stream()
                    .flatMap(lavadora -> lavadora.getMantenimientos().stream()
                            .map(mantenimiento -> {
                                enriquecer(mantenimiento, lavadora);
                                return mantenimiento;
                    }))
                    .sorted(Comparator.comparingInt(Mantenimiento::getCodigoLavadora)
                            .thenComparingInt(Mantenimiento::getId))
                    .toList();
        }
    }

    /** Busca un mantenimiento por su ID único. */
    public Optional<Mantenimiento> buscarPorId(int id) {
        synchronized (almacen.candado()) {
            return buscarEnTodas(id);
        }
    }

    /**
     * Filtra los mantenimientos de una lavadora EN EL SERVIDOR:
     *  - tecnico: texto que debe aparecer en el nombre del tecnico
     *  - completado: true = terminados, false = pendientes, null = todos
     */
    public List<Mantenimiento> listarPorFiltro(int codigoLavadora, String tecnico, Boolean completado) {
        return filtrar(listar(codigoLavadora), tecnico, completado);
    }

    public List<Mantenimiento> listarPorFiltro(Integer codigoLavadora, String tecnico, Boolean completado) {
        return filtrar(codigoLavadora == null ? listarTodos() : listar(codigoLavadora), tecnico, completado);
    }

    private List<Mantenimiento> filtrar(List<Mantenimiento> mantenimientos, String tecnico, Boolean completado) {
        String texto = (tecnico == null) ? "" : tecnico.trim().toLowerCase();
        return mantenimientos.stream()
                .filter(mantenimiento -> texto.isEmpty()
                        || mantenimiento.getTecnico().toLowerCase().contains(texto))
                .filter(mantenimiento -> completado == null
                        || mantenimiento.isCompletado() == completado)
                .toList();
    }

    // ==================================================================
    //  Crear (mutation addMantenimiento)
    // ==================================================================

    /**
     * Agrega un mantenimiento con id elegido por el usuario a una lavadora existente.
     */
    public Mantenimiento agregar(MantenimientoInput input) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = servicioLavadora.exigirPorCodigo(input.codigoLavadora());
            if (input.id() < 1) throw new IllegalArgumentException("El ID debe ser mayor que cero.");
            if (buscarEnTodas(input.id()).isPresent())
                throw new IllegalArgumentException("Ya existe un mantenimiento con el ID " + input.id());

            validarDescripcionYTecnico(input.descripcion(), input.tecnico());
            Validaciones.noNegativo(input.costo(), "El costo del mantenimiento");
            LocalDateTime fecha = Validaciones.fecha(input.fecha());

            Mantenimiento mantenimiento = Mantenimiento.builder()
                    .id(input.id())
                    .descripcion(input.descripcion().trim())
                    .tecnico(input.tecnico().trim())
                    .costo(input.costo())
                    .fecha(fecha)
                    .completado(input.completado())
                    .build();
            enriquecer(mantenimiento, lavadora);

            lavadora.agregarMantenimiento(mantenimiento);
            almacen.guardar(lavadora);
            almacen.registrarCambio("Mantenimiento " + input.id() + " agregado a la lavadora " + input.codigoLavadora());
            return mantenimiento;
        }
    }

    // ==================================================================
    //  Actualizar (mutation actualizarMantenimiento)
    // ==================================================================

    /** Busca el mantenimiento, valida lo que llego y cambia solo esos campos. */
    public Mantenimiento actualizar(int id, MantenimientoUpdateInput input) {
        synchronized (almacen.candado()) {
            Mantenimiento mantenimiento = buscarEnTodas(id).orElseThrow(() ->
                    new IllegalArgumentException("No existe un mantenimiento con el ID " + id));
            Lavadora lavadora = servicioLavadora.exigirPorCodigo(mantenimiento.getCodigoLavadora());
            enriquecer(mantenimiento, lavadora);

            if (input.descripcion() != null) {
                Validaciones.texto(input.descripcion(), "La descripcion del mantenimiento");
            }
            if (input.tecnico() != null) {
                Validaciones.texto(input.tecnico(), "El tecnico");
            }
            if (input.costo() != null) {
                Validaciones.noNegativo(input.costo(), "El costo del mantenimiento");
            }
            LocalDateTime fecha = (input.fecha() != null) ? Validaciones.fecha(input.fecha()) : null;

            if (input.descripcion() != null) {
                mantenimiento.setDescripcion(input.descripcion().trim());
            }
            if (input.tecnico() != null) {
                mantenimiento.setTecnico(input.tecnico().trim());
            }
            if (input.costo() != null) {
                mantenimiento.setCosto(input.costo());
            }
            if (fecha != null) {
                mantenimiento.setFecha(fecha);
            }
            if (input.completado() != null) {
                mantenimiento.setCompletado(input.completado());
            }

            Lavadora lavadoraGuardada = almacen.guardar(lavadora);
            Mantenimiento mantenimientoGuardado = buscarEnLavadora(lavadoraGuardada, id);
            enriquecer(mantenimientoGuardado, lavadoraGuardada);
            almacen.registrarCambio("Mantenimiento " + id + " de la lavadora " + lavadora.getCodigo() + " actualizado");
            return mantenimientoGuardado;
        }
    }

    // ==================================================================
    //  Eliminar (mutation eliminarMantenimiento)
    // ==================================================================

    /** Elimina un mantenimiento. Primero se BUSCA para confirmar que existe. */
    public boolean eliminar(int id) {
        synchronized (almacen.candado()) {
            Mantenimiento mantenimiento = buscarEnTodas(id).orElseThrow(() ->
                    new IllegalArgumentException("No existe un mantenimiento con el ID " + id));
            Lavadora lavadora = servicioLavadora.exigirPorCodigo(mantenimiento.getCodigoLavadora());
            boolean eliminado = lavadora.quitarMantenimiento(mantenimiento);
            almacen.guardar(lavadora);
            almacen.registrarCambio("Mantenimiento " + id + " de la lavadora " + lavadora.getCodigo() + " eliminado");
            return eliminado;
        }
    }

    // ==================================================================
    //  Apoyo interno
    // ==================================================================

    private Mantenimiento buscarEnLavadora(Lavadora lavadora, int id) {
        return lavadora.buscarMantenimiento(id)
                .orElseThrow(() -> new IllegalArgumentException(
                        "La lavadora " + lavadora.getCodigo() + " no tiene un mantenimiento con el id " + id));
    }

    private Optional<Mantenimiento> buscarEnTodas(int id) {
        return almacen.lavadoras().stream()
                .flatMap(lavadora -> lavadora.getMantenimientos().stream())
                .filter(mantenimiento -> mantenimiento.getId() == id)
                .findFirst()
                .map(mantenimiento -> {
                    almacen.buscarPorCodigo(mantenimiento.getCodigoLavadora())
                            .ifPresent(lavadora -> enriquecer(mantenimiento, lavadora));
                    return mantenimiento;
                });
    }

    private void enriquecer(Mantenimiento mantenimiento, Lavadora lavadora) {
        mantenimiento.setCodigoLavadora(lavadora.getCodigo());
        mantenimiento.setMarcaLavadora(lavadora.getMarca());
    }

    private void validarDescripcionYTecnico(String descripcion, String tecnico) {
        Validaciones.texto(descripcion, "La descripcion del mantenimiento");
        Validaciones.texto(tecnico, "El tecnico");
    }
}
