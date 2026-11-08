package com.example.ServidorLavadora.model;

import com.fasterxml.jackson.annotation.JsonFormat;
import org.bson.types.ObjectId;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;
import org.springframework.data.mongodb.core.index.CompoundIndex;
import org.springframework.data.annotation.Transient;

import java.time.LocalDateTime;

/**
 * Objeto DETALLE del proyecto (lado 0..* de la Figura 1: Maestro - Detalle).
 *
 * Un mantenimiento SIEMPRE pertenece a una lavadora concreta: no existe por
 * fuera de ella. Por eso la lavadora es el maestro (1..1) y el mantenimiento
 * es el detalle (0..*). Una lavadora puede tener cero, uno o muchos.
 */
@Document(collection = "mantenimientos")
@CompoundIndex(name = "uq_mantenimiento_lavadora_id", def = "{'codigoLavadora': 1, 'id': 1}", unique = true)
public class Mantenimiento {

    @Id
    private ObjectId mongoId;

    private int id;
    private int codigoLavadora;
    @Transient
    private String marcaLavadora;
    private String descripcion;
    private String tecnico;
    private double costo;
    private LocalDateTime fecha;
    private boolean completado;

    public Mantenimiento() {
    }

    private Mantenimiento(Builder builder) {
        this.id = builder.id;
        this.descripcion = builder.descripcion;
        this.tecnico = builder.tecnico;
        this.costo = builder.costo;
        this.fecha = builder.fecha;
        this.completado = builder.completado;
    }

    public static Builder builder() {
        return new Builder();
    }

    public ObjectId getMongoId() {
        return mongoId;
    }

    public void setMongoId(ObjectId mongoId) {
        this.mongoId = mongoId;
    }

    public int getId() {
        return id;
    }

    public int getCodigoLavadora() {
        return codigoLavadora;
    }

    public void setCodigoLavadora(int codigoLavadora) {
        this.codigoLavadora = codigoLavadora;
    }

    public String getMarcaLavadora() {
        return marcaLavadora;
    }

    public void setMarcaLavadora(String marcaLavadora) {
        this.marcaLavadora = marcaLavadora;
    }

    public void setId(int id) {
        this.id = id;
    }

    public String getDescripcion() {
        return descripcion;
    }

    public void setDescripcion(String descripcion) {
        this.descripcion = descripcion;
    }

    public String getTecnico() {
        return tecnico;
    }

    public void setTecnico(String tecnico) {
        this.tecnico = tecnico;
    }

    public double getCosto() {
        return costo;
    }

    public void setCosto(double costo) {
        this.costo = costo;
    }

    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss")
    public LocalDateTime getFecha() {
        return fecha;
    }

    public void setFecha(LocalDateTime fecha) {
        this.fecha = fecha;
    }

    public boolean isCompletado() {
        return completado;
    }

    public void setCompletado(boolean completado) {
        this.completado = completado;
    }

    public static class Builder {
        private int id;
        private String descripcion;
        private String tecnico;
        private double costo;
        private LocalDateTime fecha;
        private boolean completado;

        public Builder id(int id) {
            this.id = id;
            return this;
        }

        public Builder descripcion(String descripcion) {
            this.descripcion = descripcion;
            return this;
        }

        public Builder tecnico(String tecnico) {
            this.tecnico = tecnico;
            return this;
        }

        public Builder costo(double costo) {
            this.costo = costo;
            return this;
        }

        public Builder fecha(LocalDateTime fecha) {
            this.fecha = fecha;
            return this;
        }

        public Builder completado(boolean completado) {
            this.completado = completado;
            return this;
        }

        public Mantenimiento build() {
            return new Mantenimiento(this);
        }
    }
}
