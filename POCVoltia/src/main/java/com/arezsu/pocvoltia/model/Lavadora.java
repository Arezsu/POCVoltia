/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.arezsu.pocvoltia.model;

import java.time.LocalDate;

/**
 *
 * @author arezs
 */
public class Lavadora extends Electrodomestico {

    private double capacidadKilos;
    private boolean funcionSecado;
    private ProgramaLavado programaLavado; // Asociación con la clase D

    public Lavadora(int codigo, String marca, LocalDate fechaFabricacion, double precioBase, double capacidadKilos, boolean funcionSecado) throws Exception {
        super(codigo, marca, fechaFabricacion, precioBase);
        setCapacidadKilos(capacidadKilos);
        this.funcionSecado = funcionSecado;
        this.programaLavado = new ProgramaLavado("Estándar", 30);
    }

    @Override
    public double calcularPrecioFinal() {
        double precio = getPrecioBase();
        if (capacidadKilos > 10) {
            precio += 100000;
        }
        if (funcionSecado) {
            precio += 150000;
        }
        return precio;
    }

    public double getCapacidadKilos() {
        return capacidadKilos;
    }

    public void setCapacidadKilos(double capacidadKilos) throws Exception {
        if (capacidadKilos > 0) {
            this.capacidadKilos = capacidadKilos;
        } else {
            throw new Exception("Capacidad menor o igual a 0");
        }
    }

    public boolean isFuncionSecado() {
        return funcionSecado;
    }

    public void setFuncionSecado(boolean funcionSecado) {
        this.funcionSecado = funcionSecado;
    }

    public ProgramaLavado getProgramaLavado() {
        return programaLavado;
    }

    public void setProgramaLavado(ProgramaLavado programaLavado) {
        this.programaLavado = programaLavado;
    }
}
