let modalDiv = null;
let taskDaModificare = null;
let taskIdDaEliminare = null;
let taskIdDaCompletare = null;
let checkboxDaRipristinare = null;
let utenteSelezionato = null;
let categoriaSelezionata = null;
let sottoTaskIdDaModificare = null;
let sottoTaskTaskIdRiferimento = null;
let sottoTaskTitoloAttuale = '';
let sottoTaskStatoAttuale = null;

// Variabili globali per gestione modali sottotask
let sottoTaskDaModificare = null;
let sottoTaskDaEliminare = null;
let taskIdCorrente = null;

function caricaCategorieDropdown() {
  fetch('https://localhost:7000/api/Categorie')
    .then(res => res.json())
    .then(categorie => {
      const select = document.getElementById('categoriaDropdown');
      select.innerHTML = '<option value="">Seleziona categoria...</option><option value="tutti">Tutti</option>';
      categorie.forEach(cat => {
        const option = document.createElement('option');
        option.value = cat.id;
        option.textContent = cat.descrizione;
        select.appendChild(option);
      });
    })
    .catch(err => {
      alert("Errore nel caricamento categorie: " + err.message);
    });
}

// Selezione utente
function caricaUtentiDropdown() {
  fetch('https://localhost:7000/api/Utente')
    .then(res => res.json())
    .then(utenti => {
      const select = document.getElementById('utenteDropdown');
      select.innerHTML = '<option value="">Seleziona utente...</option><option value="tutti">Tutti</option>';
      utenti.forEach(u => {
        const option = document.createElement('option');
        option.value = u.id;
        option.textContent = u.nome;
        select.appendChild(option);
      });
    });
}

function apriModalUtente() {
  caricaUtentiDropdown();
  const modal = new bootstrap.Modal(document.getElementById('scegliUtenteModal'));
  modal.show();
}

function apriModalCategoria() {
  caricaCategorieDropdown();
  const modal = new bootstrap.Modal(document.getElementById('scegliCategoriaModal'));
  modal.show();
}

// Funzione per aggiornare il badge con il numero di task non fatte per utente/categoria selezionato
function aggiornaNumeroSezione() {
  // Se entrambi i filtri sono attivi, usa la nuova funzione
  if (utenteSelezionato && categoriaSelezionata) {
    aggiornaConteggiConFiltriCombinati();
    return;
  }
  
  // Se è selezionata una categoria, usa la funzione per categorie
  if (categoriaSelezionata) {
    mostraNumeroTaskNonFattePerCategoria(categoriaSelezionata);
    return;
  }

  // Altrimenti usa la logica per utenti
  let utenteId = utenteSelezionato || null;
  let url = utenteId
    ? `https://localhost:7000/api/Task/Utente/${utenteId}`
    : 'https://localhost:7000/api/Task/StatoNo';
  fetch(url)
    .then(res => res.json())
    .then(tasks => {
      let count = 0;
      if (Array.isArray(tasks)) {
        count = utenteId
          ? tasks.filter(t => !t.stato).length
          : tasks.length;
      }
      const badge = document.getElementById('numero-sezione');
      if (badge) badge.textContent = count;
    })
    .catch(() => {
      const badge = document.getElementById('numero-sezione');
      if (badge) badge.textContent = '0';
    });
}

// Funzione per aggiornare il badge con il numero di task completate per utente/categoria selezionato
function aggiornaNumeroSezioneCompletate() {
  // Se entrambi i filtri sono attivi, usa la nuova funzione
  if (utenteSelezionato && categoriaSelezionata) {
    aggiornaConteggiConFiltriCombinati();
    return;
  }
  
  // Se è selezionata una categoria, usa la funzione per categorie
  if (categoriaSelezionata) {
    mostraNumeroTaskCompletatePerCategoria(categoriaSelezionata);
    return;
  }

  // Altrimenti usa la logica per utenti
  let utenteId = utenteSelezionato || null;
  let url = utenteId
    ? `https://localhost:7000/api/Task/Utente/${utenteId}`
    : 'https://localhost:7000/api/Task';
  fetch(url)
    .then(res => res.json())
    .then(tasks => {
      let count = 0;
      if (Array.isArray(tasks)) {
        count = tasks.filter(t => t.stato).length;
      }
      const badge = document.getElementById('numero-sezione-si');
      if (badge) badge.textContent = count;
    })
    .catch(() => {
      const badge = document.getElementById('numero-sezione-si');
      if (badge) badge.textContent = '0';
    });
}

