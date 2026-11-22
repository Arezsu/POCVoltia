package com.example.ServidorLavadora.model;

public record TecnicoConLavadoraDto(int codigo, String documento, String nombre,
                                    String especialidad, String telefono,
                                    String fechaIngreso, boolean activo,
                                    Integer codigoLavadora, String marcaLavadora) { }
