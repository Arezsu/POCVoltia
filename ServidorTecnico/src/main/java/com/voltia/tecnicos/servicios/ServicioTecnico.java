package com.voltia.tecnicos.servicios;

import com.voltia.tecnicos.model.Tecnico;
import com.voltia.tecnicos.model.TecnicoInput;
import com.voltia.tecnicos.repositorio.RepositorioTecnico;
import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;
import java.time.format.DateTimeParseException;
import java.util.List;
import java.util.Locale;
import org.springframework.stereotype.Service;

@Service
public class ServicioTecnico {
    private static final DateTimeFormatter FECHA = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss");
    private final RepositorioTecnico repositorio;
    public ServicioTecnico(RepositorioTecnico repositorio) { this.repositorio = repositorio; }

    public List<Tecnico> listar(String texto, String especialidad, Boolean activo) {
        String busqueda = texto == null ? "" : texto.trim().toLowerCase(Locale.ROOT);
        String especialidadBusqueda = especialidad == null ? "" : especialidad.trim().toLowerCase(Locale.ROOT);
        return repositorio.findAllByOrderByCodigoAsc().stream()
                .filter(t -> busqueda.isEmpty() || t.getNombre().toLowerCase(Locale.ROOT).contains(busqueda)
                        || t.getDocumento().toLowerCase(Locale.ROOT).contains(busqueda))
                .filter(t -> especialidadBusqueda.isEmpty() || t.getEspecialidad().toLowerCase(Locale.ROOT).contains(especialidadBusqueda))
                .filter(t -> activo == null || t.isActivo() == activo)
                .toList();
    }
    public Tecnico buscar(int codigo) { return repositorio.findByCodigo(codigo).orElseThrow(() ->
            new IllegalArgumentException("No existe un tecnico con ese codigo.")); }
    public Tecnico buscarSiExiste(int codigo) { return repositorio.findByCodigo(codigo).orElse(null); }
    public Tecnico crear(TecnicoInput input) {
        validar(input);
        if (repositorio.existsByCodigo(input.codigo())) throw new IllegalArgumentException("Ya existe un tecnico con ese codigo.");
        if (repositorio.existsByDocumento(input.documento().trim())) throw new IllegalArgumentException("Ya existe un tecnico con ese documento.");
        return repositorio.save(convertir(input));
    }
    public Tecnico actualizar(int codigo, TecnicoInput input) {
        Tecnico actual = buscar(codigo); validar(input);
        if (input.codigo() != codigo) throw new IllegalArgumentException("El codigo no se puede cambiar.");
        if (repositorio.existsByDocumentoAndCodigoNot(input.documento().trim(), codigo)) throw new IllegalArgumentException("Ya existe otro tecnico con ese documento.");
        Tecnico nuevo = convertir(input); nuevo.setId(actual.getId()); return repositorio.save(nuevo);
    }
    public void eliminar(int codigo) { repositorio.delete(buscar(codigo)); }

    private void validar(TecnicoInput i) {
        if (i == null || i.codigo() < 1) throw new IllegalArgumentException("El codigo debe ser positivo.");
        requerido(i.documento(), "documento"); requerido(i.nombre(), "nombre"); requerido(i.especialidad(), "especialidad"); requerido(i.telefono(), "telefono");
        if (i.activo() == null) throw new IllegalArgumentException("Indique si el tecnico esta activo.");
        try { LocalDateTime.parse(i.fechaIngreso(), FECHA); }
        catch (DateTimeParseException | NullPointerException ex) { throw new IllegalArgumentException("La fecha debe tener formato yyyy-MM-ddTHH:mm:ss."); }
    }
    private void requerido(String s, String nombre) { if (s == null || s.isBlank()) throw new IllegalArgumentException("El campo " + nombre + " es obligatorio."); }
    private Tecnico convertir(TecnicoInput i) { return new Tecnico(i.codigo(), i.documento().trim(), i.nombre().trim(), i.especialidad().trim(), i.telefono().trim(), LocalDateTime.parse(i.fechaIngreso(), FECHA), i.activo()); }
}
