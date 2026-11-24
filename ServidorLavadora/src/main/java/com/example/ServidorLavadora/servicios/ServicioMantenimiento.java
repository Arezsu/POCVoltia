package com.example.ServidorLavadora.servicios;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.Mantenimiento;
import com.example.ServidorLavadora.model.MantenimientoInput;
import com.example.ServidorLavadora.model.MantenimientoUpdateInput;
import com.example.ServidorLavadora.model.TecnicoDto;
import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.util.Validaciones;
import org.springframework.stereotype.Service;
import org.springframework.graphql.client.GraphQlTransportException;

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
    private final ClienteTecnicos clienteTecnicos;

    public ServicioMantenimiento(AlmacenDatos almacen, ServicioLavadora servicioLavadora, ClienteTecnicos clienteTecnicos) {
        this.almacen = almacen;
        this.servicioLavadora = servicioLavadora;
        this.clienteTecnicos = clienteTecnicos;
    }

    // ==================================================================
    //  Consultas (queries)
    // ==================================================================

    /** Lista los mantenimientos de una lavadora (puede ser una lista vacia). */
    public List<Mantenimiento> listar(int codigoLavadora) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = servicioLavadora.exigirPorCodigo(codigoLavadora);
            List<Mantenimiento> resultado = new ArrayList<>(almacen.mantenimientosPorLavadora(codigoLavadora));
            resultado.forEach(mantenimiento -> enriquecer(mantenimiento, lavadora));
            return resultado;
        }
    }

    /** Lista todos los mantenimientos sin exigir primero el código del maestro. */
    public List<Mantenimiento> listarTodos() {
        synchronized (almacen.candado()) {
            return almacen.mantenimientos().stream()
                    .map(mantenimiento -> {
                        almacen.buscarPorCodigo(mantenimiento.getCodigoLavadora())
                                .ifPresent(lavadora -> enriquecer(mantenimiento, lavadora));
                        return mantenimiento;
                    })
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
            if (almacen.buscarMantenimiento(input.id()).isPresent())
                throw new IllegalArgumentException("Ya existe un mantenimiento con el ID " + input.id());

            validarDescripcionYTecnico(input.descripcion());
            TecnicoDto tecnico = exigirTecnicoActivo(input.codigoTecnico());
            Validaciones.noNegativo(input.costo(), "El costo del mantenimiento");
            LocalDateTime fecha = Validaciones.fecha(input.fecha());

            Mantenimiento mantenimiento = Mantenimiento.builder()
                    .id(input.id())
                    .descripcion(input.descripcion().trim())
                    .tecnico(tecnico.nombre())
                    .costo(input.costo())
                    .fecha(fecha)
                    .completado(input.completado())
                    .build();
            mantenimiento.setCodigoTecnico(tecnico.codigo());
            enriquecer(mantenimiento, lavadora);

            almacen.guardarMantenimiento(mantenimiento);
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
            TecnicoDto tecnicoActualizado = input.codigoTecnico() == null ? null : exigirTecnicoActivo(input.codigoTecnico());
            if (input.costo() != null) {
                Validaciones.noNegativo(input.costo(), "El costo del mantenimiento");
            }
            LocalDateTime fecha = (input.fecha() != null) ? Validaciones.fecha(input.fecha()) : null;

            if (input.descripcion() != null) {
                mantenimiento.setDescripcion(input.descripcion().trim());
            }
            if (tecnicoActualizado != null) {
                mantenimiento.setCodigoTecnico(tecnicoActualizado.codigo());
                mantenimiento.setTecnico(tecnicoActualizado.nombre());
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

            Mantenimiento mantenimientoGuardado = almacen.guardarMantenimiento(mantenimiento);
            enriquecer(mantenimientoGuardado, lavadora);
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
            servicioLavadora.exigirPorCodigo(mantenimiento.getCodigoLavadora());
            boolean eliminado = almacen.eliminarMantenimiento(id);
            almacen.registrarCambio("Mantenimiento " + id + " de la lavadora " + mantenimiento.getCodigoLavadora() + " eliminado");
            return eliminado;
        }
    }

    // ==================================================================
    //  Apoyo interno
    // ==================================================================

    private Optional<Mantenimiento> buscarEnTodas(int id) {
        return almacen.buscarMantenimiento(id)
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

    private void validarDescripcionYTecnico(String descripcion) {
        Validaciones.texto(descripcion, "La descripcion del mantenimiento");
    }

    public boolean tieneTecnico(int codigoTecnico) {
        synchronized (almacen.candado()) {
            return listarTodos().stream().anyMatch(m -> m.getCodigoTecnico() == codigoTecnico);
        }
    }

    public List<Mantenimiento> deLavadora(int codigo) {
        return listar(codigo);
    }
    public int cantidad(int codigo) { return listar(codigo).size(); }
    public int pendientes(int codigo) { return (int) listar(codigo).stream().filter(m -> !m.isCompletado()).count(); }
    public double costo(int codigo) { return listar(codigo).stream().mapToDouble(Mantenimiento::getCosto).sum(); }
    public boolean tieneMantenimientos(int codigo) { return almacen.tieneMantenimientos(codigo); }
    public void registrarCambio(String evento) { almacen.registrarCambio(evento); }
    public List<Lavadora> lavadorasDeTecnico(int codigoTecnico) {
        synchronized (almacen.candado()) {
            return almacen.mantenimientos().stream()
                    .filter(m -> m.getCodigoTecnico() == codigoTecnico)
                    .map(Mantenimiento::getCodigoLavadora)
                    .distinct()
                    .map(servicioLavadora::buscarPorCodigo)
                    .flatMap(Optional::stream)
                    .toList();
        }
    }
    public void actualizarNombreTecnico(int codigoTecnico, String nombre) {
        synchronized (almacen.candado()) {
            almacen.mantenimientos().stream()
                    .filter(m -> m.getCodigoTecnico() == codigoTecnico)
                    .forEach(m -> { m.setTecnico(nombre); almacen.guardarMantenimiento(m); });
        }
    }

    private TecnicoDto exigirTecnicoActivo(int codigo) {
        if (codigo < 1) throw new IllegalArgumentException("El codigo del tecnico debe ser un entero mayor que cero.");
        TecnicoDto tecnico;
        try {
            tecnico = clienteTecnicos.buscar(codigo);
        } catch (GraphQlTransportException ex) {
            throw new IllegalStateException("No se pudo conectar con el microservicio de tecnicos. Verifique que este iniciado en el puerto 8082.", ex);
        }
        if (tecnico == null) throw new IllegalArgumentException("No existe un tecnico registrado con el codigo " + codigo + ".");
        if (!tecnico.activo()) throw new IllegalArgumentException("El tecnico seleccionado esta inactivo.");
        return tecnico;
    }
}
