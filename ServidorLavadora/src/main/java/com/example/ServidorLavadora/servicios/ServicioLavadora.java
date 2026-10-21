package com.example.ServidorLavadora.servicios;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.LavadoraInput;
import com.example.ServidorLavadora.model.LavadoraUpdateInput;
import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.util.Validaciones;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

/**
 * Logica de negocio del objeto MAESTRO: la lavadora.
 *
 * El servicio recibe el AlmacenDatos por el constructor (inyeccion de dependencias):
 * no sabe si los datos estan en memoria o en una base de datos, solo conoce la
 * interface. Eso es el principio de inversion de dependencias (la D de SOLID).
 *
 * Aqui se valida todo antes de guardar y se avisa a los observadores en cada cambio.
 */
@Service // Spring crea un unico objeto de esta clase y lo entrega donde se pida
public class ServicioLavadora {

    private static final int RPM_MINIMO = 600;
    private static final int MINUTOS_MINIMOS = 1;
    private static final int MINUTOS_MAXIMOS = 300;

    private final AlmacenDatos almacen;

    public ServicioLavadora(AlmacenDatos almacen) {
        this.almacen = almacen;
    }

    // ==================================================================
    //  Consultas (queries)
    // ==================================================================

    /** Devuelve una copia de la lista de lavadoras. */
    public List<Lavadora> listar() {
        synchronized (almacen.candado()) {
            return new ArrayList<>(almacen.lavadoras());
        }
    }

    /** Busca una lavadora por su codigo. Devuelve Optional porque puede no existir. */
    public Optional<Lavadora> buscarPorCodigo(int codigo) {
        synchronized (almacen.candado()) {
            return almacen.lavadoras().stream()
                    .filter(lavadora -> lavadora.getCodigo() == codigo)
                    .findFirst();
        }
    }

    /** Igual que buscarPorCodigo pero lanza un error claro si no existe. */
    public Lavadora exigirPorCodigo(int codigo) {
        return buscarPorCodigo(codigo)
                .orElseThrow(() -> new IllegalArgumentException(
                        "No existe una lavadora con el codigo " + codigo));
    }

    /**
     * Consulta de UNA lavadora con uno o dos criterios: codigo y/o marca.
     * Si llegan los dos, la lavadora debe cumplir los dos.
     */
    public Optional<Lavadora> buscarPorCriterios(Integer codigo, String marca) {
        if (codigo == null && (marca == null || marca.isBlank())) {
            throw new IllegalArgumentException("Escriba al menos un criterio de busqueda (codigo o marca)");
        }
        String texto = (marca == null) ? "" : marca.trim().toLowerCase();
        synchronized (almacen.candado()) {
            return almacen.lavadoras().stream()
                    .filter(lavadora -> codigo == null || lavadora.getCodigo() == codigo)
                    .filter(lavadora -> texto.isEmpty() || lavadora.getMarca().toLowerCase().contains(texto))
                    .findFirst();
        }
    }

    /**
     * Listado con filtros que se aplican EN EL SERVIDOR (no en el cliente):
     *  - marca: texto que debe aparecer en la marca (sin importar mayusculas)
     *  - tecnologiaInverter: true o false
     *  - conMantenimientos: true = solo las que tienen al menos un mantenimiento,
     *                       false = solo las que no tienen ninguno, null = todas
     */
    public List<Lavadora> listarPorFiltro(String marca, Boolean tecnologiaInverter, Boolean conMantenimientos) {
        String texto = (marca == null) ? "" : marca.trim().toLowerCase();
        return listar().stream()
                .filter(lavadora -> texto.isEmpty() || lavadora.getMarca().toLowerCase().contains(texto))
                .filter(lavadora -> tecnologiaInverter == null
                        || lavadora.isTecnologiaInverter() == tecnologiaInverter)
                .filter(lavadora -> conMantenimientos == null
                        || (lavadora.getCantidadMantenimientos() > 0) == conMantenimientos)
                .toList();
    }

    /** Numero de version de los datos (lo usan los clientes para refrescar la grilla). */
    public int version() {
        return almacen.version();
    }

    // ==================================================================
    //  Crear (mutation addLavadora)
    // ==================================================================

