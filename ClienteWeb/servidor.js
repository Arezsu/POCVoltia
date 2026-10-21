/* =====================================================================
   SERVIDOR WEB VOLTIA - BASADO EN EL FRAMEWORK EXPRESS
   =====================================================================
   Este servidor utiliza el framework Express (Node.js) para servir la
   aplicacion web de Voltia en http://localhost:5501.

   Uso:
       npm start
       o: node servidor.js
   ===================================================================== */
'use strict';

const express = require('express');
const path = require('path');

const app = express();
const PUERTO = process.env.PORT || 5501;
const RAIZ = path.resolve(__dirname);

// Middleware de Express para servir archivos estaticos (HTML, CSS, JS)
app.use(express.static(RAIZ, {
  index: 'index.html',
  maxAge: 0,
  setHeaders: (res, ruta) => {
    res.setHeader('Cache-Control', 'no-store');
    res.setHeader('X-Content-Type-Options', 'nosniff');
  }
}));

// Ruta de verificacion de estado del framework Express
app.get('/api/estado', (req, res) => {
  res.json({
    estado: 'ACTIVO',
    framework: 'Express',
    entorno: 'Node.js',
    puerto: PUERTO,
    fecha: new Date().toISOString()
  });
});

// Redireccion por defecto a la pagina principal
app.get('/*splat', (req, res) => {
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

// Iniciar servidor
app.listen(PUERTO, () => {
  console.log('====================================================');
  console.log('Cliente Web Voltia iniciado con el framework Express');
  console.log('Disponible en: http://localhost:' + PUERTO);
  console.log('Para detener: presione Ctrl + C');
  console.log('====================================================');
});
