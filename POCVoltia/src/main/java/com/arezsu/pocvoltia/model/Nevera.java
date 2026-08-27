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
public class Nevera extends Electrodomestico implements ICongelable {

    private int capacidadLitros;
    private boolean tieneNoFrost;

    public Nevera(int codigo, String marca, LocalDate fechaFabricacion, double precioBase, int capacidadLitros, boolean tieneNoFrost) throws Exception {
        super(codigo, marca, fechaFabricacion, precioBase);
        setCapacidadLitros(capacidadLitros);
        this.tieneNoFrost = tieneNoFrost;
    }

    @Override
    public double calcularPrecioFinal() {
        double precio = getPrecioBase();
        if (tieneNoFrost) {
            precio += 200000;
        }
        return precio;
    }

    public int getCapacidadLitros() {
        return capacidadLitros;
    }

    public void setCapacidadLitros(int capacidadLitros) throws Exception {
        if (capacidadLitros > 0) {
            this.capacidadLitros = capacidadLitros;
        } else {
            throw new Exception("Capacidad en litros menor o igual a 0");
        }
    }

    public boolean isTieneNoFrost() {
        return tieneNoFrost;
    }

    public void setTieneNoFrost(boolean tieneNoFrost) {
        this.tieneNoFrost = tieneNoFrost;
    }

    @Override
    public void activarSuperCongelacion() {

    }

    @Override
    public void activarModoVacacionesNevera() {

    }
}