function caricaTasksPerUtente(UtenteId) {
  return fetch(`https://localhost:7000/api/Task/UtenteStatoNo/${UtenteId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      if (!tasks || tasks.length === 0) {
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Ancora nessuna task...';
        lista.appendChild(msg);
        return;
      }

      tasks.forEach(task => {
        const scadenza = new Date(task.scadenza);
        const options = {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit'
        };
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        const isCompletato = task.stato === true;
        const taskClass = isCompletato ? 'completed' : '';

        const box = document.createElement('div');
        box.className = `card mb-2 w-100 ${taskClass}`;
        box.setAttribute('data-task-id', task.id);

        box.innerHTML = `
          <div class="card-body">
            <div class="row align-items-center w-100">
              <div class="col-auto d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.4);"
                  ${isCompletato ? 'checked' : ''} onchange="toggleStato(${task.id}, this.checked, this)">
              </div>
              <div class="col d-flex flex-column">
                <span class="task-title"><strong>${task.titolo}</strong></span>
                <small class="text-muted">Scadenza: ${scadenzaFormattata}</small>
              </div>
              <div class="col-auto d-flex gap-2">
                <button class="btn btn-light rounded-circle" title="Sottotask" onclick="caricaSottoTask(${task.id})">
                  <i class="bi bi-list-task"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Nota" onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Modifica" onclick="modificaTask(${task.id})">
                  <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Elimina" onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </div>
          </div>
        `;

        lista.appendChild(box);
      });

      mostraNumeroTaskNonFattePerUtente(UtenteId);
      mostraNumeroTaskCompletatePerUtente(UtenteId);
    })
    .catch(err => console.error("Errore nel caricamento tasks per utente:", err));
}


function caricaSottoTask(taskId, keepOpenInputValue = '', forceUpdate = false) {
  const card = document.querySelector(`.card[data-task-id='${taskId}']`);
  if (!card) return;

  const isCompletatePage = window.location.pathname.endsWith('completate.html');

  const nextElem = card.nextElementSibling;
  if (nextElem && nextElem.classList.contains('sottotask-container')) {
    if (!forceUpdate) {
      // Se la dropdown è già aperta e non è un aggiornamento forzato, chiudila
      nextElem.remove();
      return;
    }
    // Se è un aggiornamento forzato, aggiorna solo il contenuto senza rimuovere il container
    fetch(`https://localhost:7000/api/SottoTask/Task/${taskId}`)
      .then(res => res.json())
      .then(sottoTasks => {
        // Ricostruisci la lista e l'input
        const container = nextElem;
        container.innerHTML = '';
        const lista = document.createElement('ul');
        lista.className = 'list-unstyled mb-2';
        if (!sottoTasks || sottoTasks.length === 0) {
          const li = document.createElement('li');
          li.className = 'text-muted fst-italic';
          li.textContent = 'Nessun sotto-task';
          lista.appendChild(li);
        } else {
          sottoTasks.forEach(st => {
            const li = document.createElement('li');
            li.className = 'mb-1 ps-2 d-flex align-items-center justify-content-between';
            li.innerHTML = `
              <span class="d-flex align-items-center gap-2">
                <input type="checkbox" class="form-check-input me-2" style="transform: scale(1.4);" ${st.stato ? 'checked' : ''} ${isCompletatePage ? 'disabled' : ''} onchange="${isCompletatePage ? '' : `toggleStatoSottoTask(${st.id}, this.checked, ${taskId}, this)`}">
                <span>${st.titolo}</span>
              </span>
              <span class="d-flex gap-2">
                ${isCompletatePage ? '' : `<button class=\"btn btn-light rounded-circle btn-sm\" title=\"Modifica sottotask\" onclick=\"modificaSottoTask(${st.id}, ${taskId}, this)\"><i class=\"bi bi-pencil\"></i></button>`}
                <button class=\"btn btn-light rounded-circle btn-sm\" title=\"Elimina sottotask\" onclick=\"eliminaSottoTask(${st.id}, ${taskId}, this)\"><i class=\"bi bi-trash\"></i></button>
              </span>
            `;
            lista.appendChild(li);
          });
        }
        // Input + pulsante
        if (!isCompletatePage) {
          const inputRow = document.createElement('div');
          inputRow.className = 'd-flex align-items-center gap-2 mt-2';
          const input = document.createElement('input');
          input.type = 'text';
          input.placeholder = 'Aggiungi una nuova sotto-task';
          input.className = 'form-control form-control-sm';
          input.style.maxWidth = '320px';
          input.style.fontSize = '0.9rem';
          if (keepOpenInputValue) input.value = keepOpenInputValue;
          const btn = document.createElement('button');
          btn.className = 'btn btn-outline-secondary p-0 d-flex align-items-center justify-content-center';
          btn.style.width = '24px';
          btn.style.height = '24px';
          btn.style.borderRadius = '50%';
          btn.innerHTML = `<i class=\"bi bi-plus-lg\" style=\"font-size: 0.9rem;\"></i>`;
          btn.title = 'Aggiungi sotto-task';
          btn.onclick = () => {
            const titolo = input.value.trim();
            if (!titolo) return;
            aggiungiSottoTask(taskId, titolo);
          };
          inputRow.appendChild(input);
          inputRow.appendChild(btn);
          container.appendChild(lista);
          container.appendChild(inputRow);
          input.focus();
        } else {
          container.appendChild(lista);
        }
      })
      .catch(() => {
        nextElem.innerHTML = '<div class="text-danger px-4 py-2">Errore nel caricamento delle sotto-task.</div>';
      });
    return;
  }

  // Se la dropdown non è aperta, aprila normalmente
  document.querySelectorAll('.sottotask-container').forEach(el => el.remove());
  fetch(`https://localhost:7000/api/SottoTask/Task/${taskId}`)
    .then(res => res.json())
    .then(sottoTasks => {
      const container = document.createElement('div');
      container.className = 'sottotask-container border-top bg-light-subtle px-4 py-3';
      container.style.borderRadius = '0 0 10px 10px';
      const lista = document.createElement('ul');
      lista.className = 'list-unstyled mb-2';
      if (!sottoTasks || sottoTasks.length === 0) {
        const li = document.createElement('li');
        li.className = 'text-muted fst-italic';
        li.textContent = 'Nessun sotto-task';
        lista.appendChild(li);
      } else {
        sottoTasks.forEach(st => {
          const li = document.createElement('li');
          li.className = 'mb-1 ps-2 d-flex align-items-center justify-content-between';
          li.innerHTML = `
            <span class="d-flex align-items-center gap-2">
              <input type="checkbox" class="form-check-input me-2" style="transform: scale(1.4);" ${st.stato ? 'checked' : ''} ${isCompletatePage ? 'disabled' : ''} onchange="${isCompletatePage ? '' : `toggleStatoSottoTask(${st.id}, this.checked, ${taskId}, this)`}">
              <span>${st.titolo}</span>
            </span>
            <span class="d-flex gap-2">
              ${isCompletatePage ? '' : `<button class=\"btn btn-light rounded-circle btn-sm\" title=\"Modifica sottotask\" onclick=\"modificaSottoTask(${st.id}, ${taskId}, this)\"><i class=\"bi bi-pencil\"></i></button>`}
              <button class=\"btn btn-light rounded-circle btn-sm\" title=\"Elimina sottotask\" onclick=\"eliminaSottoTask(${st.id}, ${taskId}, this)\"><i class=\"bi bi-trash\"></i></button>
            </span>
          `;
          lista.appendChild(li);
        });
      }
      if (!isCompletatePage) {
        const inputRow = document.createElement('div');
        inputRow.className = 'd-flex align-items-center gap-2 mt-2';
        const input = document.createElement('input');
        input.type = 'text';
        input.placeholder = 'Aggiungi una nuova sotto-task';
        input.className = 'form-control form-control-sm';
        input.style.maxWidth = '320px';
        input.style.fontSize = '0.9rem';
        if (keepOpenInputValue) input.value = keepOpenInputValue;
        const btn = document.createElement('button');
        btn.className = 'btn btn-outline-secondary p-0 d-flex align-items-center justify-content-center';
        btn.style.width = '24px';
        btn.style.height = '24px';
        btn.style.borderRadius = '50%';
        btn.innerHTML = `<i class=\"bi bi-plus-lg\" style=\"font-size: 0.9rem;\"></i>`;
        btn.title = 'Aggiungi sotto-task';
        btn.onclick = () => {
          const titolo = input.value.trim();
          if (!titolo) return;
          aggiungiSottoTask(taskId, titolo);
        };
        inputRow.appendChild(input);
        inputRow.appendChild(btn);
        container.appendChild(lista);
        container.appendChild(inputRow);
        input.focus();
      } else {
        container.appendChild(lista);
      }
      card.parentNode.insertBefore(container, card.nextSibling);
      if (!isCompletatePage && container.querySelector('input[type="text"]')) {
        container.querySelector('input[type="text"]').focus();
      }
    })
    .catch(() => {
      const errorBox = document.createElement('div');
      errorBox.className = 'sottotask-container text-danger px-4 py-2';
      errorBox.textContent = 'Errore nel caricamento delle sotto-task.';
      card.parentNode.insertBefore(errorBox, card.nextSibling);
    });
}

function aggiungiSottoTask(taskId, titolo) {
  fetch('https://localhost:7000/api/SottoTask', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ titolo, taskID: taskId })
  })
    .then(res => {
      if (!res.ok) throw new Error("Errore nell'aggiunta della sottotask");
      return res.json();
    })
    .then(() => {
      // Ricarica lasciando la dropdown aperta e l'input vuoto
      caricaSottoTask(taskId, '', true); // <--- forceUpdate true
      // Focus sull'input dopo aggiunta
      setTimeout(() => {
        const card = document.querySelector(`.card[data-task-id='${taskId}']`);
        if (card) {
          const nextElem = card.nextElementSibling;
          if (nextElem) {
            const input = nextElem.querySelector('input[type="text"]');
            if (input) input.value = '';
            if (input) input.focus();
          }
        }
      }, 100);
    })
    .catch(err => alert(err.message));
}

function toggleStatoSottoTask(sottoTaskId, nuovoStato, taskId, checkboxElement) {
  fetch(`https://localhost:7000/api/SottoTask/${sottoTaskId}`)
    .then(res => res.json())
    .then(sottoTask => {
      return fetch(`https://localhost:7000/api/SottoTask/${sottoTaskId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...sottoTask, stato: nuovoStato })
      });
    })
    .then(res => res.json())
    .then(() => {
      // Ricarica lasciando la dropdown aperta e focus sull'input
      caricaSottoTask(taskId, '', true); // <--- forceUpdate true
      setTimeout(() => {
        const card = document.querySelector(`.card[data-task-id='${taskId}']`);
        if (card) {
          const nextElem = card.nextElementSibling;
          if (nextElem) {
            const input = nextElem.querySelector('input[type="text"]');
            if (input) input.focus();
          }
        }
      }, 100);
    })
    .catch(err => alert(err.message));
}

// --- MODALI SOTTOTASK ---
// Funzione per aprire la modale di modifica sottotask
function modificaSottoTask(sottoTaskId, taskId, buttonElement) {
  sottoTaskDaModificare = sottoTaskId;
  taskIdCorrente = taskId;
  // Trova il titolo corrente dal DOM
  const liElement = buttonElement.closest('li');
  const spanElement = liElement.querySelector('span span');
  const titoloCorrente = spanElement ? spanElement.textContent : '';
  document.getElementById('titoloSottoTaskModifica').value = titoloCorrente;
  const modal = new bootstrap.Modal(document.getElementById('modificaSottoTaskModal'));
  modal.show();
}

// Funzione per aprire la modale di eliminazione sottotask
function eliminaSottoTask(sottoTaskId, taskId, buttonElement) {
  sottoTaskDaEliminare = sottoTaskId;
  taskIdCorrente = taskId;
  const modal = new bootstrap.Modal(document.getElementById('eliminaSottoTaskModal'));
  modal.show();
}

// Listener per conferma modifica sottotask
const btnConfermaModificaSottoTask = document.getElementById('confermaModificaSottoTaskBtn');
if (btnConfermaModificaSottoTask) {
  btnConfermaModificaSottoTask.addEventListener('click', async () => {
    const nuovoTitolo = document.getElementById('titoloSottoTaskModifica').value.trim();
    if (!nuovoTitolo) {
      alert('Inserisci un titolo valido');
      return;
    }
    btnConfermaModificaSottoTask.disabled = true;
    try {
      const res = await fetch(`https://localhost:7000/api/SottoTask/${sottoTaskDaModificare}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ titolo: nuovoTitolo, taskID: taskIdCorrente })
      });
      if (!res.ok) throw new Error('Errore nella modifica della sottotask');
      await res.json();
      // Ricarica lasciando la dropdown aperta e focus sull'input
      caricaSottoTask(taskIdCorrente, '');
      setTimeout(() => {
        const card = document.querySelector(`.card[data-task-id='${taskIdCorrente}']`);
        if (card) {
          const nextElem = card.nextElementSibling;
          if (nextElem) {
            const input = nextElem.querySelector('input[type="text"]');
            if (input) input.focus();
          }
        }
      }, 100);
      sottoTaskDaModificare = null;
      taskIdCorrente = null;
      const modal = bootstrap.Modal.getInstance(document.getElementById('modificaSottoTaskModal'));
      modal.hide();
    } catch (err) {
      alert(err.message);
    } finally {
      btnConfermaModificaSottoTask.disabled = false;
    }
  });
}

