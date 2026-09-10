package com.arezsu.pocvoltia.servicios;

import com.arezsu.pocvoltia.model.ProgramaLavado;
import java.util.HashMap;
import java.util.Map;

public class ServicioProgramaLavado {
    
    private static Map<String, ProgramaLavado> programasLavado = new HashMap<>();

    public static Map<String, ProgramaLavado> getProgramasLavado() {
        return Map.copyOf(programasLavado);
    }

    public static boolean addProgramaLavado(ProgramaLavado programa) {
        if (programasLavado.containsKey(programa.getId())) {
            return false; 
        }
        
        for (ProgramaLavado p : programasLavado.values()) {
            boolean seSolapa = !(programa.getMaxMinutos() < p.getMinMinutos() || programa.getMinMinutos() > p.getMaxMinutos());
            if (seSolapa) {
                throw new IllegalArgumentException("El rango de minutos se cruza con el programa existente: " + p.getNombre() + " (" + p.getMinMinutos() + " - " + p.getMaxMinutos() + ").");
            }
        }
        
        programasLavado.put(programa.getId(), programa);
        return true;
    }

    public static ProgramaLavado buscarProgramaPorId(String id) {
        return programasLavado.get(id);
    }
    

    public static ProgramaLavado determinarProgramaPorMinutos(int minutos) throws Exception {
        for (ProgramaLavado p : programasLavado.values()) {
            if (p.perteneceAlRango(minutos)) {
                return p;
            }
        }
        throw new Exception("Ningún programa de lavado creado cubre el rango de " + minutos + " minutos.");
    }

    public static boolean eliminarProgramaLavado(String id) {
        if (programasLavado.containsKey(id)) {
            programasLavado.remove(id);
            return true;
        }
        return false;
    }
}