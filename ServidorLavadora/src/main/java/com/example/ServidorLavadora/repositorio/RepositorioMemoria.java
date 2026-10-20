package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.ProgramaLavado;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;

/**
 * Guarda los datos en memoria principal, en dos colecciones (ArrayList).
 *
 * Patron SINGLETON: solo existe UNA instancia en todo el servidor. Asi todos los
 * clientes trabajan sobre los mismos datos. El constructor es privado y la unica
 * forma de obtenerla es RepositorioMemoria.getInstance().
 *
 * Tambien es el "sujeto" del patron OBSERVER: avisa a los observadores en cada cambio.
 */
public class RepositorioMemoria implements AlmacenDatos {

    private static RepositorioMemoria instancia; // La unica instancia

    private final Object candado = new Object();
    private final List<ProgramaLavado> programas = new ArrayList<>();
    private final List<Lavadora> lavadoras = new ArrayList<>();
    private final List<ObservadorRepositorio> observadores = new CopyOnWriteArrayList<>(); // Lista segura para varios hilos

    // Constructor privado: nadie puede hacer "new RepositorioMemoria()"
    private RepositorioMemoria() {
        cargarDatosDeEjemplo();
    }

    /** Punto de acceso a la unica instancia (se crea la primera vez que se pide). */
    public static synchronized RepositorioMemoria getInstance() { // Un hilo solamente
        if (instancia == null) {
            instancia = new RepositorioMemoria();
        }
        return instancia;
    }

    public void agregarObservador(ObservadorRepositorio observador) {
        observadores.add(observador);
    }

    @Override
    public Object candado() {
        return candado;
    }

    @Override
    public List<ProgramaLavado> programas() {
        return programas;
    }

    @Override
    public List<Lavadora> lavadoras() {
        return lavadoras;
    }

    @Override
    public void notificar(String evento) {
        for (ObservadorRepositorio observador : observadores) {
            observador.alCambiar(evento);
        }
    }

    // ---------------- Datos de ejemplo ----------------

    private void cargarDatosDeEjemplo() {
        ProgramaLavado rapido = crearPrograma(1, "Rapido", 10, 30, 35.0, false);
        ProgramaLavado normal = crearPrograma(2, "Normal", 31, 60, 55.0, true);
        ProgramaLavado intensivo = crearPrograma(3, "Intensivo", 61, 90, 75.5, true);
        crearPrograma(4, "Ropa pesada", 91, 180, 95.0, true);

        crearLavadora(101, "LG", 1450000, LocalDateTime.of(2024, 3, 10, 9, 0), 18.0, 1400, true, 45, normal);
        crearLavadora(102, "Samsung", 1290000, LocalDateTime.of(2023, 11, 22, 14, 30), 16.0, 1200, false, 30, rapido);
        crearLavadora(103, "Whirlpool", 980000, LocalDateTime.of(2025, 1, 5, 8, 15), 14.0, 1000, false, 60, normal);
        crearLavadora(104, "LG", 1650000, LocalDateTime.of(2025, 6, 18, 10, 45), 20.0, 1600, true, 90, intensivo);
    }

    private ProgramaLavado crearPrograma(int codigo, String nombre, int min, int max, double agua, boolean caliente) {
        ProgramaLavado programa = ProgramaLavado.builder() // Patron Builder
                .codigo(codigo)
                .nombre(nombre)
                .minMinutos(min)
                .maxMinutos(max)
                .consumoAguaLitros(agua)
                .fechaCreacion(LocalDateTime.of(2025, 1, 10, 8, 0))
                .aguaCaliente(caliente)
                .build();
        programas.add(programa);
        return programa;
    }

    private void crearLavadora(int codigo, String marca, double precio, LocalDateTime fecha, double kilos,
                               int rpm, boolean inverter, int minutos, ProgramaLavado programa) {
        lavadoras.add(Lavadora.builder() // Patron Builder
                .codigo(codigo)
                .marca(marca)
                .precioBase(precio)
                .fechaFabricacion(fecha)
                .capacidadCargaKilos(kilos)
                .velocidadCentrifugadoRPM(rpm)
                .tecnologiaInverter(inverter)
                .minutosLavado(minutos)
                .programaLavado(programa)
                .build());
    }
}
