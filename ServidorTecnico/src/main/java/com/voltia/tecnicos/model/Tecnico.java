package com.voltia.tecnicos.model;

import java.time.LocalDateTime;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.index.Indexed;
import org.springframework.data.mongodb.core.mapping.Document;

@Document(collection = "tecnicos")
public class Tecnico {
    @Id
    private String id;
    @Indexed(unique = true)
    private int codigo;
    @Indexed(unique = true)
    private String documento;
    private String nombre;
    private String especialidad;
    private String telefono;
    private LocalDateTime fechaIngreso;
    private boolean activo;

    public Tecnico() { }
    public Tecnico(int codigo, String documento, String nombre, String especialidad,
                   String telefono, LocalDateTime fechaIngreso, boolean activo) {
        this.codigo = codigo; this.documento = documento; this.nombre = nombre;
        this.especialidad = especialidad; this.telefono = telefono;
        this.fechaIngreso = fechaIngreso; this.activo = activo;
    }
    public String getId() { return id; }
    public void setId(String id) { this.id = id; }
    public int getCodigo() { return codigo; }
    public void setCodigo(int codigo) { this.codigo = codigo; }
    public String getDocumento() { return documento; }
    public void setDocumento(String documento) { this.documento = documento; }
    public String getNombre() { return nombre; }
    public void setNombre(String nombre) { this.nombre = nombre; }
    public String getEspecialidad() { return especialidad; }
    public void setEspecialidad(String especialidad) { this.especialidad = especialidad; }
    public String getTelefono() { return telefono; }
    public void setTelefono(String telefono) { this.telefono = telefono; }
    public LocalDateTime getFechaIngreso() { return fechaIngreso; }
    public void setFechaIngreso(LocalDateTime fechaIngreso) { this.fechaIngreso = fechaIngreso; }
    public boolean isActivo() { return activo; }
    public void setActivo(boolean activo) { this.activo = activo; }
}
