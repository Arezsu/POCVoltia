/* =====================================================================
   SERVIDOR ESTATICO LOCAL PARA EL CLIENTE WEB VOLTIA
   =====================================================================
   Sirve la carpeta ClienteWeb en http://localhost:5501 usando solo los
   modulos http y fs de Node (sin dependencias, sin instalar nada).

   Uso:
       node servidor.js

   Seguridad basica:
     - Solo responde GET y HEAD.
     - Resuelve la ruta pedida dentro de la carpeta del proyecto y rechaza
       cualquier intento de salir de ella (por ejemplo ../../archivo).
     - Responde con el tipo MIME correcto para que el navegador ejecute los
       .js como scripts clasicos.
   ===================================================================== */
'use strict';

const http = require('http');
const fs = require('fs');
const path = require('path');

const PUERTO = 5501;
const RAIZ = path.resolve(__dirname);

const TIPOS_MIME = {
  '.html': 'text/html; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.js': 'application/javascript; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.map': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.jpeg': 'image/jpeg',
  '.gif': 'image/gif',
  '.ico': 'image/x-icon',
  '.woff': 'font/woff',
  '.woff2': 'font/woff2',
  '.txt': 'text/plain; charset=utf-8'
};

/** Envia una respuesta corta y cierra. */
function responder(respuesta, estado, tipo, cuerpo) {
  respuesta.writeHead(estado, {
    'Content-Type': tipo,
    'Content-Length': Buffer.byteLength(cuerpo),
    'Cache-Control': 'no-store',
    'X-Content-Type-Options': 'nosniff'
  });
  respuesta.end(cuerpo);
}

/** True si la ruta resuelta sigue estando dentro de ClienteWeb. */
function estaDentroDeLaRaiz(ruta) {
  const relativa = path.relative(RAIZ, ruta);
  return relativa === '' || (!relativa.startsWith('..') && !path.isAbsolute(relativa));
}

/** Convierte la ruta de la URL en una ruta absoluta segura o null. */
function resolverRuta(rutaSolicitada) {
  const limpia = rutaSolicitada.endsWith('/') ? rutaSolicitada + 'index.html' : rutaSolicitada;
  // El punto delante convierte "/js/vista.js" en una ruta relativa: path.resolve
  // la une a RAIZ y luego se comprueba que no se haya escapado de la carpeta.
  const destino = path.resolve(RAIZ, '.' + limpia);
  return estaDentroDeLaRaiz(destino) ? destino : null;
}

function paginaNoEncontrada() {
  return [
    '<!DOCTYPE html>',
    '<html lang="es"><head><meta charset="UTF-8"><title>404 - Voltia</title>',
    '<style>body{font-family:Segoe UI,sans-serif;background:#0B1016;color:#E6EDF3;',
    'display:flex;min-height:100vh;align-items:center;justify-content:center;margin:0}',
    'main{text-align:center;padding:32px;border:1px solid rgba(255,255,255,.14);',
    'border-radius:18px;background:rgba(255,255,255,.06)}',
    'h1{color:#6FD3E8;letter-spacing:.2em;margin:0 0 8px}',
    'a{color:#E8B15A}</style></head>',
    '<body><main><h1>404</h1>',
    '<p>No se encontro el archivo solicitado.</p>',
    '<p><a href="/">Volver al panel de Voltia</a></p>',
    '</main></body></html>'
  ].join('');
}

function paginaError(mensaje) {
  return [
    '<!DOCTYPE html>',
    '<html lang="es"><head><meta charset="UTF-8"><title>Error - Voltia</title></head>',
    '<body style="font-family:Segoe UI,sans-serif;background:#0B1016;color:#E6EDF3;padding:40px">',
    '<h1 style="color:#E06C6C">Error</h1><p>' + mensaje + '</p>',
    '<p><a style="color:#E8B15A" href="/">Volver al panel de Voltia</a></p>',
    '</body></html>'
  ].join('');
}

const servidor = http.createServer((peticion, respuesta) => {
  if (peticion.method !== 'GET' && peticion.method !== 'HEAD') {
    responder(respuesta, 405, 'text/plain; charset=utf-8', 'Metodo no permitido');
    return;
  }

  let rutaSolicitada;
  try {
    const url = new URL(peticion.url, 'http://localhost:' + PUERTO);
    rutaSolicitada = decodeURIComponent(url.pathname);
  } catch (error) {
    responder(respuesta, 400, 'text/plain; charset=utf-8', 'Ruta invalida');
    return;
  }

  let destino = resolverRuta(rutaSolicitada);
  if (!destino) {
    responder(respuesta, 403, 'text/plain; charset=utf-8', 'Acceso denegado');
    return;
  }

  fs.stat(destino, (errorEstadisticas, informacion) => {
    if (!errorEstadisticas && informacion.isDirectory()) {
      destino = path.join(destino, 'index.html');
    }

    fs.readFile(destino, (errorLectura, contenido) => {
      if (errorLectura) {
        responder(respuesta, 404, 'text/html; charset=utf-8', paginaNoEncontrada());
        return;
      }
      const extension = path.extname(destino).toLowerCase();
      const tipo = TIPOS_MIME[extension] || 'application/octet-stream';
      respuesta.writeHead(200, {
        'Content-Type': tipo,
        'Content-Length': contenido.length,
        'Cache-Control': 'no-store',
        'X-Content-Type-Options': 'nosniff'
      });
      if (peticion.method === 'HEAD') {
        respuesta.end();
      } else {
        respuesta.end(contenido);
      }
    });
  });
});

servidor.on('error', (error) => {
  if (error.code === 'EADDRINUSE') {
    console.error('El puerto ' + PUERTO + ' ya esta en uso. Cierre el otro proceso y vuelva a intentar.');
  } else {
    console.error('Error del servidor: ' + error.message);
  }
  process.exitCode = 1;
});

servidor.listen(PUERTO, () => {
  console.log('Servidor estatico de Voltia encendido');
  console.log('  Panel web:  http://localhost:' + PUERTO + '/');
  console.log('  Carpeta:    ' + RAIZ);
  console.log('  Servidor GraphQL esperado: http://localhost:8081/graphql');
  console.log('Presione Ctrl+C para detenerlo.');
});
