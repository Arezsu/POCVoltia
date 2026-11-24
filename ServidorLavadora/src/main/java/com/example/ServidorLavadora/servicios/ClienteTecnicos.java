package com.example.ServidorLavadora.servicios;

import com.example.ServidorLavadora.model.TecnicoDto;
import com.example.ServidorLavadora.model.TecnicoInput;
import java.util.List;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.graphql.client.HttpSyncGraphQlClient;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestClient;

/** Cliente GraphQL del microservicio independiente de técnicos. */
@Component
public class ClienteTecnicos {
    private static final String CAMPOS = "id codigo documento nombre especialidad telefono fechaIngreso activo";
    private final HttpSyncGraphQlClient graphql;

    public ClienteTecnicos(@Value("${voltia.tecnicos.graphql-url:http://localhost:8082/graphql}") String url) {
        // RestClient solo es el transporte HTTP usado internamente por el cliente GraphQL síncrono.
        // Las operaciones enviadas aquí son documentos GraphQL al endpoint /graphql.
        this.graphql = HttpSyncGraphQlClient.create(RestClient.create(url));
    }

    public List<TecnicoDto> listar(String texto, String especialidad, Boolean activo) {
        return graphql.document("query Tecnicos($texto:String,$especialidad:String,$activo:Boolean) { " +
                        "tecnicos(texto:$texto,especialidad:$especialidad,activo:$activo) { " + CAMPOS + " } }")
                .variable("texto", texto)
                .variable("especialidad", especialidad)
                .variable("activo", activo)
                .retrieveSync("tecnicos")
                .toEntityList(TecnicoDto.class);
    }

    public TecnicoDto buscar(int codigo) {
        return graphql.document("query TecnicoPorCodigo($codigo:Int!) { tecnicoPorCodigo(codigo:$codigo) { " + CAMPOS + " } }")
                .variable("codigo", codigo)
                .retrieveSync("tecnicoPorCodigo")
                .toEntity(TecnicoDto.class);
    }

    public TecnicoDto crear(TecnicoInput input) {
        return graphql.document("mutation AgregarTecnico($input:TecnicoInput!) { addTecnico(input:$input) { " + CAMPOS + " } }")
                .variable("input", input)
                .retrieveSync("addTecnico")
                .toEntity(TecnicoDto.class);
    }

    public TecnicoDto actualizar(int codigo, TecnicoInput input) {
        return graphql.document("mutation ActualizarTecnico($codigo:Int!,$input:TecnicoInput!) { " +
                        "actualizarTecnico(codigo:$codigo,input:$input) { " + CAMPOS + " } }")
                .variable("codigo", codigo)
                .variable("input", input)
                .retrieveSync("actualizarTecnico")
                .toEntity(TecnicoDto.class);
    }

    public void eliminar(int codigo) {
        graphql.document("mutation EliminarTecnico($codigo:Int!) { eliminarTecnico(codigo:$codigo) }")
                .variable("codigo", codigo)
                .retrieveSync("eliminarTecnico")
                .toEntity(Boolean.class);
    }
}
