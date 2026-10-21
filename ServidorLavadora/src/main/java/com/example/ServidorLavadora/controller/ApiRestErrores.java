package com.example.ServidorLavadora.controller;

import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

import java.util.Map;

/** Convierte errores de validación y negocio a respuestas HTTP comprensibles. */
@RestControllerAdvice
public class ApiRestErrores {

    @ExceptionHandler(IllegalArgumentException.class)
    public ResponseEntity<Map<String, String>> errorDeValidacion(IllegalArgumentException error) {
        return ResponseEntity.status(HttpStatus.BAD_REQUEST)
                .body(Map.of("mensaje", error.getMessage() == null ? "Solicitud inválida." : error.getMessage()));
    }
}
