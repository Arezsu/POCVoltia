package com.example.ServidorLavadora.model;

import java.time.LocalDateTime;
import com.fasterxml.jackson.annotation.JsonFormat;

/**
 * Objeto DETALLE del proyecto (lado 0..* de la Figura 1: Maestro - Detalle).
 *
 * Un mantenimiento SIEMPRE pertenece a una lavadora concreta: no existe por
 * fuera de ella. Por eso la lavadora es el maestro (1..1) y el mantenimiento
 * es el detalle (0..*). Una lavadora puede tener cero, uno o muchos.
 *
 * Atributos (seis, con tipos variados, como pide la rubrica):
 *  - id (int): numero que identifica al mantenimiento DENTRO de su lavadora
 *  - descripcion (String): que trabajo se hizo o se va a hacer
 *  - tecnico (String): persona responsable del trabajo
 *  - costo (double): valor del mantenimiento
 *  - fecha (LocalDateTime): cuando se realizo o se agendo
 *  - completado (boolean): true si el trabajo ya termino
 *
 * Se construye con el patron Builder, igual que la lavadora:
 * Mantenimiento.builder().id(1).descripcion("...").build()
 */
public class Mantenimiento {

    private final int id;
    private String descripcion;
    private String tecnico;
    private double costo;
    private LocalDateTime fecha;
    private boolean completado;

    // El constructor es privado: solo el Builder puede crear mantenimientos.
    private Mantenimiento(Builder builder) {
        this.id = builder.id;
        this.descripcion = builder.descripcion;
        this.tecnico = builder.tecnico;
        this.costo = builder.costo;
        this.fecha = builder.fecha;
        this.completado = builder.completado;
    }

    /** Punto de entrada del patron Builder. */
    public static Builder builder() {
        return new Builder();
    }

    public int getId() {
        return id;
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

    /**
     * Patron BUILDER: arma el mantenimiento paso a paso y sin un constructor
     * con seis parametros seguidos (que es facil de confundir al escribir).
     */
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
