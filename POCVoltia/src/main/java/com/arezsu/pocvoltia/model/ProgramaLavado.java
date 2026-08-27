package com.arezsu.pocvoltia.model;

/**
 *
 * @author arezs
 */
public class ProgramaLavado {
    private String nombre;
    private int duracionMinutos;

    public ProgramaLavado(String nombre, int duracionMinutos) {
        this.nombre = nombre;
        this.duracionMinutos = duracionMinutos;
    }

    public String getNombre() {
        return nombre;
    }

    public void setNombre(String nombre) {
        this.nombre = nombre;
    }

    public int getDuracionMinutos() {
        return duracionMinutos;
    }

    public void setDuracionMinutos(int duracionMinutos) {
        this.duracionMinutos = duracionMinutos;
    }
}