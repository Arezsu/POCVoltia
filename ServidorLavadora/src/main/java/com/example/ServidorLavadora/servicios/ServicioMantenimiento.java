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
import java.util.List;
import java.util.Optional;

/**
 * Logica de negocio del objeto DETALLE: el mantenimiento.
 *
 * Todo mantenimiento vive DENTRO de una lavadora (relacion maestro-detalle), por
 * eso cada operacion recibe el codigo de la lavadora. Primero se busca la lavadora
 * y despues se trabaja sobre su lista de mantenimientos.
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
            return new ArrayList<>(servicioLavadora.exigirPorCodigo(codigoLavadora).getMantenimientos());
        }
    }

    /** Busca un mantenimiento por su id dentro de una lavadora. */
    public Optional<Mantenimiento> buscarPorId(int codigoLavadora, int id) {
        synchronized (almacen.candado()) {
            return servicioLavadora.exigirPorCodigo(codigoLavadora).buscarMantenimiento(id);
        }
    }

    /**
     * Filtra los mantenimientos de una lavadora EN EL SERVIDOR:
     *  - tecnico: texto que debe aparecer en el nombre del tecnico
     *  - completado: true = terminados, false = pendientes, null = todos
     */
    public List<Mantenimiento> listarPorFiltro(int codigoLavadora, String tecnico, Boolean completado) {
        String texto = (tecnico == null) ? "" : tecnico.trim().toLowerCase();
        return listar(codigoLavadora).stream()
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
     * Agrega un mantenimiento a una lavadora existente. El id lo calcula el servidor:
     * toma el mayor id de esa lavadora y le suma uno. Asi nunca se repite.
     */
    public Mantenimiento agregar(int codigoLavadora, MantenimientoInput input) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = servicioLavadora.exigirPorCodigo(codigoLavadora);

            validarDescripcionYTecnico(input.descripcion(), input.tecnico());
            Validaciones.noNegativo(input.costo(), "El costo del mantenimiento");
            LocalDateTime fecha = Validaciones.fecha(input.fecha());

            int id = siguienteId(lavadora);
            Mantenimiento mantenimiento = Mantenimiento.builder()
                    .id(id)
                    .descripcion(input.descripcion().trim())
                    .tecnico(input.tecnico().trim())
                    .costo(input.costo())
                    .fecha(fecha)
                    .completado(input.completado())
                    .build();

            lavadora.agregarMantenimiento(mantenimiento);
            almacen.notificar("Mantenimiento " + id + " agregado a la lavadora " + codigoLavadora);
            return mantenimiento;
        }
    }

    // ==================================================================
    //  Actualizar (mutation actualizarMantenimiento)
    // ==================================================================

    /** Busca el mantenimiento, valida lo que llego y cambia solo esos campos. */
    public Mantenimiento actualizar(int codigoLavadora, int id, MantenimientoUpdateInput input) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = servicioLavadora.exigirPorCodigo(codigoLavadora);
            Mantenimiento mantenimiento = buscarEnLavadora(lavadora, id);

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

            almacen.notificar("Mantenimiento " + id + " de la lavadora " + codigoLavadora + " actualizado");
            return mantenimiento;
        }
    }

    // ==================================================================
    //  Eliminar (mutation eliminarMantenimiento)
    // ==================================================================

    /** Elimina un mantenimiento. Primero se BUSCA para confirmar que existe. */
    public boolean eliminar(int codigoLavadora, int id) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = servicioLavadora.exigirPorCodigo(codigoLavadora);
            Mantenimiento mantenimiento = buscarEnLavadora(lavadora, id);
            boolean eliminado = lavadora.quitarMantenimiento(mantenimiento);
            almacen.notificar("Mantenimiento " + id + " de la lavadora " + codigoLavadora + " eliminado");
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

    private int siguienteId(Lavadora lavadora) {
        return lavadora.getMantenimientos().stream()
                .mapToInt(Mantenimiento::getId)
                .max()
                .orElse(0) + 1;
    }

    private void validarDescripcionYTecnico(String descripcion, String tecnico) {
        Validaciones.texto(descripcion, "La descripcion del mantenimiento");
        Validaciones.texto(tecnico, "El tecnico");
    }
}
