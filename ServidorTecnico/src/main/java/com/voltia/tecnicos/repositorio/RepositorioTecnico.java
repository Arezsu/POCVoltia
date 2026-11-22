package com.voltia.tecnicos.repositorio;

import com.voltia.tecnicos.model.Tecnico;
import java.util.List;
import java.util.Optional;
import org.springframework.data.mongodb.repository.MongoRepository;

public interface RepositorioTecnico extends MongoRepository<Tecnico, String> {
    Optional<Tecnico> findByCodigo(int codigo);
    List<Tecnico> findAllByOrderByCodigoAsc();
    boolean existsByCodigo(int codigo);
    boolean existsByDocumento(String documento);
    boolean existsByDocumentoAndCodigoNot(String documento, int codigo);
}