// Listener per conferma eliminazione sottotask
const btnConfermaEliminaSottoTask = document.getElementById('confermaEliminaSottoTaskBtn');
if (btnConfermaEliminaSottoTask) {
  btnConfermaEliminaSottoTask.addEventListener('click', () => {
    fetch(`https://localhost:7000/api/SottoTask/${sottoTaskDaEliminare}`, {
      method: 'DELETE'
    })
      .then(res => {
        if (!res.ok) throw new Error('Errore nell\'eliminazione della sottotask');
        return res.json();
      })
      .then(() => {
        // Ricarica lasciando la dropdown aperta e focus sull'input
        caricaSottoTask(taskIdCorrente, '');
        setTimeout(() => {
          const card = document.querySelector(`.card[data-task-id='${taskIdCorrente}']`);
          if (card) {
            const nextElem = card.nextElementSibling;
            if (nextElem) {
              const input = nextElem.querySelector('input[type="text"]');
              if (input) input.focus();
            }
          }
        }, 100);
        sottoTaskDaEliminare = null;
        taskIdCorrente = null;
        const modal = bootstrap.Modal.getInstance(document.getElementById('eliminaSottoTaskModal'));
        modal.hide();
      })
      .catch(err => alert(err.message));
  });
}

function toggleStatoSottoTask(sottoTaskId, nuovoStato, taskId, checkboxElement) {
  fetch(`https://localhost:7000/api/SottoTask/${sottoTaskId}`)
    .then(res => res.json())
    .then(sottoTask => {
      return fetch(`https://localhost:7000/api/SottoTask/${sottoTaskId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...sottoTask, stato: nuovoStato })
      });
    })
    .then(res => res.json())
    .then(() => {
      // Ricarica lasciando la dropdown aperta e focus sull'input
      caricaSottoTask(taskId, '', true); // <--- forceUpdate true
      setTimeout(() => {
        const card = document.querySelector(`.card[data-task-id='${taskId}']`);
        if (card) {
          const nextElem = card.nextElementSibling;
          if (nextElem) {
            const input = nextElem.querySelector('input[type="text"]');
            if (input) input.focus();
          }
        }
      }, 100);
    })
    .catch(err => alert(err.message));
}

function caricaTasksPerCategoria(CategoriaId) {
  return fetch(`https://localhost:7000/api/Task/Categoria/${CategoriaId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      tasks.forEach(task => {
        const scadenza = new Date(task.scadenza);
        const options = {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit'
        };
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        const isCompletato = task.stato === true;
        const taskClass = isCompletato ? 'completed' : '';

        const box = document.createElement('div');
        box.className = `card mb-2 w-100 ${taskClass}`;
        box.setAttribute('data-task-id', task.id);

        box.innerHTML = `
          <div class="card-body">
            <div class="row align-items-center w-100">
              <div class="col-auto d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.4);"
                  ${isCompletato ? 'checked' : ''} onchange="toggleStato(${task.id}, this.checked, this)">
              </div>
              <div class="col d-flex flex-column">
                <span class="task-title"><strong>${task.titolo}</strong></span>
                <small class="text-muted">Scadenza: ${scadenzaFormattata}</small>
              </div>
              <div class="col-auto d-flex gap-2">
                <button class="btn btn-light rounded-circle" title="Sottotask" onclick="caricaSottoTask(${task.id})">
                  <i class="bi bi-list-task"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Nota" onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Modifica" onclick="modificaTask(${task.id})">
                  <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Elimina" onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </div>
          </div>
        `;

        lista.appendChild(box);
      });

      mostraNumeroTaskNonFattePerCategoria(CategoriaId);
      mostraNumeroTaskCompletatePerCategoria(CategoriaId);
    })
    .catch(err => console.error("Errore nel caricamento tasks per categoria:", err));
}


function aggiungiCategoria(e) {
  e.preventDefault();
  const descrizione = document.getElementById('nomeCategoria').value;

  fetch('https://localhost:7000/api/Categorie', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ descrizione })
  })
    .then(res => {
      if (!res.ok) throw new Error('Errore nell\'aggiunta categoria');
      return res.json();
    })
    .then(() => {
      document.getElementById('formAggiungiCategoria').reset();
      caricaCategorie && caricaCategorie();
      const modal = bootstrap.Modal.getInstance(document.getElementById('aggiungiCategoriaModal'));
      modal.hide();
      alert('Categoria aggiunta!');
    })
    .catch(err => alert(err.message));
}

function apriModalEliminaCategoria() {
  fetch('https://localhost:7000/api/Categorie')
    .then(res => res.json())
    .then(categorie => {
      const select = document.getElementById('eliminaCategoriaDropdown');
      select.innerHTML = '<option value="">Seleziona categoria da eliminare...</option>';
      categorie.forEach(cat => {
        const option = document.createElement('option');
        option.value = cat.id;
        option.textContent = cat.descrizione;
        select.appendChild(option);
      });

      const modal = new bootstrap.Modal(document.getElementById('eliminaCategoriaModal'));
      modal.show();
    })
    .catch(err => alert("Errore nel caricamento categorie: " + err.message));
}

