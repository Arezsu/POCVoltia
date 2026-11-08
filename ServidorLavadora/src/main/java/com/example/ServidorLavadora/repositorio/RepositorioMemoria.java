package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;
import java.util.List;
import java.util.Optional;
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
    public List<Lavadora> listarPorFiltro(String marca, Boolean tecnologiaInverter, Boolean conMantenimientos) {
        String texto = marca == null ? "" : marca.trim().toLowerCase(java.util.Locale.ROOT);
        return lavadoras.stream()
                .filter(lavadora -> texto.isEmpty()
                        || lavadora.getMarca().toLowerCase(java.util.Locale.ROOT).contains(texto))
                .filter(lavadora -> tecnologiaInverter == null
                        || lavadora.isTecnologiaInverter() == tecnologiaInverter)
                .filter(lavadora -> conMantenimientos == null
                        || (lavadora.getCantidadMantenimientos() > 0) == conMantenimientos)
                .toList();
    }

    @Override
    public Optional<Lavadora> buscarPorCodigo(int codigo) {
        return lavadoras.stream().filter(lavadora -> lavadora.getCodigo() == codigo).findFirst();
    }

    @Override
    public Optional<Lavadora> buscarPorCriterios(Integer codigo, String marca) {
        String texto = marca == null ? "" : marca.trim().toLowerCase(java.util.Locale.ROOT);
        return lavadoras.stream()
                .filter(lavadora -> codigo == null || lavadora.getCodigo() == codigo)
                .filter(lavadora -> texto.isEmpty()
                        || lavadora.getMarca().toLowerCase(java.util.Locale.ROOT).contains(texto))
                .findFirst();
    }

    @Override
    public Lavadora guardar(Lavadora lavadora) {
        if (!lavadoras.contains(lavadora)) {
            lavadoras.add(lavadora);
        }
        return lavadora;
    }

    @Override
    public boolean eliminar(int codigo) {
        return lavadoras.removeIf(lavadora -> lavadora.getCodigo() == codigo);
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
