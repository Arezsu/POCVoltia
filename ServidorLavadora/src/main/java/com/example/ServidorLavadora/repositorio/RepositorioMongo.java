package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.Mantenimiento;
import org.springframework.data.mongodb.core.MongoTemplate;

import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.atomic.AtomicInteger;

public class RepositorioMongo implements AlmacenDatos {

    private final Object candado = new Object();
    private final MongoTemplate mongoTemplate;
    private final List<ObservadorRepositorio> observadores = new CopyOnWriteArrayList<>();
    private final AtomicInteger version = new AtomicInteger(1);

    public RepositorioMongo(MongoTemplate mongoTemplate) {
        this.mongoTemplate = mongoTemplate;
    }

    public void cargarDatosPruebaSiVacio() {
        synchronized (candado) {
            if (mongoTemplate.collectionExists("lavadoras") && mongoTemplate.count(null, "lavadoras") > 0) {
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

            mongoTemplate.insert(List.of(lg, samsung), "lavadoras");
        }
        notificar("Se cargaron dos lavadoras y dos mantenimientos de prueba");
    }

    public void agregarObservador(ObservadorRepositorio observador) {
        observadores.add(observador);
    }

    @Override
    public Object candado() {
        return candado;
    }

    @Override
    public List<Lavadora> lavadoras() {
        return mongoTemplate.findAll(Lavadora.class, "lavadoras");
    }

    @Override
    public void notificar(String evento) {
        version.incrementAndGet();
        for (ObservadorRepositorio observador : observadores) {
            observador.alCambiar(evento);
        }
    }

    @Override
    public int version() {
        return version.get();
    }
}
