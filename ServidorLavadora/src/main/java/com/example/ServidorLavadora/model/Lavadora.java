package com.example.ServidorLavadora.model;

import java.time.LocalDateTime;

/**
 * Objeto DETALLE del proyecto: la lavadora. Hija de Electrodomestico.
 * Hereda codigo, marca, precioBase (double) y fechaFabricacion (LocalDateTime)
 * y agrega lo suyo:
 *  - capacidadCargaKilos (double)
 *  - velocidadCentrifugadoRPM (int)
 *  - tecnologiaInverter (boolean)
 *  - minutosLavado (int)
 *  - programaLavado: el programa (maestro) que usa esta lavadora
 *
 * Se crea con el patron Builder: Lavadora.builder().codigo(101).marca("LG")...build()
 */
public class Lavadora extends Electrodomestico {
    private double capacidadCargaKilos;
    private int velocidadCentrifugadoRPM;
    private boolean tecnologiaInverter;
    private int minutosLavado;
    private ProgramaLavado programaLavado;

    private Lavadora(Builder b) {
        super(b.codigo, b.marca, b.precioBase, b.fechaFabricacion);
        this.capacidadCargaKilos = b.capacidadCargaKilos;
        this.velocidadCentrifugadoRPM = b.velocidadCentrifugadoRPM;
        this.tecnologiaInverter = b.tecnologiaInverter;
        this.minutosLavado = b.minutosLavado;
        this.programaLavado = b.programaLavado;
    }

    public static Builder builder() {
        return new Builder();
    }

    /**
     * Reglas del proyecto en Java:
     * +120.000 si carga mas de 15 kg y +250.000 si tiene inverter.
     */
    @Override
    public double calcularPrecioFinal() {
        double precio = getPrecioBase();
        if (capacidadCargaKilos > 15) {
            precio += 120000;
        }
        if (tecnologiaInverter) {
            precio += 250000;
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

    public ProgramaLavado getProgramaLavado() {
        return programaLavado;
    }

    public void setProgramaLavado(ProgramaLavado programaLavado) {
        this.programaLavado = programaLavado;
    }

    /** Patron BUILDER: la lavadora tiene 9 datos, el Builder evita un constructor enorme. */
    public static class Builder {
        private int codigo;
        private String marca;
        private double precioBase;
        private LocalDateTime fechaFabricacion;
        private double capacidadCargaKilos;
        private int velocidadCentrifugadoRPM;
        private boolean tecnologiaInverter;
        private int minutosLavado;
        private ProgramaLavado programaLavado;

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

        public Builder programaLavado(ProgramaLavado programaLavado) {
            this.programaLavado = programaLavado;
            return this;
        }

        public Lavadora build() {
            return new Lavadora(this);
        }
    }
}