function aggiungiUtente(e) {
  e.preventDefault();
  const nome = document.getElementById('nomeUtente').value;

  fetch('https://localhost:7000/api/Utente', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ nome })
  })
    .then(res => {
      if (!res.ok) throw new Error('Errore nell\'aggiunta utente');
      return res.json();
    })
    .then(() => {
      document.getElementById('formAggiungiUtente').reset();
      caricaUtentiForm && caricaUtentiForm();
      caricaUtentiDropdown && caricaUtentiDropdown();
      const modal = bootstrap.Modal.getInstance(document.getElementById('aggiungiUtenteModal'));
      modal.hide();
      alert('Utente aggiunto!');
    })
    .catch(err => alert(err.message));
}

function apriModalEliminaUtente() {
  fetch('https://localhost:7000/api/Utente')
    .then(res => res.json())
    .then(utenti => {
      const select = document.getElementById('eliminaUtenteDropdown');
      select.innerHTML = '<option value="">Seleziona utente da eliminare...</option>';
      utenti.forEach(u => {
        const option = document.createElement('option');
        option.value = u.id;
        option.textContent = u.nome;
        select.appendChild(option);
      });

      const modal = new bootstrap.Modal(document.getElementById('eliminaUtenteModal'));
      modal.show();
    })
    .catch(err => alert("Errore nel caricamento utenti: " + err.message));
}

function caricaTasks() {
  return fetch('https://localhost:7000/api/Task/StatoNo')
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      tasks.forEach(task => {
        const scadenza = new Date(task.scadenza);
        const options = {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit'
        };
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        // Controlla se il task è completato
        const isCompletato = task.stato === true;
        const taskClass = isCompletato ? 'completed' : '';

        const box = document.createElement('div');
        box.className = `card mb-2 w-100 ${taskClass}`;
        box.setAttribute('data-task-id', task.id);

        box.innerHTML = `
          <div class="card-body">
            <div class="row align-items-center w-100">
              <div class="col-auto d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.4);"
                  ${isCompletato ? 'checked' : ''} onchange="toggleStato(${task.id}, this.checked, this)">
              </div>
              <div class="col d-flex flex-column">
                <span class="task-title"><strong>${task.titolo}</strong></span>
                <small class="text-muted">Scadenza: ${scadenzaFormattata}</small>
              </div>
              <div class="col-auto d-flex gap-2">
                <button class="btn btn-light rounded-circle" title="Sottotask" onclick="caricaSottoTask(${task.id})">
                  <i class="bi bi-list-task"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Nota" onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Modifica" onclick="modificaTask(${task.id})">
                  <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Elimina" onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </div>
          </div>
        `;

        lista.appendChild(box);
      });

      aggiornaNumeroSezione();
      aggiornaNumeroSezioneCompletate();
    })
    .catch(err => console.error("Errore nel caricamento delle task:", err));
}


function salvaTask(e) {
  if (e) e.preventDefault();

  const titolo = document.getElementById('titolo').value;
  const descrizione = document.getElementById('descrizione').value;
  const data = document.getElementById('scadenza').value;      // yyyy-mm-dd
  const ora = document.getElementById('scadenzaOra').value;   // hh:mm
  const categoriaID = parseInt(document.getElementById('categoria').value);
  const utenteID = parseInt(document.getElementById('utente').value);

  let scadenza = null;
  if (data) {
    // Se c'è l'ora, mettila, altrimenti metti mezzanotte
    scadenza = ora ? `${data}T${ora}:00` : `${data}T00:00:00`;
  }

  // Validazione data futura
  const oggi = new Date();
  oggi.setHours(0, 0, 0, 0);
  const dataScadenza = new Date(scadenza);
  if (dataScadenza < oggi) {
    alert('La data di scadenza deve essere oggi o una data futura.');
    return;
  }

  let url = 'https://localhost:7000/api/Task';
  let method = 'POST';
  if (taskDaModificare) {
    url = `https://localhost:7000/api/Task/${taskDaModificare}`;
    method = 'PUT';
  }
  console.log("Scadenza inviata:", scadenza);
  fetch(url, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ titolo, descrizione, stato: false, scadenza, categoriaID, utenteID })
  })
    .then(res => {
      if (!res.ok) throw new Error('Errore nel salvataggio');
      return res.json();
    })
    .then(() => {
      const modal = bootstrap.Modal.getInstance(document.getElementById('exampleModal'));
      modal.hide();
      document.getElementById('taskForm').reset();
      taskDaModificare = null;      document.getElementById('btnAggiungi').textContent = 'Aggiungi';
      caricaTasksConFiltri();
    })
}

function eliminaTask(id) {
  console.log('eliminaTask chiamata con id:', id);
  taskIdDaEliminare = id;
  var modal = new bootstrap.Modal(document.getElementById('confermaEliminaModal'));
  modal.show();
}

function toggleStato(id, nuovoStato, checkbox) {
  if (nuovoStato) {
    taskIdDaCompletare = id;
    checkboxDaRipristinare = checkbox;
    var modal = new bootstrap.Modal(document.getElementById('confermaCompletaModal'));
    modal.show();
  } else {
    aggiornaStatoTask(id, false);
  }
}

// Aggiorna badge anche dopo modifica stato o eliminazione
function aggiornaStatoTask(id, nuovoStato) {
  fetch(`https://localhost:7000/api/Task/${id}`)
    .then(res => res.json())
    .then(task => {
      return fetch(`https://localhost:7000/api/Task/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ ...task, stato: nuovoStato })
      });
    })
    .then(res => res.json())    .then(() => {
      // Ricarica con la nuova logica di filtri combinati
      caricaTasksConFiltri();
    })
    .catch(err => alert(err.message));
}

// Funzione per gestire l'eliminazione task
function gestioneEliminazioneTask() {
  const btnElimina = document.getElementById('btnConfermaElimina');
  if (!btnElimina) {
    console.log('Bottone btnConfermaElimina non trovato');
    return;
  }

  btnElimina.addEventListener('click', function () {
    console.log('Click su btnConfermaElimina, taskId:', taskIdDaEliminare);
    if (taskIdDaEliminare !== null) {
      console.log('Chiamata API DELETE per task:', taskIdDaEliminare);
      fetch(`https://localhost:7000/api/Task/${taskIdDaEliminare}`, {
        method: 'DELETE'
      })
        .then(response => {
          console.log('Risposta DELETE:', response.status);
          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }
          taskIdDaEliminare = null;
          var modal = bootstrap.Modal.getInstance(document.getElementById('confermaEliminaModal'));
          modal.hide();

          console.log('Controllo pagina corrente:', window.location.pathname);
          console.log('categoriaSelezionata:', categoriaSelezionata);
          console.log('utenteSelezionato:', utenteSelezionato);          // Ricarica con la nuova logica di filtri combinati
          caricaTasksConFiltri();
        })
        .catch(err => {
          console.error('Errore durante eliminazione:', err);
          alert('Errore durante l\'eliminazione della task: ' + err.message);
        });
    } else {
      console.log('taskIdDaEliminare è null!');
    }
  });
}

function modificaTask(id) {
  fetch(`https://localhost:7000/api/Task/${id}`)
    .then(res => res.json())
    .then(task => {
      document.getElementById('titolo').value = task.titolo;
      document.getElementById('descrizione').value = task.descrizione;
      document.getElementById('categoria').value = task.categoriaID;
      document.getElementById('utente').value = task.utenteID;

      // Gestione della data e ora di scadenza
      if (task.scadenza) {
        const dataOra = task.scadenza.split('T');
        document.getElementById('scadenza').value = dataOra[0];
        if (dataOra[1]) {
          // Estrai solo ore e minuti (formato HH:MM)
          const ora = dataOra[1].substring(0, 5);
          document.getElementById('scadenzaOra').value = ora;
        }
      } else {
        document.getElementById('scadenza').value = '';
        document.getElementById('scadenzaOra').value = '';
      }

      taskDaModificare = id;
      document.getElementById('btnAggiungi').textContent = 'Salva modifiche';

      const modal = new bootstrap.Modal(document.getElementById('exampleModal'));
      modal.show();
    })
    .catch(err => {
      alert("Impossibile caricare il task.");
      console.error("Errore:", err);
    });
}

