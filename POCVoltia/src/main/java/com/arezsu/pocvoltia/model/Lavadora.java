package com.arezsu.pocvoltia.model;

import java.time.LocalDate;

public class Lavadora extends Electrodomestico {

    private double capacidadCargaKilos;
    private int velocidadCentrifugadoRPM;
    private boolean tecnologiaInverter;
    private ProgramaLavado programaLavado; // Asociación con la Clase D

    public Lavadora(int codigo, String marca, LocalDate fechaFabricacion, double precioBase, double capacidadCargaKilos, int velocidadCentrifugadoRPM1, boolean tecnologiaInverter) throws Exception {
        super(codigo, marca, fechaFabricacion, precioBase);
        setCapacidadCargaKilos(capacidadCargaKilos);
        setVelocidadCentrifugadoRPM(velocidadCentrifugadoRPM);
        this.tecnologiaInverter = tecnologiaInverter;
        this.programaLavado = new ProgramaLavado("Ciclo Rápido", 15);
    }

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

    public void setCapacidadCargaKilos(double capacidadCargaKilos) throws Exception {
        if (capacidadCargaKilos > 0) {
            this.capacidadCargaKilos = capacidadCargaKilos;
        } else {
            throw new Exception("La capacidad de carga en kilos debe ser mayor a 0");
        }
    }

    public int getVelocidadCentrifugadoRPM() {
        return velocidadCentrifugadoRPM;
    }

    public void setVelocidadCentrifugadoRPM(int velocidadCentrifugadoRPM) throws Exception {
        if (velocidadCentrifugadoRPM >= 600) {
            this.velocidadCentrifugadoRPM = velocidadCentrifugadoRPM;
        } else {
            throw new Exception("La velocidad de centrifugado mínima debe ser 600 RPM");
        }
    }

    public boolean isTecnologiaInverter() {
        return tecnologiaInverter;
    }

    public void setTecnologiaInverter(boolean tecnologiaInverter) {
        this.tecnologiaInverter = tecnologiaInverter;
    }

    public ProgramaLavado getProgramaLavado() {
        return programaLavado;
    }

    public void setProgramaLavado(ProgramaLavado programaLavado) {
        this.programaLavado = programaLavado;
    }
}