package com.example.ServidorLavadora.servicios;

import com.example.ServidorLavadora.model.TecnicoDto;
import com.example.ServidorLavadora.model.TecnicoInput;
import java.util.Arrays;
import java.util.List;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestClient;
import org.springframework.web.client.RestClientResponseException;

/** Adaptador HTTP: el microservicio GraphQL oculta a sus clientes que Tecnico vive en otro servicio. */
@Component
public class ClienteTecnicos {
    private final RestClient http;
    public ClienteTecnicos(@Value("${voltia.tecnicos.url:http://localhost:8082}") String url) {
        this.http = RestClient.builder().baseUrl(url).build();
    }
    public List<TecnicoDto> listar(String texto, String especialidad, Boolean activo) {
        TecnicoDto[] datos = http.get().uri(uri -> uri.path("/tecnicos")
                .queryParamIfPresent("texto", java.util.Optional.ofNullable(texto).filter(s -> !s.isBlank()))
                .queryParamIfPresent("especialidad", java.util.Optional.ofNullable(especialidad).filter(s -> !s.isBlank()))
                .queryParamIfPresent("activo", java.util.Optional.ofNullable(activo)).build())
                .retrieve().body(TecnicoDto[].class);
        return datos == null ? List.of() : Arrays.asList(datos);
    }
    public TecnicoDto buscar(int codigo) {
        try { return http.get().uri("/tecnicos/{codigo}", codigo).retrieve().body(TecnicoDto.class); }
        catch (RestClientResponseException ex) { throw error(ex); }
    }
    public TecnicoDto crear(TecnicoInput input) {
        try { return http.post().uri("/tecnicos").body(input).retrieve().body(TecnicoDto.class); }
        catch (RestClientResponseException ex) { throw error(ex); }
    }
    public TecnicoDto actualizar(int codigo, TecnicoInput input) {
        try { return http.put().uri("/tecnicos/{codigo}", codigo).body(input).retrieve().body(TecnicoDto.class); }
        catch (RestClientResponseException ex) { throw error(ex); }
    }
    public void eliminar(int codigo) {
        try { http.delete().uri("/tecnicos/{codigo}", codigo).retrieve().toBodilessEntity(); }
        catch (RestClientResponseException ex) { throw error(ex); }
    }
    private IllegalArgumentException error(RestClientResponseException ex) {
        String cuerpo = ex.getResponseBodyAsString();
        return new IllegalArgumentException(cuerpo.isBlank() ? "No fue posible comunicarse con el servicio de tecnicos." : cuerpo);
    }
}