function notaTask(id) {
  fetch(`https://localhost:7000/api/Task/${id}`)
    .then(res => res.json())
    .then(task => {
      Promise.all([
        fetch(`https://localhost:7000/api/Utente/${task.utenteID}`).then(r => r.json()),
        fetch(`https://localhost:7000/api/Categorie/${task.categoriaID}`).then(r => r.json())
      ])
        .then(([utente, categoria]) => {
          document.getElementById('modalDescrizioneTesto').innerHTML = `
            <div><strong>Titolo:</strong> ${task.titolo}</div>
            <div><strong>Descrizione:</strong> ${task.descrizione}</div>
            <div><strong>Utente:</strong> ${utente.nome}</div>
            <div><strong>Categoria:</strong> ${categoria.descrizione}</div>
          `;
          var modal = new bootstrap.Modal(document.getElementById('descrizioneModal'));
          modal.show();
        })
        .catch(() => {
          document.getElementById('modalDescrizioneTesto').textContent = "Errore nel caricamento di utente o categoria.";
          var modal = new bootstrap.Modal(document.getElementById('descrizioneModal'));
          modal.show();
        });
    })
    .catch(() => {
      document.getElementById('modalDescrizioneTesto').textContent = "Descrizione non trovata.";
      var modal = new bootstrap.Modal(document.getElementById('descrizioneModal'));
      modal.show();
    });
}

function caricaCategorie() {
  return fetch('https://localhost:7000/api/Categorie')
    .then(res => res.json())
    .then(categorie => {
      const select = document.getElementById('categoria');
      select.innerHTML = '<option value="">Seleziona categoria...</option>';
      categorie.forEach(cat => {
        const option = document.createElement('option');
        option.value = cat.id;
        option.textContent = cat.descrizione;
        select.appendChild(option);
      });
    })
    .catch(err => console.error("Errore nel caricamento categorie:", err));
}

function caricaUtentiForm() {
  return fetch('https://localhost:7000/api/Utente')
    .then(res => res.json())
    .then(utenti => {
      const select = document.getElementById('utente');
      select.innerHTML = '<option value="">Seleziona utente...</option>';
      utenti.forEach(u => {
        const option = document.createElement('option');
        option.value = u.id;
        option.textContent = u.nome;
        select.appendChild(option);
      });
    })
    .catch(err => console.error("Errore nel caricamento utenti:", err));
}

function caricaTasksCompletate() {
  let url = 'https://localhost:7000/api/Task';

  if (categoriaSelezionata) {
    url = `https://localhost:7000/api/Task/Categoria/${categoriaSelezionata}`;
  } else if (utenteSelezionato) {
    url = `https://localhost:7000/api/Task/Utente/${utenteSelezionato}`;
  }

  fetch(url)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      const completate = tasks.filter(task => task.stato === true);

      if (completate.length === 0) {
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Nessuna task completata...';
        lista.appendChild(msg);
        return;
      }

      completate.forEach(task => {
        const scadenza = new Date(task.scadenza);
        const options = {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit'
        };
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        const box = document.createElement('div');
        box.className = 'card mb-2 w-100 completed';
        box.setAttribute('data-task-id', task.id);

        box.innerHTML = `
          <div class="card-body">
            <div class="row align-items-center w-100">
              <div class="col-auto d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.4);" checked
                  onchange="toggleStato(${task.id}, this.checked, this)">
              </div>
              <div class="col d-flex flex-column">
                <span class="task-title"><strong>${task.titolo}</strong></span>
                <small class="text-muted">Scadenza: ${scadenzaFormattata}</small>
              </div>
              <div class="col-auto d-flex gap-2">
                <button class="btn btn-light rounded-circle" title="Sottotask" onclick="caricaSottoTask(${task.id})">
                  <i class="bi bi-list-task"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Nota" onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Elimina" onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </div>
          </div>
        `;

        lista.appendChild(box);
      });

      aggiornaNumeroSezioneCompletate();
    })
    .catch(err => {
      console.error("Errore nel caricamento delle task completate:", err);
      const lista = document.getElementById('lista-box');
      lista.innerHTML = `
        <div class="text-center text-danger my-4">
          Errore nel caricamento delle task completate
        </div>
      `;
    });
}


function mostraNumeroTaskNonFattePerUtente(utenteId) {
  if (!utenteId) {
    document.getElementById('numero-sezione').textContent = '0';
    return;
  }
  fetch(`https://localhost:7000/api/Task/UtenteStatoNo/${utenteId}`)
    .then(res => res.json())
    .then(tasks => {
      const nonFatte = tasks.filter(t => !t.stato).length;
      document.getElementById('numero-sezione').textContent = nonFatte;
    })
    .catch(() => {
      document.getElementById('numero-sezione').textContent = '0';
    });
}

function mostraNumeroTaskCompletatePerUtente(utenteId) {
  if (!utenteId) {
    document.getElementById('numero-sezione-si').textContent = '0';
    return;
  }
  fetch(`https://localhost:7000/api/Task/Utente/${utenteId}`)
    .then(res => res.json())
    .then(tasks => {
      const completate = tasks.filter(t => t.stato).length;
      document.getElementById('numero-sezione-si').textContent = completate;
    })
    .catch(() => {
      document.getElementById('numero-sezione-si').textContent = '0';
    });
}

function mostraNumeroTaskNonFattePerCategoria(categoriaId) {
  if (!categoriaId) {
    document.getElementById('numero-sezione').textContent = '0';
    return;
  }
  fetch(`https://localhost:7000/api/Task/Categoria/${categoriaId}`)
    .then(res => res.json())
    .then(tasks => {
      const nonFatte = tasks.filter(t => !t.stato).length;
      document.getElementById('numero-sezione').textContent = nonFatte;
    })
    .catch(() => {
      document.getElementById('numero-sezione').textContent = '0';
    });
}

function mostraNumeroTaskCompletatePerCategoria(categoriaId) {
  if (!categoriaId) {
    document.getElementById('numero-sezione-si').textContent = '0';
    return;
  }
  fetch(`https://localhost:7000/api/Task/Categoria/${categoriaId}`)
    .then(res => res.json())
    .then(tasks => {
      const completate = tasks.filter(t => t.stato).length;
      document.getElementById('numero-sezione-si').textContent = completate;
    })
    .catch(() => {
      document.getElementById('numero-sezione-si').textContent = '0';
    });
}

