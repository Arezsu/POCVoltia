package com.example.ServidorLavadora.repositorio;

import com.example.ServidorLavadora.model.Lavadora;
import com.example.ServidorLavadora.model.Mantenimiento;
import org.springframework.data.mongodb.core.MongoTemplate;
import org.springframework.data.mongodb.core.query.Criteria;
import org.springframework.data.mongodb.core.query.Query;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.atomic.AtomicInteger;
import java.util.regex.Pattern;
import java.util.stream.Collectors;

/** Acceso a las colecciones MongoDB con criterios tipados y consultas parametrizadas. */
public class RepositorioMongo implements AlmacenDatos {

    private final Object candado = new Object();
    private final MongoTemplate mongoTemplate;
    private final List<ObservadorRepositorio> observadores = new CopyOnWriteArrayList<>();
    private final AtomicInteger version = new AtomicInteger(1);

    public RepositorioMongo(MongoTemplate mongoTemplate) {
        this.mongoTemplate = mongoTemplate;
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
        return cargarLavadoras(new Query());
    }

    @Override
    public List<Lavadora> listarPorFiltro(String marca, Boolean tecnologiaInverter, Boolean conMantenimientos) {
        List<Criteria> criterios = new ArrayList<>();
        if (marca != null && !marca.isBlank()) {
            criterios.add(Criteria.where("marca").regex(Pattern.quote(marca.trim()), "i"));
        }
        if (tecnologiaInverter != null) {
            criterios.add(Criteria.where("tecnologiaInverter").is(tecnologiaInverter));
        }
        if (conMantenimientos != null) {
            List<Integer> codigosConDetalle = mongoTemplate.findDistinct(
                    new Query(), "codigoLavadora", Mantenimiento.class, Integer.class);
            if (conMantenimientos && codigosConDetalle.isEmpty()) {
                return List.of();
            }
            if (!codigosConDetalle.isEmpty()) {
                Criteria relacion = Criteria.where("codigo");
                criterios.add(conMantenimientos
                        ? relacion.in(codigosConDetalle)
                        : relacion.nin(codigosConDetalle));
            }
        }

        Query consulta = new Query();
        if (!criterios.isEmpty()) {
            consulta.addCriteria(new Criteria().andOperator(criterios.toArray(Criteria[]::new)));
        }
        return cargarLavadoras(consulta);
    }

    @Override
    public Optional<Lavadora> buscarPorCodigo(int codigo) {
        List<Lavadora> coincidencias = cargarLavadoras(
                Query.query(Criteria.where("codigo").is(codigo)));
        return coincidencias.stream().findFirst();
    }

    @Override
    public Optional<Lavadora> buscarPorCriterios(Integer codigo, String marca) {
        List<Criteria> criterios = new ArrayList<>();
        if (codigo != null) {
            criterios.add(Criteria.where("codigo").is(codigo));
        }
        if (marca != null && !marca.isBlank()) {
            criterios.add(Criteria.where("marca").regex(Pattern.quote(marca.trim()), "i"));
        }
        Query consulta = new Query();
        if (criterios.size() == 1) {
            consulta.addCriteria(criterios.get(0));
        } else if (!criterios.isEmpty()) {
            consulta.addCriteria(new Criteria().andOperator(criterios.toArray(Criteria[]::new)));
        }
        return cargarLavadoras(consulta).stream().findFirst();
    }

    @Override
    public Lavadora guardar(Lavadora lavadora) {
        Lavadora guardada = mongoTemplate.save(lavadora, "lavadoras");
        List<Mantenimiento> actuales = guardada.getMantenimientos();
        for (Mantenimiento mantenimiento : actuales) {
            mantenimiento.setCodigoLavadora(guardada.getCodigo());
            mongoTemplate.save(mantenimiento, "mantenimientos");
        }

        Query filtroDetalle = Query.query(Criteria.where("codigoLavadora").is(guardada.getCodigo()));
        if (!actuales.isEmpty()) {
            filtroDetalle.addCriteria(Criteria.where("id")
                    .nin(actuales.stream().map(Mantenimiento::getId).toList()));
        }
        mongoTemplate.remove(filtroDetalle, Mantenimiento.class, "mantenimientos");
        return guardada;
    }

    @Override
    public boolean eliminar(int codigo) {
        mongoTemplate.remove(Query.query(Criteria.where("codigoLavadora").is(codigo)),
                Mantenimiento.class, "mantenimientos");
        return mongoTemplate.remove(Query.query(Criteria.where("codigo").is(codigo)),
                Lavadora.class, "lavadoras").getDeletedCount() > 0;
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

    private List<Lavadora> cargarLavadoras(Query consultaLavadoras) {
        List<Lavadora> lavadoras = mongoTemplate.find(consultaLavadoras, Lavadora.class, "lavadoras");
        if (lavadoras.isEmpty()) {
            return lavadoras;
        }

        List<Integer> codigos = lavadoras.stream().map(Lavadora::getCodigo).toList();
        Query consultaDetalle = Query.query(Criteria.where("codigoLavadora").in(codigos));
        List<Mantenimiento> mantenimientos = mongoTemplate.find(
                consultaDetalle, Mantenimiento.class, "mantenimientos");
        Map<Integer, List<Mantenimiento>> porLavadora = mantenimientos.stream()
                .collect(Collectors.groupingBy(Mantenimiento::getCodigoLavadora));

        for (Lavadora lavadora : lavadoras) {
            List<Mantenimiento> detalle = porLavadora.getOrDefault(lavadora.getCodigo(), List.of());
            detalle.forEach(mantenimiento -> mantenimiento.setMarcaLavadora(lavadora.getMarca()));
            lavadora.setMantenimientos(detalle);
        }
        return lavadoras;
    }
}
