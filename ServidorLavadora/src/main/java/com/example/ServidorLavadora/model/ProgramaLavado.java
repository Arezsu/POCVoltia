package com.example.ServidorLavadora.model;

import java.time.LocalDateTime;

/**
 * Objeto MAESTRO del proyecto: el programa de lavado.
 * Un programa se usa en muchas lavadoras (0..*) y cada lavadora tiene un solo programa (1..1).
 *
 * Atributos:
 *  - codigo (int)
 *  - nombre (String)
 *  - minMinutos y maxMinutos (int): rango de minutos que cubre el programa
 *  - consumoAguaLitros (double)
 *  - fechaCreacion (LocalDateTime)
 *  - aguaCaliente (boolean)
 *
 * Se crea con el patron Builder: ProgramaLavado.builder().codigo(1).nombre("Rapido")...build()
 */
public class ProgramaLavado {
    private final int codigo;
    private String nombre;
    private int minMinutos;
    private int maxMinutos;
    private double consumoAguaLitros;
    private LocalDateTime fechaCreacion;
    private boolean aguaCaliente;

    private ProgramaLavado(Builder b) {
        this.codigo = b.codigo;
        this.nombre = b.nombre;
        this.minMinutos = b.minMinutos;
        this.maxMinutos = b.maxMinutos;
        this.consumoAguaLitros = b.consumoAguaLitros;
        this.fechaCreacion = b.fechaCreacion;
        this.aguaCaliente = b.aguaCaliente;
    }

    public static Builder builder() {
        return new Builder();
    }

    /** Dice si los minutos caen dentro del rango de este programa. */
    public boolean cubre(int minutos) {
        return minutos >= minMinutos && minutos <= maxMinutos;
    }

    public int getCodigo() {
        return codigo;
    }

    public String getNombre() {
        return nombre;
    }

    public void setNombre(String nombre) {
        this.nombre = nombre;
    }

    public int getMinMinutos() {
        return minMinutos;
    }

    public void setMinMinutos(int minMinutos) {
        this.minMinutos = minMinutos;
    }

    public int getMaxMinutos() {
        return maxMinutos;
    }

    public void setMaxMinutos(int maxMinutos) {
        this.maxMinutos = maxMinutos;
    }

    public double getConsumoAguaLitros() {
        return consumoAguaLitros;
    }

    public void setConsumoAguaLitros(double consumoAguaLitros) {
        this.consumoAguaLitros = consumoAguaLitros;
    }

    public LocalDateTime getFechaCreacion() {
        return fechaCreacion;
    }

    public void setFechaCreacion(LocalDateTime fechaCreacion) {
        this.fechaCreacion = fechaCreacion;
    }

    public boolean isAguaCaliente() {
        return aguaCaliente;
    }

    public void setAguaCaliente(boolean aguaCaliente) {
        this.aguaCaliente = aguaCaliente;
    }

    /** Patron BUILDER: arma el programa paso a paso, sin un constructor con 7 parametros. */
    public static class Builder {
        private int codigo;
        private String nombre;
        private int minMinutos;
        private int maxMinutos;
        private double consumoAguaLitros;
        private LocalDateTime fechaCreacion;
        private boolean aguaCaliente;

        public Builder codigo(int codigo) {
            this.codigo = codigo;
            return this;
        }

        public Builder nombre(String nombre) {
            this.nombre = nombre;
            return this;
        }

        public Builder minMinutos(int minMinutos) {
            this.minMinutos = minMinutos;
            return this;
        }

        public Builder maxMinutos(int maxMinutos) {
            this.maxMinutos = maxMinutos;
            return this;
        }

        public Builder consumoAguaLitros(double consumoAguaLitros) {
            this.consumoAguaLitros = consumoAguaLitros;
            return this;
        }

        public Builder fechaCreacion(LocalDateTime fechaCreacion) {
            this.fechaCreacion = fechaCreacion;
            return this;
        }

        public Builder aguaCaliente(boolean aguaCaliente) {
            this.aguaCaliente = aguaCaliente;
            return this;
        }

        public ProgramaLavado build() {
            return new ProgramaLavado(this);
        }
    }
}