function caricaTasksCompletatePerUtente(utenteId) {
  fetch(`https://localhost:7000/api/Task/Utente/${utenteId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      const completate = tasks.filter(task => task.stato === true);

      if (completate.length === 0) {
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Nessuna task completata per questo utente...';
        lista.appendChild(msg);
        return;
      }

      completate.forEach(task => {
        const scadenza = new Date(task.scadenza);
        const options = {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit'
        };
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        const box = document.createElement('div');
        box.className = 'card mb-2 w-100 completed';
        box.setAttribute('data-task-id', task.id);

        box.innerHTML = `
          <div class="card-body">
            <div class="row align-items-center w-100">
              <div class="col-auto d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.4);" checked
                  onchange="toggleStato(${task.id}, this.checked, this)">
              </div>
              <div class="col d-flex flex-column">
                <span class="task-title"><strong>${task.titolo}</strong></span>
                <small class="text-muted">Scadenza: ${scadenzaFormattata}</small>
              </div>
              <div class="col-auto d-flex gap-2">
                <button class="btn btn-light rounded-circle" title="Sottotask" onclick="caricaSottoTask(${task.id})">
                  <i class="bi bi-list-task"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Nota" onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Elimina" onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </div>
          </div>
        `;

        lista.appendChild(box);
      });

      mostraNumeroTaskCompletatePerUtente(utenteId);
    })
    .catch(err => {
      console.error("Errore nel caricamento tasks completate per utente:", err);
      const lista = document.getElementById('lista-box');
      lista.innerHTML = `
        <div class="text-center text-danger my-4">
          Errore nel caricamento delle task completate per l'utente
        </div>
      `;
    });
}


function caricaTasksCompletatePerCategoria(categoriaId) {
  fetch(`https://localhost:7000/api/Task/CategoriaStatoSi/${categoriaId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      const completate = tasks.filter(task => task.stato === true);

      if (completate.length === 0) {
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Nessuna task completata per questa categoria...';
        lista.appendChild(msg);
        return;
      }

      completate.forEach(task => {
        const scadenza = new Date(task.scadenza);
        const options = {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit'
        };
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        const box = document.createElement('div');
        box.className = 'card mb-2 w-100 completed';
        box.setAttribute('data-task-id', task.id);

        box.innerHTML = `
          <div class="card-body">
            <div class="row align-items-center w-100">
              <div class="col-auto d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.4);" checked
                  onchange="toggleStato(${task.id}, this.checked, this)">
              </div>
              <div class="col d-flex flex-column">
                <span class="task-title"><strong>${task.titolo}</strong></span>
                <small class="text-muted">Scadenza: ${scadenzaFormattata}</small>
              </div>
              <div class="col-auto d-flex gap-2">
                <button class="btn btn-light rounded-circle" title="Sottotask" onclick="caricaSottoTask(${task.id})">
                  <i class="bi bi-list-task"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Nota" onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Elimina" onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash"></i>
                </button>
              </div>
            </div>
          </div>
        `;

        lista.appendChild(box);
      });

      mostraNumeroTaskCompletatePerCategoria(categoriaId);
    })
    .catch(err => {
      console.error("Errore nel caricamento tasks completate per categoria:", err);
      const lista = document.getElementById('lista-box');
      lista.innerHTML = `
        <div class="text-center text-danger my-4">
          Errore nel caricamento delle task completate per la categoria
        </div>
      `;
    });
}

// Funzione principale per caricare le task con filtri combinati
function caricaTasksConFiltri() {
  const isCompletate = window.location.pathname.endsWith('completate.html');
  
  // Se nessun filtro è attivo, carica tutte le task
  if (!utenteSelezionato && !categoriaSelezionata) {
    if (isCompletate) {
      caricaTasksCompletate();
    } else {
      caricaTasks();
    }
    // Aggiorna conteggi anche quando non ci sono filtri
    aggiornaNumeroSezione();
    if (typeof aggiornaNumeroSezioneCompletate === 'function') {
      aggiornaNumeroSezioneCompletate();
    }
    return;
  }
  
  // Se solo utente è selezionato
  if (utenteSelezionato && !categoriaSelezionata) {
    if (isCompletate) {
      caricaTasksCompletatePerUtente(utenteSelezionato);
    } else {
      caricaTasksPerUtente(utenteSelezionato);
    }
    return;
  }
  
  // Se solo categoria è selezionata
  if (!utenteSelezionato && categoriaSelezionata) {
    if (isCompletate) {
      caricaTasksCompletatePerCategoria(categoriaSelezionata);
    } else {
      caricaTasksPerCategoria(categoriaSelezionata);
    }
    return;
  }
  
  // Se entrambi sono selezionati, carica con filtro combinato
  if (utenteSelezionato && categoriaSelezionata) {
    if (isCompletate) {
      caricaTasksCompletatePerUtenteECategoria(utenteSelezionato, categoriaSelezionata);
    } else {
      caricaTasksPerUtenteECategoria(utenteSelezionato, categoriaSelezionata);
    }
  }
}

// Funzione per caricare task filtrate per utente e categoria (task attive)
function caricaTasksPerUtenteECategoria(utenteId, categoriaId) {
  fetch(`https://localhost:7000/api/Task/UtenteCategoria/${utenteId}/${categoriaId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      const tasksFiltrate = tasks.filter(task => task.stato === false);

      if (tasksFiltrate.length === 0) {
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Nessuna task trovata per questo utente e categoria...';
        lista.appendChild(msg);
        return;
      }

      tasksFiltrate.forEach(task => {
        const scadenza = new Date(task.scadenza);
        const options = {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit'
        };
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        const box = document.createElement('div');
        box.className = 'card mb-2 w-100';
        box.setAttribute('data-task-id', task.id);

        box.innerHTML = `
          <div class="card-body">
            <div class="row align-items-center w-100">
              <div class="col-auto d-flex align-items-center">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.4);"
                  ${task.stato ? 'checked' : ''} onchange="toggleStato(${task.id}, this.checked, this)">
              </div>
              <div class="col d-flex flex-column">
                <span class="task-title"><strong>${task.titolo}</strong></span>
                <small class="text-muted">Scadenza: ${scadenzaFormattata}</small>
              </div>
              <div class="col-auto d-flex gap-2">
                <button class="btn btn-light rounded-circle" title="Sottotask" onclick="caricaSottoTask(${task.id})">
                  <i class="bi bi-list-task"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Nota" onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Modifica" onclick="modificaTask(${task.id})">
                  <i class="bi bi-pencil-square"></i>
                </button>
                <button class="btn btn-light rounded-circle" title="Elimina" onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash3"></i>
                </button>
              </div>
            </div>
          </div>
        `;

        lista.appendChild(box);
      });

      aggiornaConteggiConFiltriCombinati(); // funzione di aggiornamento contatori
    })
    .catch(err => {
      console.error("Errore nel caricamento tasks per utente e categoria:", err);
      const lista = document.getElementById('lista-box');
      lista.innerHTML = `
        <div class="text-center text-danger my-4">
          Errore nel caricamento delle task filtrate
        </div>
      `;
    });
}


function caricaTasksCompletatePerUtenteECategoria(utenteId, categoriaId) {
  fetch(`https://localhost:7000/api/Task/UtenteCategoriaCompletate/${utenteId}/${categoriaId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      if (!tasks || tasks.length === 0) {
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Nessuna task completata trovata per questo utente e categoria...';
        lista.appendChild(msg);
        return;
      }

      tasks.forEach(task => {
  const scadenza = new Date(task.scadenza);
  const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
  const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

  const taskClass = task.stato ? 'completed' : '';
  const box = document.createElement('div');
  box.className = `card mb-2 w-100 ${taskClass}`;
  box.setAttribute('data-task-id', task.id);

  box.innerHTML = `
    <div class="card-body">
      <div class="row align-items-center w-100">
        <div class="col-auto d-flex align-items-center">
          <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.4);" checked
            onchange="toggleStato(${task.id}, this.checked, this)">
        </div>
        <div class="col d-flex flex-column">
          <span class="task-title"><strong>${task.titolo}</strong></span>
          <small class="text-muted">Scadenza: ${scadenzaFormattata}</small>
        </div>
        <div class="col-auto d-flex gap-2">
          <button class="btn btn-light rounded-circle" title="Sottotask" onclick="caricaSottoTask(${task.id})">
            <i class="bi bi-list-task"></i>
          </button>
          <button class="btn btn-light rounded-circle" title="Nota" onclick="notaTask(${task.id})">
            <i class="bi bi-sticky"></i>
          </button>
          <button class="btn btn-light rounded-circle" title="Elimina" onclick="eliminaTask(${task.id})">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </div>
    </div>
  `;

  lista.appendChild(box);
});


      aggiornaConteggiConFiltriCombinati();
    })
    .catch(err => {
      console.error("Errore nel caricamento tasks completate per utente e categoria:", err);
      const lista = document.getElementById('lista-box');
      lista.innerHTML = `
        <div class="text-center text-danger my-4">
          Errore nel caricamento delle task completate
        </div>
      `;
    });
}



