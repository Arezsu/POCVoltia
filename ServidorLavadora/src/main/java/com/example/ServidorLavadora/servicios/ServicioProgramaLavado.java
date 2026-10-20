package com.example.ServidorLavadora.servicios;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.ProgramaInput;
import com.example.ServidorLavadora.model.ProgramaLavado;
import com.example.ServidorLavadora.model.ProgramaUpdateInput;
import com.example.ServidorLavadora.repositorio.AlmacenDatos;
import com.example.ServidorLavadora.util.Validaciones;
import org.springframework.stereotype.Service;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

/**
 * Logica del objeto MAESTRO: el programa de lavado.
 * Recibe el AlmacenDatos por el constructor (inyeccion de dependencias): no sabe
 * si los datos estan en memoria o en una base de datos.
 */
@Service // Spring crea un unico objeto de esta clase
public class ServicioProgramaLavado {

    private static final int MINUTOS_MAXIMOS = 300;

    private final AlmacenDatos almacen;

    public ServicioProgramaLavado(AlmacenDatos almacen) {
        this.almacen = almacen;
    }

    // ---------------- Read ----------------

    public List<ProgramaLavado> listar() {
        synchronized (almacen.candado()) {
            return new ArrayList<>(almacen.programas()); // Copia de la lista
        }
    }

    public Optional<ProgramaLavado> buscarPorCodigo(int codigo) {
        synchronized (almacen.candado()) {
            return almacen.programas().stream()
                    .filter(p -> p.getCodigo() == codigo) // Filtra la lista
                    .findFirst();
        }
    }

    /**
     * Consulta de UN programa con uno o dos criterios: codigo y/o nombre.
     * Si llegan los dos, el programa debe cumplir los dos.
     */
    public Optional<ProgramaLavado> buscarPorCriterios(Integer codigo, String nombre) {
        if (codigo == null && (nombre == null || nombre.isBlank())) {
            throw new IllegalArgumentException("Escriba al menos un criterio de busqueda (codigo o nombre)");
        }
        String texto = (nombre == null) ? "" : nombre.trim().toLowerCase();
        synchronized (almacen.candado()) {
            return almacen.programas().stream()
                    .filter(p -> codigo == null || p.getCodigo() == codigo)
                    .filter(p -> texto.isEmpty() || p.getNombre().toLowerCase().contains(texto))
                    .findFirst();
        }
    }

    /** Listado filtrado en el servidor: nombre (contiene el texto) y aguaCaliente (true o false). Ambos opcionales. */
    public List<ProgramaLavado> listarPorFiltro(String nombre, Boolean aguaCaliente) {
        String texto = (nombre == null) ? "" : nombre.trim().toLowerCase();
        return listar().stream()
                .filter(p -> texto.isEmpty() || p.getNombre().toLowerCase().contains(texto))
                .filter(p -> aguaCaliente == null || p.isAguaCaliente() == aguaCaliente)
                .toList();
    }

    /** Busca el programa que cubre los minutos (lo usa la lavadora para saber cual programa tiene). */
    public ProgramaLavado determinarPorMinutos(int minutos) {
        synchronized (almacen.candado()) {
            return almacen.programas().stream()
                    .filter(p -> p.cubre(minutos))
                    .findFirst()
                    .orElseThrow(() -> new IllegalArgumentException(
                            "Ningun programa de lavado cubre " + minutos
                                    + " minutos. Cree primero un programa que incluya ese tiempo"));
        }
    }

    // ---------------- Create ----------------

    public ProgramaLavado agregar(ProgramaInput input) {
        synchronized (almacen.candado()) {
            if (buscarPorCodigo(input.codigo()).isPresent()) {
                throw new IllegalArgumentException("Ya existe un programa con el codigo " + input.codigo());
            }
            Validaciones.texto(input.nombre(), "El nombre");
            validarNombreLibre(input.nombre(), input.codigo());
            Validaciones.positivo(input.consumoAguaLitros(), "El consumo de agua");
            validarRango(input.minMinutos(), input.maxMinutos(), input.codigo());
            LocalDateTime fecha = Validaciones.fecha(input.fechaCreacion());

            ProgramaLavado programa = ProgramaLavado.builder() // Patron Builder
                    .codigo(input.codigo())
                    .nombre(input.nombre().trim())
                    .minMinutos(input.minMinutos())
                    .maxMinutos(input.maxMinutos())
                    .consumoAguaLitros(input.consumoAguaLitros())
                    .fechaCreacion(fecha)
                    .aguaCaliente(input.aguaCaliente())
                    .build();
            almacen.programas().add(programa);
            almacen.notificar("Programa " + programa.getCodigo() + " (" + programa.getNombre() + ") agregado");
            return programa;
        }
    }

    // ---------------- Update ----------------

