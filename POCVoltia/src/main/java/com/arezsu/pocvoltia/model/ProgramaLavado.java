package com.arezsu.pocvoltia.model;

public class ProgramaLavado {
    private String id;
    private String nombre;
    private int minMinutos;
    private int maxMinutos;

    public ProgramaLavado(String id, String nombre, int minMinutos, int maxMinutos) {
        setId(id);
        this.nombre = nombre;
        this.minMinutos = minMinutos;
        this.maxMinutos = maxMinutos;
    }

    public String getId() {
        return id;
    }

    public void setId(String id) {
        if (id == null || !id.matches("\\d{3,4}")) {
            throw new IllegalArgumentException("El código debe tener exactamente 3 o 4 dígitos numéricos.");
        }
        this.id = id;
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
    
    // Método para saber si los minutos que digita la lavadora caen dentro de este programa
    public boolean perteneceAlRango(int minutos) {
        return minutos >= minMinutos && minutos <= maxMinutos;
    }
}