function aggiornaConteggiConFiltriCombinati() {
  const badgeNonFatte = document.getElementById('numero-sezione');
  const badgeCompletate = document.getElementById('numero-sezione-si');

  if (utenteSelezionato && categoriaSelezionata) {
    fetch(`https://localhost:7000/api/Task/UtenteCategoria/${utenteSelezionato}/${categoriaSelezionata}`)
      .then(res => res.json())
      .then(tasks => {
        const nonCompletate = tasks.filter(task => !task.stato).length;
        const completate = tasks.filter(task => task.stato).length;

        if (badgeNonFatte) badgeNonFatte.textContent = nonCompletate;
        if (badgeCompletate) badgeCompletate.textContent = completate;
      })
      .catch(err => {
        console.error("Errore nel conteggio con filtri combinati:", err);
        if (badgeNonFatte) badgeNonFatte.textContent = '0';
        if (badgeCompletate) badgeCompletate.textContent = '0';
      });
  } else {
    aggiornaNumeroSezione();
    aggiornaNumeroSezioneCompletate();
  }
}

// Funzione per resettare completamente tutti i filtri
function resettaTuttiFiltri() {
  utenteSelezionato = null;
  categoriaSelezionata = null;
  document.getElementById('utente-in-uso').textContent = 'Nessun utente selezionato';
  document.getElementById('categoria-in-uso').textContent = 'Nessuna categoria selezionata';
  
  // Carica le task appropriate in base alla pagina corrente
  caricaTasksConFiltri();
}

// Event Listeners
document.addEventListener('DOMContentLoaded', () => {
  // Imposta il min della data di scadenza a oggi
  const scadenzaInput = document.getElementById('scadenza');
  if (scadenzaInput) {
    const today = new Date();
    const yyyy = today.getFullYear();
    const mm = String(today.getMonth() + 1).padStart(2, '0');
    const dd = String(today.getDate()).padStart(2, '0');
    const minDate = `${yyyy}-${mm}-${dd}`;
    scadenzaInput.setAttribute('min', minDate);
  }

  // Inizializza gestione eliminazione
  gestioneEliminazioneTask();

  // Inizializza badge categoria
  document.getElementById('utente-in-uso').textContent = 'Nessun utente selezionato';
  document.getElementById('categoria-in-uso').textContent = 'Nessuna categoria selezionata';
  Promise.all([
    caricaTasksConFiltri(),
    caricaCategorie(),
    caricaUtentiForm()
  ]);
});

// Event listener per form task
document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('taskForm');
  if (form) {
    form.addEventListener('submit', salvaTask);
  }
});
// Event listener per conferma completamento task
document.getElementById('btnConfermaCompleta').addEventListener('click', function () {
  if (taskIdDaCompletare !== null) {
    aggiornaStatoTask(taskIdDaCompletare, true);
    taskIdDaCompletare = null;
    checkboxDaRipristinare = null;
    var modal = bootstrap.Modal.getInstance(document.getElementById('confermaCompletaModal'));
    modal.hide();
  }
});

// Event listener per chiusura modal completa
document.getElementById('confermaCompletaModal').addEventListener('hidden.bs.modal', function () {
  if (checkboxDaRipristinare) {
    checkboxDaRipristinare.checked = false;
    checkboxDaRipristinare = null;
  }
  taskIdDaCompletare = null;
});

// Event listener per conferma utente
document.getElementById('confermaUtenteBtn').addEventListener('click', function () {
  const utenteId = document.getElementById('utenteDropdown').value;
  const utenteSelect = document.getElementById('utenteDropdown');
  const nomeUtente = utenteSelect.options[utenteSelect.selectedIndex].textContent;

  if (utenteId === "tutti") {
    utenteSelezionato = null;
    // NON resettare la categoria, mantieni il filtro categoria se presente
    document.getElementById('utente-in-uso').textContent = "Tutti";

    // Carica le task appropriate in base alla pagina corrente
    caricaTasksConFiltri();

    const modal = bootstrap.Modal.getInstance(document.getElementById('scegliUtenteModal'));
    modal.hide();
    return;
  }
  if (utenteId && utenteId !== "") {
    utenteSelezionato = utenteId;
    document.getElementById('utente-in-uso').textContent = nomeUtente;

    // Carica le task con i filtri combinati
    caricaTasksConFiltri();

    const modal = bootstrap.Modal.getInstance(document.getElementById('scegliUtenteModal'));
    modal.hide();
  } else if (utenteId === "") {
    alert('Seleziona un utente!');
  }
});

// Event listener per conferma categoria
document.getElementById('confermaCategoriaBtn').addEventListener('click', function () {
  const CategoriaID = document.getElementById('categoriaDropdown').value;
  const categoriaSelect = document.getElementById('categoriaDropdown');
  const nomeCategoria = categoriaSelect.options[categoriaSelect.selectedIndex].textContent;

  if (CategoriaID === "tutti") {
    // Reset solo la categoria, mantieni l'utente se selezionato
    categoriaSelezionata = null;
    document.getElementById('categoria-in-uso').textContent = "Tutti";

    // Carica le task appropriate in base alla pagina corrente
    caricaTasksConFiltri();

    const modal = bootstrap.Modal.getInstance(document.getElementById('scegliCategoriaModal'));
    modal.hide();
    return;
  }
  if (CategoriaID) {
    categoriaSelezionata = CategoriaID;
    document.getElementById('categoria-in-uso').textContent = nomeCategoria;

    // Carica le task con i filtri combinati
    caricaTasksConFiltri();

    const modal = bootstrap.Modal.getInstance(document.getElementById('scegliCategoriaModal'));
    modal.hide();
  } else {    alert('Seleziona una categoria!');
  }
});

// Event listener per form categoria
document.addEventListener('DOMContentLoaded', () => {
  const formCategoria = document.getElementById('formAggiungiCategoria');
  if (formCategoria) {
    formCategoria.addEventListener('submit', aggiungiCategoria);
  }
});

// Event listener per form utente
document.addEventListener('DOMContentLoaded', () => {
  const formUtente = document.getElementById('formAggiungiUtente');
  if (formUtente) {
    formUtente.addEventListener('submit', aggiungiUtente);
  }
});

const btnConfermaEliminaCategoria = document.getElementById('confermaEliminaCategoriaBtn');
const selectEliminaCategoria = document.getElementById('eliminaCategoriaDropdown');

if (btnConfermaEliminaCategoria && selectEliminaCategoria) {
  btnConfermaEliminaCategoria.addEventListener('click', () => {
    const categoriaId = selectEliminaCategoria.value;
    if (!categoriaId) {
      alert("Seleziona una categoria da eliminare.");
      return;
    }

    if (confirm("Sei sicuro di voler eliminare questa categoria?")) {
      fetch(`https://localhost:7000/api/Categorie/${categoriaId}`, {
        method: 'DELETE'
      })
        .then(res => {
          if (!res.ok) throw new Error('Errore nell\'eliminazione categoria');
          alert("Categoria eliminata!");
          caricaCategorie && caricaCategorie();
          const modal = bootstrap.Modal.getInstance(document.getElementById('eliminaCategoriaModal'));
          modal.hide();
        })
        .catch(err => alert(err.message));
    }
  });
}


