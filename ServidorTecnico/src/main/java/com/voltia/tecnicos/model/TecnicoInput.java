package com.voltia.tecnicos.model;

public record TecnicoInput(int codigo, String documento, String nombre, String especialidad,
                           String telefono, String fechaIngreso, Boolean activo) { }
