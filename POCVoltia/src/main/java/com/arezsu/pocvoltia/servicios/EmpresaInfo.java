package com.arezsu.pocvoltia.servicios;

public class EmpresaInfo {
    // 1. Instancia estática privada de la misma clase
    private static EmpresaInfo instancia;

    // Atributos globales de la app
    private String nombreApp;
    private String version;
    private String fecha;
    private String desarrolladores;

    // 2. Constructor privado para bloquear el uso de 'new EmpresaInfo()' desde afuera
    private EmpresaInfo() {
        this.nombreApp = "Sistema de Gestión Electrodomésticos - Voltia";
        this.version = "Beta 1.2";
        this.fecha = "Septiembre 2026";
        this.desarrolladores = "Juan Andres Bermeo Alvarez (2220241053)\n" +
                               "Alejandro Sanchez Quimbayo (2220241077)\n" +
                               "David Arredondo (2220241062)";
    }

    // 3. Método público estático para acceder a la única instancia (Punto de acceso global)
    public static synchronized EmpresaInfo getInstancia() {
        if (instancia == null) {
            instancia = new EmpresaInfo();
        }
        return instancia;
    }

    // Getters para consultar la información
    public String getNombreApp() {
        return nombreApp;
    }

    public String getVersion() {
        return version;
    }

    public String getFecha() {
        return fecha;
    }

    public String getDesarrolladores() {
        return desarrolladores;
    }

    // Método para unificar el texto del diálogo "Acerca de"
    public String getInfoCompleta() {
        return nombreApp + "\n" +
               "Versión: " + version + " - " + fecha + "\n\n" +
               "Desarrolladores:\n" + desarrolladores;
    }
}
