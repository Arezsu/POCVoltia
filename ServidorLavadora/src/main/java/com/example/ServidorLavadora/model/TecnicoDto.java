package com.example.ServidorLavadora.model;

public record TecnicoDto(String id, int codigo, String documento, String nombre,
                         String especialidad, String telefono, String fechaIngreso,
                         boolean activo) { }