    /** Cambia solo lo que venga en el input. Primero se valida todo y despues se cambia. */
    public ProgramaLavado actualizar(int codigo, ProgramaUpdateInput input) {
        synchronized (almacen.candado()) {
            ProgramaLavado programa = buscarPorCodigo(codigo)
                    .orElseThrow(() -> new IllegalArgumentException("No existe un programa con el codigo " + codigo));

            String nombre = (input.nombre() != null) ? input.nombre().trim() : programa.getNombre();
            int min = (input.minMinutos() != null) ? input.minMinutos() : programa.getMinMinutos();
            int max = (input.maxMinutos() != null) ? input.maxMinutos() : programa.getMaxMinutos();
            double agua = (input.consumoAguaLitros() != null) ? input.consumoAguaLitros() : programa.getConsumoAguaLitros();
            LocalDateTime fecha = (input.fechaCreacion() != null)
                    ? Validaciones.fecha(input.fechaCreacion()) : programa.getFechaCreacion();

            Validaciones.texto(nombre, "El nombre");
            validarNombreLibre(nombre, codigo);
            Validaciones.positivo(agua, "El consumo de agua");
            validarRango(min, max, codigo);
            validarLavadorasSiguenCubiertas(codigo, min, max);

            programa.setNombre(nombre);
            programa.setMinMinutos(min);
            programa.setMaxMinutos(max);
            programa.setConsumoAguaLitros(agua);
            programa.setFechaCreacion(fecha);
            if (input.aguaCaliente() != null) {
                programa.setAguaCaliente(input.aguaCaliente());
            }
            almacen.notificar("Programa " + codigo + " actualizado");
            return programa;
        }
    }

    // ---------------- Delete ----------------

    /** No se puede eliminar un programa que todavia usan lavadoras (relacion maestro-detalle). */
    public boolean eliminar(int codigo) {
        synchronized (almacen.candado()) {
            ProgramaLavado programa = buscarPorCodigo(codigo)
                    .orElseThrow(() -> new IllegalArgumentException("No existe un programa con el codigo " + codigo));

            long usadas = contarLavadoras(codigo);
            if (usadas > 0) {
                throw new IllegalArgumentException("No se puede eliminar el programa " + programa.getNombre()
                        + " porque lo usan " + usadas + " lavadora(s)");
            }
            boolean eliminado = almacen.programas().remove(programa);
            almacen.notificar("Programa " + codigo + " eliminado");
            return eliminado;
        }
    }

    /** Cuantas lavadoras usan este programa (la parte "0..*" de la relacion). */
    public long contarLavadoras(int codigoPrograma) {
        synchronized (almacen.candado()) {
            return almacen.lavadoras().stream()
                    .filter(l -> l.getProgramaLavado() != null && l.getProgramaLavado().getCodigo() == codigoPrograma)
                    .count();
        }
    }

    // ---------------- Validaciones propias del programa ----------------

    private void validarNombreLibre(String nombre, int codigoActual) {
        boolean repetido = almacen.programas().stream()
                .anyMatch(p -> p.getCodigo() != codigoActual && p.getNombre().equalsIgnoreCase(nombre.trim()));
        if (repetido) {
            throw new IllegalArgumentException("Ya existe un programa con el nombre " + nombre.trim());
        }
    }

    /** El rango debe ser valido y no cruzarse con otro programa (asi cada tiempo tiene un solo programa). */
    private void validarRango(int min, int max, int codigoActual) {
        if (min < 1) {
            throw new IllegalArgumentException("Los minutos minimos deben ser al menos 1");
        }
        if (max < min) {
            throw new IllegalArgumentException("Los minutos maximos no pueden ser menores que los minimos");
        }
        if (max > MINUTOS_MAXIMOS) {
            throw new IllegalArgumentException("Los minutos maximos no pueden pasar de " + MINUTOS_MAXIMOS);
        }
        for (ProgramaLavado otro : almacen.programas()) {
            boolean seCruza = otro.getCodigo() != codigoActual && min <= otro.getMaxMinutos() && max >= otro.getMinMinutos();
            if (seCruza) {
                throw new IllegalArgumentException("El rango " + min + "-" + max
                        + " se cruza con el programa " + otro.getNombre()
                        + " (" + otro.getMinMinutos() + "-" + otro.getMaxMinutos() + ")");
            }
        }
    }

    /** Si se cambia el rango, las lavadoras que usan el programa deben seguir dentro de el. */
    private void validarLavadorasSiguenCubiertas(int codigoPrograma, int min, int max) {
        for (Lavadora lavadora : almacen.lavadoras()) {
            boolean usaPrograma = lavadora.getProgramaLavado() != null
                    && lavadora.getProgramaLavado().getCodigo() == codigoPrograma;
            if (usaPrograma && (lavadora.getMinutosLavado() < min || lavadora.getMinutosLavado() > max)) {
                throw new IllegalArgumentException("No se puede cambiar el rango: la lavadora " + lavadora.getCodigo()
                        + " (" + lavadora.getMinutosLavado() + " min) quedaria fuera del programa");
            }
        }
    }
}
