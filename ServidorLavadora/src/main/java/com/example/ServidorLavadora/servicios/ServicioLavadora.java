package com.example.ServidorLavadora.servicios;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.LavadoraInput;
import com.example.ServidorLavadora.model.LavadoraUpdateInput;
import com.example.ServidorLavadora.model.ProgramaLavado;
import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.util.Validaciones;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

/**
 * Logica del objeto DETALLE: la lavadora.
 * Depende de la abstraccion AlmacenDatos y del servicio de programas
 * (para saber que programa le toca segun los minutos de lavado).
 */
@Service // Spring crea un unico objeto de esta clase
public class ServicioLavadora {

    private static final int RPM_MINIMO = 600;

    private final AlmacenDatos almacen;
    private final ServicioProgramaLavado servicioPrograma;

    public ServicioLavadora(AlmacenDatos almacen, ServicioProgramaLavado servicioPrograma) {
        this.almacen = almacen;
        this.servicioPrograma = servicioPrograma;
    }

    // ---------------- Read ----------------

    public List<Lavadora> listar() {
        synchronized (almacen.candado()) {
            return new ArrayList<>(almacen.lavadoras()); // Copia de la lista
        }
    }

    public Optional<Lavadora> buscarPorCodigo(int codigo) {
        synchronized (almacen.candado()) {
            return almacen.lavadoras().stream()
                    .filter(l -> l.getCodigo() == codigo) // Filtra la lista
                    .findFirst();
        }
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
                    .filter(l -> codigo == null || l.getCodigo() == codigo)
                    .filter(l -> texto.isEmpty() || l.getMarca().toLowerCase().contains(texto))
                    .findFirst();
        }
    }

    /**
     * Listado filtrado en el servidor por 2 parametros opcionales:
     * marca (contiene el texto, sin importar mayusculas) y tecnologiaInverter (true o false).
     */
    public List<Lavadora> listarPorFiltro(String marca, Boolean tecnologiaInverter) {
        String texto = (marca == null) ? "" : marca.trim().toLowerCase();
        return listar().stream()
                .filter(l -> texto.isEmpty() || l.getMarca().toLowerCase().contains(texto))
                .filter(l -> tecnologiaInverter == null || l.isTecnologiaInverter() == tecnologiaInverter)
                .toList();
    }

    // ---------------- Create ----------------

    public Lavadora agregar(LavadoraInput input) {
        synchronized (almacen.candado()) {
            if (buscarPorCodigo(input.codigo()).isPresent()) {
                throw new IllegalArgumentException("Ya existe una lavadora con el codigo " + input.codigo());
            }
            Validaciones.texto(input.marca(), "La marca");
            Validaciones.positivo(input.precioBase(), "El precio base");
            Validaciones.positivo(input.capacidadCargaKilos(), "La capacidad de carga en kilos");
            validarRpm(input.velocidadCentrifugadoRPM());
            LocalDateTime fecha = Validaciones.fecha(input.fechaFabricacion());
            ProgramaLavado programa = servicioPrograma.determinarPorMinutos(input.minutosLavado());

            Lavadora lavadora = Lavadora.builder() // Patron Builder
                    .codigo(input.codigo())
                    .marca(input.marca().trim())
                    .precioBase(input.precioBase())
                    .fechaFabricacion(fecha)
                    .capacidadCargaKilos(input.capacidadCargaKilos())
                    .velocidadCentrifugadoRPM(input.velocidadCentrifugadoRPM())
                    .tecnologiaInverter(input.tecnologiaInverter())
                    .minutosLavado(input.minutosLavado())
                    .programaLavado(programa)
                    .build();
            almacen.lavadoras().add(lavadora);
            almacen.notificar("Lavadora " + lavadora.getCodigo() + " (" + lavadora.getMarca() + ") agregada");
            return lavadora;
        }
    }

    // ---------------- Update ----------------

    /** Cambia solo lo que venga en el input. Primero se valida todo y despues se cambia. */
    public Lavadora actualizar(int codigo, LavadoraUpdateInput input) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = buscarPorCodigo(codigo)
                    .orElseThrow(() -> new IllegalArgumentException("No existe una lavadora con el codigo " + codigo));

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
            LocalDateTime fecha = (input.fechaFabricacion() != null) ? Validaciones.fecha(input.fechaFabricacion()) : null;
            ProgramaLavado programa = (input.minutosLavado() != null)
                    ? servicioPrograma.determinarPorMinutos(input.minutosLavado())
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
            if (programa != null) {
                lavadora.setMinutosLavado(input.minutosLavado());
                lavadora.setProgramaLavado(programa);
            }
            almacen.notificar("Lavadora " + codigo + " actualizada");
            return lavadora;
        }
    }

    // ---------------- Delete ----------------

    public boolean eliminar(int codigo) {
        synchronized (almacen.candado()) {
            Lavadora lavadora = buscarPorCodigo(codigo)
                    .orElseThrow(() -> new IllegalArgumentException("No existe una lavadora con el codigo " + codigo));
            boolean eliminada = almacen.lavadoras().remove(lavadora);
            almacen.notificar("Lavadora " + codigo + " eliminada");
            return eliminada;
        }
    }

    // ---------------- Validaciones propias de la lavadora ----------------

    private void validarRpm(int rpm) {
        if (rpm < RPM_MINIMO) {
            throw new IllegalArgumentException("La velocidad de centrifugado minima debe ser " + RPM_MINIMO + " RPM");
        }
    }
}
