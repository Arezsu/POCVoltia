(function (global) {
  'use strict';

  const api = global.Voltia && global.Voltia.clienteGraphQL;
  if (!api) return;

  const campos = [
    ['codigo', 'Código', 'number'],
    ['documento', 'Documento', 'text'],
    ['nombre', 'Nombre completo', 'text'],
    ['especialidad', 'Especialidad', 'text'],
    ['telefono', 'Teléfono', 'tel'],
    ['fechaIngreso', 'Fecha de ingreso', 'datetime-local']
  ];

  function iniciar() {
    document.querySelectorAll('[data-tecnico-action]').forEach((boton) => {
      boton.addEventListener('click', () => abrir(boton.dataset.tecnicoAction));
    });
  }

  function fechaParaCampo(valor) {
    return valor ? String(valor).slice(0, 16) : '';
  }

  function fechaLegible(valor) {
    if (!valor) return '—';
    const partes = String(valor).slice(0, 10).split('-');
    return partes.length === 3 ? `${partes[2]}/${partes[1]}/${partes[0]}` : valor;
  }

  function armarDatos(form) {
    const valor = (nombre) => form.elements[nombre].value.trim();
    return {
      codigo: Number(valor('codigo')),
      documento: valor('documento'),
      nombre: valor('nombre'),
      especialidad: valor('especialidad'),
      telefono: valor('telefono'),
      fechaIngreso: valor('fechaIngreso') ? `${valor('fechaIngreso')}:00` : '',
      activo: form.elements.activo.checked
    };
  }

  function mostrarEstado(contenedor, mensaje, tipo) {
    const estado = contenedor.querySelector('[data-estado-tecnico]');
    if (!estado) return;
    estado.textContent = mensaje || '';
    estado.className = `tecnico-estado${tipo ? ` ${tipo}` : ''}`;
    estado.hidden = !mensaje;
  }

  function formulario(tecnico, caso) {
    const form = document.createElement('form');
    form.className = 'formulario-tecnico';
    form.innerHTML = `<div class="rejilla-campos">${campos.map(([nombre, etiqueta, tipo]) =>
      `<div class="campo"><label for="tecnico-${nombre}">${etiqueta}</label><input id="tecnico-${nombre}" name="${nombre}" type="${tipo}" ${nombre === 'codigo' ? 'min="1" step="1"' : ''} required ${tecnico && nombre === 'codigo' ? 'readonly' : ''}></div>`
    ).join('')}<label class="casilla"><input name="activo" type="checkbox"><span>Técnico activo</span></label></div>
      <p class="error-formulario" data-estado-tecnico role="status" aria-live="polite" hidden></p>
      <div class="modal-pie"><button type="button" class="boton secundario" data-cerrar>Cancelar</button><button class="boton primario" type="submit">${caso === 'actualizar' ? 'Guardar cambios' : 'Registrar técnico'}</button></div>`;

    if (tecnico) {
      campos.forEach(([nombre]) => {
        form.elements[nombre].value = nombre === 'fechaIngreso' ? fechaParaCampo(tecnico[nombre]) : tecnico[nombre] || '';
      });
      form.elements.activo.checked = tecnico.activo === true;
    } else {
      form.elements.activo.checked = true;
    }
    return form;
  }

  function abrir(caso) {
    const titulos = {
      listar: 'Técnicos registrados',
      agregar: 'Registrar técnico',
      consultar: 'Consultar técnico',
      actualizar: 'Actualizar técnico',
      eliminar: 'Eliminar técnico'
    };
    const fondo = document.createElement('div');
    fondo.className = 'modal-fondo';
    const dialogo = document.createElement('section');
    dialogo.className = 'modal ancho modal-tecnico';
    dialogo.setAttribute('role', 'dialog');
    dialogo.setAttribute('aria-modal', 'true');
    dialogo.setAttribute('aria-labelledby', 'titulo-modal-tecnico');
    dialogo.innerHTML = `<header class="modal-cabecera"><h2 id="titulo-modal-tecnico">${titulos[caso]}</h2><button type="button" class="boton-cerrar" aria-label="Cerrar">Cerrar</button></header><div class="modal-cuerpo"></div>`;
    fondo.appendChild(dialogo);
    document.body.appendChild(fondo);

    const cuerpo = dialogo.querySelector('.modal-cuerpo');
    const cerrar = () => fondo.remove();
    dialogo.querySelector('.boton-cerrar').addEventListener('click', cerrar);
    fondo.addEventListener('mousedown', (evento) => { if (evento.target === fondo) cerrar(); });

    if (caso === 'listar') {
      listar(cuerpo);
      return;
    }
    if (caso === 'agregar') {
      const form = formulario(null, caso);
      cuerpo.appendChild(form);
      form.elements.codigo.focus();
      form.querySelector('[data-cerrar]').addEventListener('click', cerrar);
      form.addEventListener('submit', async (evento) => {
        evento.preventDefault();
        mostrarEstado(form, '', '');
        if (!form.reportValidity()) return;
        try {
          await api.agregarTecnico(armarDatos(form));
          mostrarEstado(form, 'Técnico registrado correctamente.', 'exito');
          form.reset();
          form.elements.activo.checked = true;
          form.elements.codigo.focus();
        } catch (error) {
          mostrarEstado(form, mensajeError(error), 'error');
        }
      });
      return;
    }

    const busqueda = document.createElement('form');
    busqueda.className = 'formulario-busqueda-tecnico';
    busqueda.innerHTML = `<div class="campo"><label for="buscar-codigo-tecnico">Código del técnico</label><input id="buscar-codigo-tecnico" name="codigo" type="number" min="1" step="1" required></div><button class="boton primario">Buscar</button><p class="error-formulario" data-estado-tecnico role="status" aria-live="polite" hidden></p>`;
    cuerpo.appendChild(busqueda);
    busqueda.elements.codigo.focus();

    busqueda.addEventListener('submit', async (evento) => {
      evento.preventDefault();
      mostrarEstado(busqueda, '', '');
      if (!busqueda.reportValidity()) return;
      const codigo = Number(busqueda.elements.codigo.value);
      try {
        const tecnico = await api.consultarTecnico(codigo);
        if (!tecnico) {
          mostrarEstado(busqueda, `No existe un técnico registrado con el código ${codigo}.`, 'error');
          return;
        }
        if (caso === 'consultar') {
          cuerpo.appendChild(crearFicha(tecnico));
          mostrarEstado(busqueda, 'Técnico encontrado.', 'exito');
          return;
        }
        if (caso === 'eliminar') {
          cuerpo.appendChild(crearFicha(tecnico));
          agregarConfirmacion(cuerpo, tecnico, cerrar);
          return;
        }
        const form = formulario(tecnico, caso);
        cuerpo.appendChild(form);
        form.querySelector('[data-cerrar]').addEventListener('click', cerrar);
        form.addEventListener('submit', async (ev) => {
          ev.preventDefault();
          mostrarEstado(form, '', '');
          if (!form.reportValidity()) return;
          try {
            await api.actualizarTecnico(codigo, armarDatos(form));
            mostrarEstado(form, 'Técnico actualizado correctamente.', 'exito');
          } catch (error) {
            mostrarEstado(form, mensajeError(error), 'error');
          }
        });
      } catch (error) {
        mostrarEstado(busqueda, mensajeError(error), 'error');
      }
    });
  }

  function listar(cuerpo) {
    cuerpo.innerHTML = `<form class="form-filtros-mantenimientos filtros-tecnicos">
      <div class="campo"><label for="tecnicos-filtro-texto">Nombre o documento</label><input id="tecnicos-filtro-texto" name="texto" type="search"></div>
      <div class="campo"><label for="tecnicos-filtro-especialidad">Especialidad</label><input id="tecnicos-filtro-especialidad" name="especialidad" type="search"></div>
      <div class="campo"><label for="tecnicos-filtro-activo">Estado</label><select id="tecnicos-filtro-activo" name="activo"><option value="">Todos</option><option value="true">Activos</option><option value="false">Inactivos</option></select></div>
      <button class="boton primario">Filtrar</button>
    </form>
    <p class="tecnico-estado" data-estado-tecnico role="status" aria-live="polite">Consultando técnicos…</p>
    <div class="tabla-scroll tabla-scroll-tecnicos"><table class="tabla tabla-tecnicos"><caption class="visualmente-oculto">Técnicos y lavadoras asociadas</caption><thead><tr>
      <th>Código</th><th>Documento</th><th>Nombre</th><th>Especialidad</th><th>Teléfono</th><th>Ingreso</th><th>Estado</th><th>Código lavadora</th><th>Marca lavadora</th>
    </tr></thead><tbody></tbody></table></div>`;
    const form = cuerpo.querySelector('form');

    async function cargar(evento) {
      if (evento) evento.preventDefault();
      mostrarEstado(cuerpo, 'Consultando técnicos…', '');
      try {
        const datos = await api.listarTecnicos({
          texto: form.elements.texto.value.trim() || null,
          especialidad: form.elements.especialidad.value.trim() || null,
          activo: form.elements.activo.value === '' ? null : form.elements.activo.value === 'true'
        });
        const filas = cuerpo.querySelector('tbody');
        filas.replaceChildren();
        datos.forEach((tecnico) => {
          const fila = document.createElement('tr');
          const valores = [tecnico.codigo, tecnico.documento, tecnico.nombre, tecnico.especialidad,
            tecnico.telefono, fechaLegible(tecnico.fechaIngreso), tecnico.activo ? 'Activo' : 'Inactivo',
            tecnico.codigoLavadora ?? '—', tecnico.marcaLavadora ?? '—'];
          valores.forEach((valor) => {
            const celda = document.createElement('td');
            celda.textContent = String(valor ?? '—');
            fila.appendChild(celda);
          });
          filas.appendChild(fila);
        });
        mostrarEstado(cuerpo, datos.length ? `${datos.length} técnico(s) encontrado(s).` : 'No hay técnicos que coincidan con los filtros.', datos.length ? 'exito' : '');
      } catch (error) {
        mostrarEstado(cuerpo, mensajeError(error), 'error');
      }
    }
    form.addEventListener('submit', cargar);
    cargar();
  }

  function crearFicha(tecnico) {
    const ficha = document.createElement('dl');
    ficha.className = 'ficha-tecnico';
    campos.forEach(([nombre, etiqueta]) => {
      const termino = document.createElement('dt');
      const valor = document.createElement('dd');
      termino.textContent = etiqueta;
      valor.textContent = nombre === 'fechaIngreso' ? fechaLegible(tecnico[nombre]) : String(tecnico[nombre] ?? '—');
      ficha.append(termino, valor);
    });
    const estado = document.createElement('dt');
    const valorEstado = document.createElement('dd');
    estado.textContent = 'Estado';
    valorEstado.textContent = tecnico.activo ? 'Activo' : 'Inactivo';
    ficha.append(estado, valorEstado);
    return ficha;
  }

  function agregarConfirmacion(cuerpo, tecnico, cerrar) {
    const zona = document.createElement('section');
    zona.className = 'tecnico-confirmacion';
    zona.innerHTML = '<p>Confirma que desea eliminar este técnico. La acción no se puede deshacer.</p><p class="error-formulario" data-estado-tecnico role="status" aria-live="polite" hidden></p><div class="modal-pie"><button type="button" class="boton secundario" data-cancelar>Cancelar</button><button type="button" class="boton peligro" data-eliminar>Confirmar eliminación</button></div>';
    zona.querySelector('[data-cancelar]').addEventListener('click', () => zona.remove());
    zona.querySelector('[data-eliminar]').addEventListener('click', async () => {
      const boton = zona.querySelector('[data-eliminar]');
      boton.disabled = true;
      boton.textContent = 'Eliminando…';
      try {
        await api.eliminarTecnico(tecnico.codigo);
        mostrarEstado(zona, 'Técnico eliminado correctamente.', 'exito');
        zona.querySelector('[data-cancelar]').textContent = 'Cerrar';
        zona.querySelector('[data-cancelar]').onclick = cerrar;
        boton.remove();
      } catch (error) {
        mostrarEstado(zona, mensajeError(error), 'error');
        boton.disabled = false;
        boton.textContent = 'Confirmar eliminación';
      }
    });
    cuerpo.appendChild(zona);
  }

  function mensajeError(error) {
    return error && error.message ? error.message : 'No fue posible completar la operación. Verifique la conexión con los servidores.';
  }

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', iniciar);
  else iniciar();
})(window);
