package com.example.ServidorLavadora.model;

import org.bson.types.ObjectId;
import org.springframework.data.annotation.Id;
import org.springframework.data.mongodb.core.mapping.Document;

import java.time.LocalDateTime;

/**
 * Objeto MAESTRO del proyecto (lado 1..1 de la Figura 1) e hija de Electrodomestico.
 *
 * La lavadora es el objeto principal. Los mantenimientos se guardan en su propia
 * coleccion y se relacionan por codigoLavadora; no se representa la relacion con
 * un atributo tipo lista dentro de esta clase.
 */
@Document(collection = "lavadoras")
public class Lavadora extends Electrodomestico {

    private static final double BONO_CAPACIDAD = 120000;
    private static final double BONO_INVERTER = 250000;
    private static final double KILOS_QUE_ACTIVAN_EL_BONO = 15;

    @Id
    private ObjectId mongoId;

    private double capacidadCargaKilos;
    private int velocidadCentrifugadoRPM;
    private boolean tecnologiaInverter;
    private int minutosLavado;

    public Lavadora() {
    }

    private Lavadora(Builder builder) {
        super(builder.codigo, builder.marca, builder.precioBase, builder.fechaFabricacion);
        this.capacidadCargaKilos = builder.capacidadCargaKilos;
        this.velocidadCentrifugadoRPM = builder.velocidadCentrifugadoRPM;
        this.tecnologiaInverter = builder.tecnologiaInverter;
        this.minutosLavado = builder.minutosLavado;
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

    @Override
    public double calcularPrecioFinal() {
        double precio = getPrecioBase();
        if (capacidadCargaKilos > KILOS_QUE_ACTIVAN_EL_BONO) {
            precio += BONO_CAPACIDAD;
        }
        if (tecnologiaInverter) {
            precio += BONO_INVERTER;
        }
        return precio;
    }

    public double getCapacidadCargaKilos() {
        return capacidadCargaKilos;
    }

    public void setCapacidadCargaKilos(double capacidadCargaKilos) {
        this.capacidadCargaKilos = capacidadCargaKilos;
    }

    public int getVelocidadCentrifugadoRPM() {
        return velocidadCentrifugadoRPM;
    }

    public void setVelocidadCentrifugadoRPM(int velocidadCentrifugadoRPM) {
        this.velocidadCentrifugadoRPM = velocidadCentrifugadoRPM;
    }

    public boolean isTecnologiaInverter() {
        return tecnologiaInverter;
    }

    public void setTecnologiaInverter(boolean tecnologiaInverter) {
        this.tecnologiaInverter = tecnologiaInverter;
    }

    public int getMinutosLavado() {
        return minutosLavado;
    }

    public void setMinutosLavado(int minutosLavado) {
        this.minutosLavado = minutosLavado;
    }

    public static class Builder {
        private int codigo;
        private String marca;
        private double precioBase;
        private LocalDateTime fechaFabricacion;
        private double capacidadCargaKilos;
        private int velocidadCentrifugadoRPM;
        private boolean tecnologiaInverter;
        private int minutosLavado;

        public Builder codigo(int codigo) {
            this.codigo = codigo;
            return this;
        }

        public Builder marca(String marca) {
            this.marca = marca;
            return this;
        }

        public Builder precioBase(double precioBase) {
            this.precioBase = precioBase;
            return this;
        }

        public Builder fechaFabricacion(LocalDateTime fechaFabricacion) {
            this.fechaFabricacion = fechaFabricacion;
            return this;
        }

        public Builder capacidadCargaKilos(double capacidadCargaKilos) {
            this.capacidadCargaKilos = capacidadCargaKilos;
            return this;
        }

        public Builder velocidadCentrifugadoRPM(int velocidadCentrifugadoRPM) {
            this.velocidadCentrifugadoRPM = velocidadCentrifugadoRPM;
            return this;
        }

        public Builder tecnologiaInverter(boolean tecnologiaInverter) {
            this.tecnologiaInverter = tecnologiaInverter;
            return this;
        }

        public Builder minutosLavado(int minutosLavado) {
            this.minutosLavado = minutosLavado;
            return this;
        }

        public Lavadora build() {
            return new Lavadora(this);
        }
    }
}