    /** Crea una lavadora nueva. Empieza con cero mantenimientos (lado 0..* de la relacion). */
    public Lavadora agregar(LavadoraInput input) {
        synchronized (almacen.candado()) {
            if (buscarPorCodigo(input.codigo()).isPresent()) {
                throw new IllegalArgumentException("Ya existe una lavadora con el codigo " + input.codigo());
            }

            // Primero se valida TODO. Asi no se guarda nada a medias.
            Validaciones.enteroPositivo(input.codigo(), "El codigo de la lavadora");
            Validaciones.texto(input.marca(), "La marca");
            Validaciones.positivo(input.precioBase(), "El precio base");
            Validaciones.positivo(input.capacidadCargaKilos(), "La capacidad de carga en kilos");
            validarRpm(input.velocidadCentrifugadoRPM());
            Validaciones.rango(input.minutosLavado(), MINUTOS_MINIMOS, MINUTOS_MAXIMOS,
                    "Los minutos de lavado");
            LocalDateTime fecha = Validaciones.fecha(input.fechaFabricacion());

            // Patron Builder: se arma paso a paso, sin un constructor de ocho parametros.
            Lavadora lavadora = Lavadora.builder()
                    .codigo(input.codigo())
                    .marca(input.marca().trim())
                    .precioBase(input.precioBase())
                    .fechaFabricacion(fecha)
                    .capacidadCargaKilos(input.capacidadCargaKilos())
                    .velocidadCentrifugadoRPM(input.velocidadCentrifugadoRPM())
                    .tecnologiaInverter(input.tecnologiaInverter())
                    .minutosLavado(input.minutosLavado())
                    .build();

            almacen.lavadoras().add(lavadora);
            almacen.notificar("Lavadora " + lavadora.getCodigo() + " (" + lavadora.getMarca() + ") agregada");
            return lavadora;
        }
    }

    // ==================================================================
    //  Actualizar (mutation actualizarLavadora)
    // ==================================================================

    /**
     * Actualiza una lavadora. Primero BUSCA (si no existe, responde error) y despues
     * cambia solo los campos que llegaron; lo que viene null se queda como estaba.
     */
    public Lavadora actualizar(int codigo, LavadoraUpdateInput input) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = exigirPorCodigo(codigo);

            // Validar lo que llego antes de modificar el objeto.
            if (input.marca() != null) {
                Validaciones.texto(input.marca(), "La marca");
            }
            if (input.precioBase() != null) {
                Validaciones.positivo(input.precioBase(), "El precio base");
            }
            if (input.capacidadCargaKilos() != null) {
                Validaciones.positivo(input.capacidadCargaKilos(), "La capacidad de carga en kilos");
            }
            if (input.velocidadCentrifugadoRPM() != null) {
                validarRpm(input.velocidadCentrifugadoRPM());
            }
            if (input.minutosLavado() != null) {
                Validaciones.rango(input.minutosLavado(), MINUTOS_MINIMOS, MINUTOS_MAXIMOS,
                        "Los minutos de lavado");
            }
            LocalDateTime fecha = (input.fechaFabricacion() != null)
                    ? Validaciones.fecha(input.fechaFabricacion())
                    : null;

            if (input.marca() != null) {
                lavadora.setMarca(input.marca().trim());
            }
            if (input.precioBase() != null) {
                lavadora.setPrecioBase(input.precioBase());
            }
            if (fecha != null) {
                lavadora.setFechaFabricacion(fecha);
            }
            if (input.capacidadCargaKilos() != null) {
                lavadora.setCapacidadCargaKilos(input.capacidadCargaKilos());
            }
            if (input.velocidadCentrifugadoRPM() != null) {
                lavadora.setVelocidadCentrifugadoRPM(input.velocidadCentrifugadoRPM());
            }
            if (input.tecnologiaInverter() != null) {
                lavadora.setTecnologiaInverter(input.tecnologiaInverter());
            }
            if (input.minutosLavado() != null) {
                lavadora.setMinutosLavado(input.minutosLavado());
            }

            almacen.notificar("Lavadora " + codigo + " actualizada");
            return lavadora;
        }
    }

    // ==================================================================
    //  Eliminar (mutation eliminarLavadora)
    // ==================================================================

    /**
     * Elimina una lavadora. Primero se BUSCA para confirmar que existe.
     * Al borrar el maestro se borran tambien sus mantenimientos, porque el detalle
     * esta guardado dentro de la lavadora (relacion de composicion).
     */
    public boolean eliminar(int codigo) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = exigirPorCodigo(codigo);
            boolean eliminada = almacen.lavadoras().remove(lavadora);
            almacen.notificar("Lavadora " + codigo + " eliminada");
            return eliminada;
        }
    }

    // ==================================================================
    //  Validaciones propias de la lavadora
    // ==================================================================

    private void validarRpm(int rpm) {
        if (rpm < RPM_MINIMO) {
            throw new IllegalArgumentException(
                    "La velocidad de centrifugado minima debe ser " + RPM_MINIMO + " RPM");
        }
    }
}