// Event listener per conferma eliminazione utente
const btnConfermaEliminaUtente = document.getElementById('confermaEliminaUtenteBtn');
const selectEliminaUtente = document.getElementById('eliminaUtenteDropdown');

if (btnConfermaEliminaUtente && selectEliminaUtente) {
  btnConfermaEliminaUtente.addEventListener('click', () => {
    const utenteId = selectEliminaUtente.value;
    if (!utenteId) {
      alert("Seleziona un utente da eliminare.");
      return;
    }

    if (confirm("Sei sicuro di voler eliminare questo utente?")) {
      fetch(`https://localhost:7000/api/Utente/${utenteId}`, {
        method: 'DELETE'
      })
        .then(res => {
          if (!res.ok) throw new Error('Errore nell\'eliminazione utente');
          alert("Utente eliminato!");
          caricaUtentiForm && caricaUtentiForm();
          caricaUtentiDropdown && caricaUtentiDropdown();
          const modal = bootstrap.Modal.getInstance(document.getElementById('eliminaUtenteModal'));
          modal.hide();
        })
        .catch(err => alert(err.message));
    }
  });
}

// === FUNZIONE CONFERMA MODIFICA SOTTOTASK ===
function confermaSalvaModificaSottoTask() {
  const nuovoTitolo = document.getElementById('titoloSottoTaskModifica').value.trim();
  if (!nuovoTitolo) {
    alert('Il titolo non può essere vuoto');
    return;
  }
  fetch(`https://localhost:7000/api/SottoTask/${sottoTaskDaModificare}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ titolo: nuovoTitolo, taskID: taskIdCorrente })
  })
    .then(res => {
      if (!res.ok) throw new Error('Errore nella modifica della sottotask');
      return res.json();
    })
    .then(() => {
      const modal = bootstrap.Modal.getInstance(document.getElementById('modificaSottoTaskModal'));
      modal.hide();
      caricaSottoTask(taskIdCorrente, '', true);
      sottoTaskDaModificare = null;
      taskIdCorrente = null;
    })
    .catch(err => alert('Errore nella modifica della sottotask: ' + err.message));
}

// === FUNZIONE CONFERMA ELIMINAZIONE SOTTOTASK ===
function confermaEliminaSottoTask() {
  fetch(`https://localhost:7000/api/SottoTask/${sottoTaskDaEliminare}`, {
    method: 'DELETE'
  })
    .then(res => {
      if (!res.ok) throw new Error('Errore nell\'eliminazione della sottotask');
      return res.json();
    })
    .then(() => {
      const modal = bootstrap.Modal.getInstance(document.getElementById('eliminaSottoTaskModal'));
      modal.hide();
      caricaSottoTask(taskIdCorrente, '', true);
      sottoTaskDaEliminare = null;
      taskIdCorrente = null;
    })
    .catch(err => alert('Errore nell\'eliminazione della sottotask: ' + err.message));
}

// === LISTENER UNIFICATI PER LE MODALI SOTTOTASK ===
document.addEventListener('DOMContentLoaded', () => {
  const btnConfermaSalvaModifica = document.getElementById('btnConfermaSalvaModificaSottoTask');
  if (btnConfermaSalvaModifica) {
    btnConfermaSalvaModifica.addEventListener('click', confermaSalvaModificaSottoTask);
  }
  const btnConfermaEliminaSottoTask = document.getElementById('btnConfermaEliminaSottoTask');
  if (btnConfermaEliminaSottoTask) {
    btnConfermaEliminaSottoTask.addEventListener('click', confermaEliminaSottoTask);
  }
  // Reset variabili quando si chiudono le modali
  const modificaSottoTaskModal = document.getElementById('modificaSottoTaskModal');
  if (modificaSottoTaskModal) {
    modificaSottoTaskModal.addEventListener('hidden.bs.modal', () => {
      sottoTaskDaModificare = null;
      taskIdCorrente = null;
      document.getElementById('titoloSottoTaskModifica').value = '';
    });
  }
  const eliminaSottoTaskModal = document.getElementById('eliminaSottoTaskModal');
  if (eliminaSottoTaskModal) {
    eliminaSottoTaskModal.addEventListener('hidden.bs.modal', () => {
      sottoTaskDaEliminare = null;
      taskIdCorrente = null;
    });
  }

  // Gestione eliminazione di tutte le task completate
  const btnConfermaTutteTask = document.getElementById('btnConfermaTutteTask');
  if (btnConfermaTutteTask) {
    btnConfermaTutteTask.addEventListener('click', confermaEliminazioneTutteTaskCompletate);
  }
});

// Funzione per aprire il modal di conferma eliminazione tutte le task completate
function eliminaTutteTaskCompletate() {
  // Ripristina il contenuto originale del modal
  const modalTitle = document.getElementById('confermaTutteTaskModalLabel');
  const modalBody = document.querySelector('#confermaTutteTaskModal .modal-body');
  const modalFooter = document.querySelector('#confermaTutteTaskModal .modal-footer');
  
  modalTitle.textContent = 'Eliminazione di Tutte le Task Completate';
  modalBody.innerHTML = 'Sei sicuro di voler eliminare <strong>tutte</strong> le task completate?';
  modalFooter.innerHTML = `
    <button type="button" class="btn btn-primary" data-bs-dismiss="modal">Annulla</button>
    <button type="button" class="btn btn-primary" id="btnConfermaTutteTask">Elimina</button>
  `;
  
  // Riattacca l'event listener al nuovo bottone
  const btnConfermaTutteTask = document.getElementById('btnConfermaTutteTask');
  if (btnConfermaTutteTask) {
    btnConfermaTutteTask.addEventListener('click', confermaEliminazioneTutteTaskCompletate);
  }
  
  const modal = new bootstrap.Modal(document.getElementById('confermaTutteTaskModal'));
  modal.show();
}

// Funzione per confermare ed eseguire l'eliminazione di tutte le task completate
async function confermaEliminazioneTutteTaskCompletate() {
  try {
    // Prima otteniamo tutte le task completate
    let url = 'https://localhost:7000/api/Task';
    
    if (categoriaSelezionata) {
      url = `https://localhost:7000/api/Task/Categoria/${categoriaSelezionata}`;
    } else if (utenteSelezionato) {
      url = `https://localhost:7000/api/Task/Utente/${utenteSelezionato}`;
    }

    const response = await fetch(url);
    const tasks = await response.json();
    
    // Filtriamo solo le task completate
    const taskCompletate = tasks.filter(task => task.stato === true);
    
    if (taskCompletate.length === 0) {
      // Cambia il contenuto del modal per mostrare il messaggio
      const modalTitle = document.getElementById('confermaTutteTaskModalLabel');
      const modalBody = document.querySelector('#confermaTutteTaskModal .modal-body');
      const modalFooter = document.querySelector('#confermaTutteTaskModal .modal-footer');
      
      modalTitle.textContent = 'Nessuna Task da Eliminare';
      modalBody.innerHTML = '<div class="text-center"><p>Non ci sono task completate da eliminare.</p></div>';
      modalFooter.innerHTML = '<button type="button" class="btn btn-primary" data-bs-dismiss="modal">OK</button>';
      
      return;
    }

    // Eliminiamo tutte le task completate una per una
    const eliminazioniPromises = taskCompletate.map(task => 
      fetch(`https://localhost:7000/api/Task/${task.id}`, {
        method: 'DELETE'
      })
    );

    await Promise.all(eliminazioniPromises);

    // Chiudiamo il modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('confermaTutteTaskModal'));
    modal.hide();

    // Ricarichiamo la lista delle task
    caricaTasksConFiltri();

  } catch (error) {
    console.error('Errore durante l\'eliminazione delle task completate:', error);
    alert('Errore durante l\'eliminazione delle task completate: ' + error.message);
  }
}