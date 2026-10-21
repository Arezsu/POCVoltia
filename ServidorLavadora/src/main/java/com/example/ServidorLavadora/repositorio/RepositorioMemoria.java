package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.Mantenimiento;
import java.util.List;
import java.time.LocalDateTime;
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

    private RepositorioMemoria() { }

    /** Carga dos maestros y dos detalles de demostracion solo si el almacen esta vacio. */
    public void cargarDatosPruebaSiVacio() {
        synchronized (candado) {
            if (!lavadoras.isEmpty()) {
                return;
            }

            Lavadora lg = Lavadora.builder()
                    .codigo(901)
                    .marca("LG")
                    .precioBase(1450000)
                    .fechaFabricacion(LocalDateTime.of(2024, 3, 10, 9, 0))
                    .capacidadCargaKilos(18)
                    .velocidadCentrifugadoRPM(1400)
                    .tecnologiaInverter(true)
                    .minutosLavado(45)
                    .build();
            lg.agregarMantenimiento(Mantenimiento.builder()
                    .id(1)
                    .descripcion("Revision de bomba de agua")
                    .tecnico("Carlos Perez")
                    .costo(185000)
                    .fecha(LocalDateTime.of(2025, 6, 12, 10, 30))
                    .completado(false)
                    .build());

            Lavadora samsung = Lavadora.builder()
                    .codigo(902)
                    .marca("Samsung")
                    .precioBase(1100000)
                    .fechaFabricacion(LocalDateTime.of(2023, 11, 5, 14, 0))
                    .capacidadCargaKilos(12)
                    .velocidadCentrifugadoRPM(1200)
                    .tecnologiaInverter(false)
                    .minutosLavado(60)
                    .build();
            samsung.agregarMantenimiento(Mantenimiento.builder()
                    .id(1)
                    .descripcion("Limpieza de filtro y mangueras")
                    .tecnico("Laura Gomez")
                    .costo(95000)
                    .fecha(LocalDateTime.of(2025, 8, 20, 15, 0))
                    .completado(true)
                    .build());

            lavadoras.add(lg);
            lavadoras.add(samsung);
        }
        notificar("Se cargaron dos lavadoras y dos mantenimientos de prueba");
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

}
