package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.Mantenimiento;

import java.time.LocalDateTime;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Guarda todos los datos del sistema en la memoria principal del servidor.
 *
 * Patron SINGLETON: en todo el servidor existe UNA sola instancia de esta clase.
 * Si hubiera varias, una lavadora creada por un cliente apareceria en una copia
 * de los datos y no en la que consulta el otro cliente. El constructor es privado
 * y la unica forma de obtener el objeto es RepositorioMemoria.getInstance().
 *
 * Patron OBSERVER (como sujeto): guarda una lista de observadores y les avisa en
 * cada cambio. Ademas, cada aviso sube en 1 el numero de "version".
 *
 * Listas concurrentes: se usa CopyOnWriteArrayList porque el servidor atiende
 * varias peticiones al mismo tiempo. Las operaciones que cambian datos se hacen
 * dentro del candado para que no se crucen.
 */
public class RepositorioMemoria implements AlmacenDatos {

    // volatile: la instancia se crea una vez y todos los hilos deben ver el mismo valor.
    private static volatile RepositorioMemoria instancia;

    private final Object candado = new Object();

    // Lista concurrente: varias peticiones pueden leerla sin bloquearse.
    private final List<Lavadora> lavadoras = new CopyOnWriteArrayList<>();

    // Lista de observadores suscritos (el RegistroCambios de la consola).
    private final List<ObservadorRepositorio> observadores = new CopyOnWriteArrayList<>();

    // Version de los datos. AtomicInteger permite aumentarla sin candado.
    private final AtomicInteger version = new AtomicInteger(1);

    private RepositorioMemoria() {
        cargarDatosDeEjemplo();
    }

    /** Unica puerta de entrada a la instancia (Singleton). */
    public static synchronized RepositorioMemoria getInstance() {
        if (instancia == null) {
            instancia = new RepositorioMemoria();
        }
        return instancia;
    }

    /** Suscribe un observador para que reciba los avisos de cambio. */
    public void agregarObservador(ObservadorRepositorio observador) {
        observadores.add(observador);
    }

    @Override
    public Object candado() {
        return candado;
    }

    @Override
    public List<Lavadora> lavadoras() {
        return lavadoras;
    }

    @Override
    public int version() {
        return version.get();
    }

    @Override
    public void notificar(String evento) {
        version.incrementAndGet(); // Los clientes ven que algo cambio
        for (ObservadorRepositorio observador : observadores) {
            observador.alCambiar(evento);
        }
    }

    // ==================================================================
    //  Datos de ejemplo que ya existen cuando arranca el servidor
    // ==================================================================

    private void cargarDatosDeEjemplo() {
        // Lavadora 101: tres mantenimientos (detalle con varios hijos).
        Lavadora lg = crearLavadora(101, "LG", 1450000, LocalDateTime.of(2024, 3, 10, 9, 0),
                18.0, 1400, true, 45);
        agregarMantenimiento(lg, 1, "Cambio de bomba de agua", "Carlos Perez",
                185000, LocalDateTime.of(2024, 6, 12, 10, 30), true);
        agregarMantenimiento(lg, 2, "Revision de rodamientos", "Laura Gomez",
                95000, LocalDateTime.of(2025, 2, 3, 15, 0), true);
        agregarMantenimiento(lg, 3, "Limpieza de filtro", "Carlos Perez",
                45000, LocalDateTime.of(2025, 7, 20, 9, 15), false);

        // Lavadora 102: un solo mantenimiento.
        Lavadora samsung = crearLavadora(102, "Samsung", 1290000, LocalDateTime.of(2023, 11, 22, 14, 30),
                16.0, 1200, false, 30);
        agregarMantenimiento(samsung, 1, "Calibracion de tambor", "Andres Ruiz",
                120000, LocalDateTime.of(2024, 1, 15, 11, 0), true);

        // Lavadora 103: CERO mantenimientos. Esto demuestra el lado 0..* de la relacion.
        crearLavadora(103, "Whirlpool", 980000, LocalDateTime.of(2025, 1, 5, 8, 15),
                14.0, 1000, false, 60);

        // Lavadora 104: dos mantenimientos, uno todavia pendiente.
        Lavadora lgNueva = crearLavadora(104, "LG", 1650000, LocalDateTime.of(2025, 6, 18, 10, 45),
                20.0, 1600, true, 90);
        agregarMantenimiento(lgNueva, 1, "Instalacion y nivelacion", "Diana Torres",
                80000, LocalDateTime.of(2025, 6, 20, 8, 0), true);
        agregarMantenimiento(lgNueva, 2, "Revision de mangueras", "Diana Torres",
                60000, LocalDateTime.of(2025, 7, 1, 16, 45), false);
    }

    /** Crea una lavadora con el patron Builder y la guarda en la lista. */
    private Lavadora crearLavadora(int codigo, String marca, double precio, LocalDateTime fecha,
                                   double kilos, int rpm, boolean inverter, int minutos) {
        Lavadora lavadora = Lavadora.builder()
                .codigo(codigo)
                .marca(marca)
                .precioBase(precio)
                .fechaFabricacion(fecha)
                .capacidadCargaKilos(kilos)
                .velocidadCentrifugadoRPM(rpm)
                .tecnologiaInverter(inverter)
                .minutosLavado(minutos)
                .build();
        lavadoras.add(lavadora);
        return lavadora;
    }

    /** Crea un mantenimiento con el patron Builder y lo agrega a su lavadora. */
    private void agregarMantenimiento(Lavadora lavadora, int id, String descripcion, String tecnico,
                                      double costo, LocalDateTime fecha, boolean completado) {
        Mantenimiento mantenimiento = Mantenimiento.builder()
                .id(id)
                .descripcion(descripcion)
                .tecnico(tecnico)
                .costo(costo)
                .fecha(fecha)
                .completado(completado)
                .build();
        lavadora.agregarMantenimiento(mantenimiento);
    }
}
