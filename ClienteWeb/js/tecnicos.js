(function (global) {
  'use strict';
  const api = global.Voltia && global.Voltia.clienteGraphQL;
  if (!api) return;

  const labels = {
    codigo: 'Código', documento: 'Documento', nombre: 'Nombre',
    especialidad: 'Especialidad', telefono: 'Teléfono', fechaIngreso: 'Ingreso'
  };
  const campos = [
    ['codigo', 'number'], ['documento', 'text'], ['nombre', 'text'],
    ['especialidad', 'text'], ['telefono', 'tel'], ['fechaIngreso', 'datetime-local']
  ];

  function iniciar() {
    document.querySelectorAll('[data-tecnico-action]').forEach((boton) => {
      boton.addEventListener('click', () => abrir(boton.dataset.tecnicoAction));
    });
  }
  function formatoFecha(valor) { return valor ? String(valor).slice(0, 16) : ''; }
  function armarDatos(form) {
    const valor = (nombre) => form.elements[nombre].value.trim();
    return {
      codigo: Number(valor('codigo')), documento: valor('documento'), nombre: valor('nombre'),
      especialidad: valor('especialidad'), telefono: valor('telefono'),
      fechaIngreso: valor('fechaIngreso') ? valor('fechaIngreso') + ':00' : '',
      activo: form.elements.activo.checked
    };
  }
  function formulario(tecnico, soloLectura) {
    const form = document.createElement('form');
    form.innerHTML = `<div class="rejilla-campos">${campos.map(([nombre, tipo]) =>
      `<div class="campo"><label>${labels[nombre]}</label><input name="${nombre}" type="${tipo}" ${nombre === 'codigo' ? 'min="1"' : ''} required ${soloLectura || (tecnico && nombre === 'codigo') ? 'readonly' : ''}></div>`
    ).join('')}<label class="casilla"><input name="activo" type="checkbox"><span>Activo</span></label></div>
      <p class="error-formulario" role="alert"></p><div class="modal-pie"><button type="button" class="boton secundario" data-cerrar>Cancelar</button><button class="boton primario" type="submit">Guardar</button></div>`;
    if (tecnico) {
      campos.forEach(([nombre]) => { form.elements[nombre].value = nombre === 'fechaIngreso' ? formatoFecha(tecnico[nombre]) : tecnico[nombre]; });
      form.elements.activo.checked = tecnico.activo === true;
    }
    return form;
  }
  function abrir(caso) {
    const fondo = document.createElement('div');
    fondo.className = 'modal-fondo';
    const titulo = { listar: 'Listar técnicos', agregar: 'Agregar técnico', consultar: 'Consultar técnico', actualizar: 'Actualizar técnico', eliminar: 'Eliminar técnico' }[caso];
    const dialogo = document.createElement('section');
    dialogo.className = 'modal ancho'; dialogo.setAttribute('role', 'dialog'); dialogo.setAttribute('aria-modal', 'true');
    dialogo.innerHTML = `<header class="modal-cabecera"><h2>${titulo}</h2><button type="button" class="boton-cerrar">Cerrar</button></header><div class="modal-cuerpo"></div>`;
    fondo.appendChild(dialogo); document.body.appendChild(fondo);
    const cuerpo = dialogo.querySelector('.modal-cuerpo');
    const cerrar = () => fondo.remove();
    dialogo.querySelector('.boton-cerrar').addEventListener('click', cerrar);
    fondo.addEventListener('mousedown', (e) => { if (e.target === fondo) cerrar(); });

    if (caso === 'listar') {
      cuerpo.innerHTML = `<form class="form-filtros-mantenimientos"><div class="campo"><label>Nombre o documento</label><input name="texto"></div><div class="campo"><label>Especialidad</label><input name="especialidad"></div><div class="campo"><label>Estado</label><select name="activo"><option value="">Todos</option><option value="true">Activos</option><option value="false">Inactivos</option></select></div><button class="boton primario">Filtrar</button></form><p role="status"></p><div class="tabla-scroll"><table class="tabla"><thead><tr>${Object.values(labels).map(x => `<th>${x}</th>`).join('')}<th>Estado</th><th>Código lavadora (A)</th><th>Marca lavadora (A)</th></tr></thead><tbody></tbody></table></div>`;
      const listar = async (e) => {
        if (e) e.preventDefault();
        const f = e && e.currentTarget;
        const status = cuerpo.querySelector('[role="status"]'); status.textContent = 'Consultando…';
        try {
          const datos = await api.listarTecnicos({ texto: f ? f.elements.texto.value.trim() || null : null,
            especialidad: f ? f.elements.especialidad.value.trim() || null : null,
            activo: f && f.elements.activo.value !== '' ? f.elements.activo.value === 'true' : null });
          const tbody = cuerpo.querySelector('tbody'); tbody.replaceChildren();
          datos.forEach((t) => {
            const fila = document.createElement('tr');
            [...campos.map(([n]) => n), 'activo', 'codigoLavadora', 'marcaLavadora'].forEach((n) => {
              const celda = document.createElement('td'); celda.textContent = n === 'fechaIngreso' ? formatoFecha(t[n]).replace('T', ' ') : n === 'activo' ? (t.activo ? 'Activo' : 'Inactivo') : String(t[n] ?? ''); fila.appendChild(celda);
            }); tbody.appendChild(fila);
          });
          status.textContent = `${datos.length} técnico(s)`;
        } catch (err) { status.textContent = err.message; }
      };
      cuerpo.querySelector('form').addEventListener('submit', listar); listar(); return;
    }
    if (caso === 'agregar') {
      const form = formulario(null, false); cuerpo.appendChild(form);
      form.addEventListener('submit', async (e) => {
        e.preventDefault(); const data = armarDatos(form); const error = form.querySelector('[role="alert"]');
        try { await api.agregarTecnico(data); cerrar(); alert('Técnico registrado.'); } catch (err) { error.textContent = err.message; }
      });
      form.querySelector('[data-cerrar]').addEventListener('click', cerrar); return;
    }
    const busqueda = document.createElement('form');
    busqueda.innerHTML = '<div class="campo"><label>Código del técnico</label><input name="codigo" type="number" min="1" required></div><p class="error-formulario" role="alert"></p><button class="boton primario">Buscar</button>';
    cuerpo.appendChild(busqueda);
    busqueda.addEventListener('submit', async (e) => {
      e.preventDefault(); const codigo = Number(busqueda.elements.codigo.value); const error = busqueda.querySelector('[role="alert"]');
      try {
        const tecnico = await api.consultarTecnico(codigo);
        if (!tecnico) { error.textContent = 'No existe un técnico con ese código.'; return; }
        if (caso === 'consultar') {
          const ficha = document.createElement('dl');
          [...campos.map(([n]) => n), 'activo'].forEach((n) => { const dt = document.createElement('dt'); const dd = document.createElement('dd'); dt.textContent = labels[n] || 'Estado'; dd.textContent = n === 'fechaIngreso' ? tecnico[n].replace('T', ' ') : n === 'activo' ? (tecnico.activo ? 'Activo' : 'Inactivo') : tecnico[n]; ficha.append(dt, dd); });
          cuerpo.appendChild(ficha); return;
        }
        if (caso === 'eliminar') {
          const ficha = document.createElement('dl');
          [...campos.map(([n]) => n), 'activo'].forEach((n) => {
            const dt = document.createElement('dt'); const dd = document.createElement('dd');
            dt.textContent = labels[n] || 'Estado';
            dd.textContent = n === 'fechaIngreso' ? tecnico[n].replace('T', ' ') : n === 'activo' ? (tecnico.activo ? 'Activo' : 'Inactivo') : String(tecnico[n] ?? '');
            ficha.append(dt, dd);
          });
          cuerpo.appendChild(ficha);
          const btn = document.createElement('button'); btn.className = 'boton peligro'; btn.textContent = 'Eliminar técnico'; cuerpo.appendChild(btn);
          btn.addEventListener('click', async () => {
            if (!global.confirm(`¿Eliminar a ${tecnico.nombre}, documento ${tecnico.documento}, especialidad ${tecnico.especialidad}?`)) return;
            try { await api.eliminarTecnico(codigo); cerrar(); alert('Técnico eliminado.'); } catch (err) { error.textContent = err.message; }
          }); return;
        }
        const form = formulario(tecnico, false); cuerpo.appendChild(form);
        form.addEventListener('submit', async (ev) => { ev.preventDefault(); try { await api.actualizarTecnico(codigo, armarDatos(form)); cerrar(); alert('Técnico actualizado.'); } catch (err) { form.querySelector('[role="alert"]').textContent = err.message; } });
        form.querySelector('[data-cerrar]').addEventListener('click', cerrar);
      } catch (err) { error.textContent = err.message; }
    });
  }
  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', iniciar); else iniciar();
})(window);
