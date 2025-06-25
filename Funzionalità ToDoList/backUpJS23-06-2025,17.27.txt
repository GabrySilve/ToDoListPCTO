let modalDiv = null;
let taskDaModificare = null;
let taskIdDaEliminare = null;
let taskIdDaCompletare = null;
let checkboxDaRipristinare = null;
let utenteSelezionato = null;
let categoriaSelezionata = null;

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
  fetch(`https://localhost:7000/api/Task/UtenteStatoNo/${UtenteId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';
      if (!tasks || tasks.length === 0) {
        // Mostra il messaggio se non ci sono task
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Ancora nessuna task...';
        lista.appendChild(msg);
      } else {
        tasks.forEach(task => {
          const scadenza = new Date(task.scadenza);
          const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
          const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

          const box = document.createElement('div');
          box.className = 'card mb-2 w-100';
          box.setAttribute('data-task-id', task.id);
          box.innerHTML = `
            <div class="card-body d-flex align-items-center p-2">
              <div class="d-flex align-items-center flex-wrap gap-3">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.5);"
                ${task.stato ? 'checked' : ''} onchange="toggleStato(${task.id}, this.checked, this)">
                <span><strong>Titolo:</strong> ${task.titolo}</span>
                <span class="ms-3"><strong>Scadenza:</strong> ${scadenzaFormattata}</span>
              </div>
              <div class="d-flex ms-auto gap-2">
                <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 48px; height: 48px; padding: 0;"
                        onclick="caricaSottoTask(${task.id})" title="Visualizza sottotask">
                  <i class="bi bi-list-task" style="font-size: 2rem; font-weight: bold;"></i>
                </button>
                <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 48px; height: 48px; padding: 0;"
                        onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky" style="font-size: 2rem; font-weight: bold;"></i>
                </button>
                <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 48px; height: 48px; padding: 0;"
                        onclick="modificaTask(${task.id})">
                  <i class="bi bi-pencil" style="font-size: 2rem; font-weight: bold;"></i>
                </button>
                <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 48px; height: 48px; padding: 0;"
                        onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash" style="font-size: 2rem; font-weight: bold;"></i>
                </button>
              </div>
            </div>
          `;
          lista.appendChild(box);
        });
      }
      mostraNumeroTaskNonFattePerUtente(UtenteId);
      mostraNumeroTaskCompletatePerUtente(UtenteId);
    })
    .catch(err => alert("Errore nel caricamento tasks per utente: " + err.message));
}

function caricaSottoTask(taskId) {
  // Trova la card della task selezionata
  const card = document.querySelector(`.card[data-task-id='${taskId}']`);
  if (!card) return;

  // Se la sottotask-container esiste già subito dopo la card, la rimuovo (toggle)
  const nextElem = card.nextElementSibling;
  if (nextElem && nextElem.classList.contains('sottotask-container')) {
    nextElem.remove();
    return;
  }

  // Altrimenti, creo e inserisco la sottotask-container subito dopo la card
  fetch(`https://localhost:7000/api/SottoTask/Task/${taskId}`)
    .then(res => res.json())
    .then(sottoTasks => {
      // Rimuovi eventuali altre sottotask-container aperte
      document.querySelectorAll('.sottotask-container').forEach(el => el.remove());

      const container = document.createElement('div');
      container.className = 'sottotask-container';
      container.style.width = '100%';
      if (!sottoTasks || sottoTasks.length === 0) {
        container.innerHTML = '<div class="text-muted small ms-3">Nessun sotto-task</div>';
      } else {
        let items = sottoTasks.map(st =>
          `<li>${st.titolo}</li>`
        ).join('');
        container.innerHTML = `
          <ul class="mb-0">
            ${items}
          </ul>
        `;
      }
      // Inserisci subito dopo la card
      card.parentNode.insertBefore(container, card.nextSibling);
    })
    .catch(() => {
      // Rimuovi eventuali altre sottotask-container aperte
      document.querySelectorAll('.sottotask-container').forEach(el => el.remove());
      const container = document.createElement('div');
      container.className = 'sottotask-container';
      container.innerHTML = '<div class="text-danger small ms-3">Errore caricamento sotto-task</div>';
      card.parentNode.insertBefore(container, card.nextSibling);
    });
}

function resetUtenteVisualizzato() {
  // Carica le task appropriate in base alla pagina corrente
  if (window.location.pathname.endsWith('completate.html')) {
    caricaTasksCompletate();
  } else {
    caricaTasks();
  }
}

function caricaTasksPerCategoria(CategoriaId) {
  fetch(`https://localhost:7000/api/Task/Categoria/${CategoriaId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';
      tasks.forEach(task => {
        const scadenza = new Date(task.scadenza);
        const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        const box = document.createElement('div');
        box.className = 'card mb-2 w-100';
        box.setAttribute('data-task-id', task.id);
        box.innerHTML = `
          <div class="card-body d-flex align-items-center p-2">
            <div class="d-flex align-items-center flex-wrap gap-3">
              <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.5);"
              ${task.stato ? 'checked' : ''} onchange="toggleStato(${task.id}, this.checked, this)">
              <span><strong>Titolo:</strong> ${task.titolo}</span>
              <span class="ms-3"><strong>Scadenza:</strong> ${scadenzaFormattata}</span>
            </div>
            <div class="d-flex ms-auto gap-2">
              <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                      style="width: 48px; height: 48px; padding: 0;"
                      onclick="caricaSottoTask(${task.id})" title="Visualizza sottotask">
                <i class="bi bi-list-task" style="font-size: 2rem; font-weight: bold;"></i>
              </button>
              <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                      style="width: 48px; height: 48px; padding: 0;"
                      onclick="notaTask(${task.id})">
                <i class="bi bi-sticky" style="font-size: 2rem; font-weight: bold;"></i>
              </button>
              <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                      style="width: 48px; height: 48px; padding: 0;"
                      onclick="modificaTask(${task.id})">
                <i class="bi bi-pencil" style="font-size: 2rem; font-weight: bold;"></i>
              </button>
              <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                      style="width: 48px; height: 48px; padding: 0;"
                      onclick="eliminaTask(${task.id})">
                <i class="bi bi-trash" style="font-size: 2rem; font-weight: bold;"></i>
              </button>
            </div>
          </div>
        `;
        lista.appendChild(box);
      });
      mostraNumeroTaskNonFattePerCategoria(CategoriaId);
      mostraNumeroTaskCompletatePerCategoria(CategoriaId);
    })
    .catch(err => alert("Errore nel caricamento tasks per categoria: " + err.message));
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
        // Trasforma la stringa scadenza in oggetto Date
        const scadenza = new Date(task.scadenza);

        // Opzioni per formattare data e ora in italiano con giorno, mese, anno, ora e minuti
        const options = {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit'
        };

        // Formatta la data/ora
        const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

        const box = document.createElement('div');
        box.className = 'card mb-2 w-100';
        box.setAttribute('data-task-id', task.id);
        box.innerHTML = `
          <div class="card-body d-flex align-items-center p-2">
            <div class="d-flex align-items-center flex-wrap gap-3">
              <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.5);"
                ${task.stato ? 'checked' : ''} onchange="toggleStato(${task.id}, this.checked, this)">
              <span><strong>Titolo:</strong> ${task.titolo}</span>
              <span class="ms-3"><strong>Scadenza:</strong> ${scadenzaFormattata}</span>
            </div>
            <div class="d-flex ms-auto gap-2">
              <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                      style="width: 48px; height: 48px; padding: 0;"
                      onclick="caricaSottoTask(${task.id})" title="Visualizza sottotask">
                <i class="bi bi-list-task" style="font-size: 2rem; font-weight: bold;"></i>
              </button>
              <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                      style="width: 48px; height: 48px; padding: 0;"
                      onclick="notaTask(${task.id})">
                <i class="bi bi-sticky" style="font-size: 2rem; font-weight: bold;"></i>
              </button>
              <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                      style="width: 48px; height: 48px; padding: 0;"
                      onclick="modificaTask(${task.id})">
                <i class="bi bi-pencil" style="font-size: 2rem; font-weight: bold;"></i>
              </button>
              <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                      style="width: 48px; height: 48px; padding: 0;"
                      onclick="eliminaTask(${task.id})">
                <i class="bi bi-trash" style="font-size: 2rem; font-weight: bold;"></i>
              </button>
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
      taskDaModificare = null;
      document.getElementById('btnAggiungi').textContent = 'Aggiungi';
      if (utenteSelezionato) {
        caricaTasksPerUtente(utenteSelezionato);
      } else {
        caricaTasks();
      }
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
    .then(res => res.json())
    .then(() => {
      // Ricarica la lista in base al filtro attivo e alla pagina
      const isCompletate = window.location.pathname.endsWith('completate.html');
      if (isCompletate) {
        if (categoriaSelezionata) {
          caricaTasksCompletatePerCategoria(categoriaSelezionata);
        } else if (utenteSelezionato) {
          caricaTasksCompletatePerUtente(utenteSelezionato);
        } else {
          caricaTasksCompletate();
        }
        aggiornaNumeroSezioneCompletate();
      } else {
        if (categoriaSelezionata) {
          caricaTasksPerCategoria(categoriaSelezionata);
        } else if (utenteSelezionato) {
          caricaTasksPerUtente(utenteSelezionato);
        } else {
          caricaTasks();
        }
        aggiornaNumeroSezione();
        aggiornaNumeroSezioneCompletate();
      }
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
          console.log('utenteSelezionato:', utenteSelezionato);

          // Ricarica la lista giusta in base alla pagina e ai filtri attivi
          if (window.location.pathname.endsWith('completate.html')) {
            console.log('Siamo nella pagina completate');
            // Pagina completate - considera sia categoria che utente
            if (categoriaSelezionata) {
              console.log('Ricaricando per categoria:', categoriaSelezionata);
              caricaTasksCompletatePerCategoria(categoriaSelezionata);
            } else if (utenteSelezionato) {
              console.log('Ricaricando per utente:', utenteSelezionato);
              caricaTasksCompletatePerUtente(utenteSelezionato);
            } else {
              console.log('Ricaricando tutte le task completate');
              caricaTasksCompletate();
            }
          } else {
            console.log('Siamo nella pagina principale');
            // Pagina principale - considera sia categoria e utente
            if (categoriaSelezionata) {
              caricaTasksPerCategoria(categoriaSelezionata);
            } else if (utenteSelezionato) {
              caricaTasksPerUtente(utenteSelezionato);
            } else {
              caricaTasks();
            }
          }
          aggiornaNumeroSezione();
          aggiornaNumeroSezioneCompletate();
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

  // Se è selezionata una categoria specifica
  if (categoriaSelezionata) {
    url = `https://localhost:7000/api/Task/Categoria/${categoriaSelezionata}`;
  }
  // Altrimenti se è selezionato un utente specifico
  else if (utenteSelezionato) {
    url = `https://localhost:7000/api/Task/Utente/${utenteSelezionato}`;
  }
  // Altrimenti carica tutte le task

  fetch(url)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';

      // Filtra solo le task completate
      const tasksCompletate = tasks.filter(task => task.stato);

      if (tasksCompletate.length === 0) {
        // Mostra messaggio se non ci sono task completate
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Nessuna task completata...';
        lista.appendChild(msg);
      } else {
        tasksCompletate.forEach(task => {
          const scadenza = new Date(task.scadenza);
          const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
          const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

          const box = document.createElement('div');
          box.className = 'card mb-2 w-100';
          box.setAttribute('data-task-id', task.id);
          box.innerHTML = `
            <div class="card-body d-flex align-items-center p-2">
              <div class="d-flex align-items-center flex-wrap gap-3">
                <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.5);" checked 
                  onchange="toggleStato(${task.id}, this.checked, this)">
                <span><strong>Titolo:</strong> ${task.titolo}</span>
                <span class="ms-3"><strong>Scadenza:</strong> ${scadenzaFormattata}</span>
              </div>
              <div class="d-flex ms-auto gap-2">
                <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 48px; height: 48px; padding: 0;"
                        onclick="caricaSottoTask(${task.id})" title="Visualizza sottotask">
                  <i class="bi bi-list-task" style="font-size: 2rem; font-weight: bold;"></i>
                </button>
                <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 48px; height: 48px; padding: 0;"
                        onclick="notaTask(${task.id})">
                  <i class="bi bi-sticky" style="font-size: 2rem; font-weight: bold;"></i>
                </button>
                <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                        style="width: 48px; height: 48px; padding: 0;"
                        onclick="eliminaTask(${task.id})">
                  <i class="bi bi-trash" style="font-size: 2rem; font-weight: bold;"></i>
                </button>
              </div>
            </div>
          `;
          lista.appendChild(box);
        });
      }
      aggiornaNumeroSezioneCompletate();
    })
    .catch(err => {
      console.error("Errore nel caricamento delle task completate:", err);
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '<div class="text-center text-danger my-4">Errore nel caricamento delle task completate</div>';
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

      // Filtra solo le task completate
      const tasksCompletate = tasks.filter(task => task.stato);

      if (tasksCompletate.length === 0) {
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Nessuna task completata per questo utente...';
        lista.appendChild(msg);
      } else {
        tasksCompletate.forEach(task => {
          const scadenza = new Date(task.scadenza);
          const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
          const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

          const box = document.createElement('div');
          box.className = 'card mb-2 w-100';
          box.setAttribute('data-task-id', task.id);
          box.innerHTML = `
              <div class="card-body d-flex align-items-center p-2">
                <div class="d-flex align-items-center flex-wrap gap-3">
                  <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.5);" checked
                  onchange="toggleStato(${task.id}, this.checked, this)">
                  <span><strong>Titolo:</strong> ${task.titolo}</span>
                  <span class="ms-3"><strong>Scadenza:</strong> ${scadenzaFormattata}</span>
                </div>
                <div class="d-flex ms-auto gap-2">
                  <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                          style="width: 48px; height: 48px; padding: 0;"
                          onclick="caricaSottoTask(${task.id})" title="Visualizza sottotask">
                    <i class="bi bi-list-task" style="font-size: 2rem; font-weight: bold;"></i>
                  </button>
                  <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                          style="width: 48px; height: 48px; padding: 0;"
                          onclick="notaTask(${task.id})">
                    <i class="bi bi-sticky" style="font-size: 2rem; font-weight: bold;"></i>
                  </button>
                  <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                          style="width: 48px; height: 48px; padding: 0;"
                          onclick="eliminaTask(${task.id})">
                    <i class="bi bi-trash" style="font-size: 2rem; font-weight: bold;"></i>
                  </button>
                </div>
              </div>
            `;
          lista.appendChild(box);
        });
      }
      mostraNumeroTaskCompletatePerUtente(utenteId);
    })
    .catch(err => {
      console.error("Errore nel caricamento tasks completate per utente:", err);
      alert("Errore nel caricamento tasks completate per utente: " + err.message);
    });
}

function caricaTasksCompletatePerCategoria(categoriaId) {
  fetch(`https://localhost:7000/api/Task/CategoriaStatoSi/${categoriaId}`)
    .then(res => res.json())
    .then(tasks => {
      const lista = document.getElementById('lista-box');
      lista.innerHTML = '';
      console.log("Tasks completate per categoria:", tasks);
      // Filtra solo le task completate
      const tasksCompletate = tasks.filter(task => task.stato);

      if (tasksCompletate.length === 0) {
        const msg = document.createElement('div');
        msg.className = 'text-center text-muted my-4';
        msg.textContent = 'Nessuna task completata per questa categoria...';
        lista.appendChild(msg);
      } else {
        tasksCompletate.forEach(task => {
          const scadenza = new Date(task.scadenza);
          const options = { year: 'numeric', month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' };
          const scadenzaFormattata = scadenza.toLocaleString('it-IT', options);

          const box = document.createElement('div');
          box.className = 'card mb-2 w-100';
          box.setAttribute('data-task-id', task.id);
          box.innerHTML = `
              <div class="card-body d-flex align-items-center p-2">
                <div class="d-flex align-items-center flex-wrap gap-3">
                  <input type="checkbox" class="form-check-input me-3" style="transform: scale(1.5);" checked
                  onchange="toggleStato(${task.id}, this.checked, this)">
                  <span><strong>Titolo:</strong> ${task.titolo}</span>
                  <span class="ms-3"><strong>Scadenza:</strong> ${scadenzaFormattata}</span>
                </div>
                <div class="d-flex ms-auto gap-2">
                  <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                          style="width: 48px; height: 48px; padding: 0;"
                          onclick="caricaSottoTask(${task.id})" title="Visualizza sottotask">
                    <i class="bi bi-list-task" style="font-size: 2rem; font-weight: bold;"></i>
                  </button>
                  <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                          style="width: 48px; height: 48px; padding: 0;"
                          onclick="notaTask(${task.id})">
                    <i class="bi bi-sticky" style="font-size: 2rem; font-weight: bold;"></i>
                  </button>
                  <button class="btn btn-light rounded-circle d-flex align-items-center justify-content-center"
                          style="width: 48px; height: 48px; padding: 0;"
                          onclick="eliminaTask(${task.id})">
                    <i class="bi bi-trash" style="font-size: 2rem; font-weight: bold;"></i>
                  </button>
                </div>
              </div>
            `;
          lista.appendChild(box);
        });
      }
      mostraNumeroTaskCompletatePerCategoria(categoriaId);
    })
    .catch(err => {
      console.error("Errore nel caricamento tasks completate per categoria:", err);
      alert("Errore nel caricamento tasks completate per categoria: " + err.message);
    });
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
    caricaTasks(),
    caricaCategorie(),
    caricaUtentiForm()
  ]);
  aggiornaNumeroSezione();
  aggiornaNumeroSezioneCompletate();
});

// Event listener per form task
document.getElementById('taskForm').addEventListener('submit', salvaTask);

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
    categoriaSelezionata = null; // Reset anche categoria quando si seleziona "Tutti"
    document.getElementById('utente-in-uso').textContent = "Tutti";

    // Carica le task appropriate in base alla pagina corrente
    if (window.location.pathname.endsWith('completate.html')) {
      caricaTasksCompletate();
    } else {
      caricaTasks();
    }

    const modal = bootstrap.Modal.getInstance(document.getElementById('scegliUtenteModal'));
    modal.hide();
    return;
  }
  if (utenteId && utenteId !== "") {
    utenteSelezionato = utenteId;
    categoriaSelezionata = null; // Reset categoria quando si seleziona un utente specifico
    document.getElementById('utente-in-uso').textContent = nomeUtente;

    // Carica le task appropriate in base alla pagina corrente
    if (window.location.pathname.endsWith('completate.html')) {
      caricaTasksCompletatePerUtente(utenteId);
    } else {
      caricaTasksPerUtente(utenteId);
    }

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
    // Reset tutte le selezioni quando si sceglie "Tutti" per le categorie
    categoriaSelezionata = null;
    document.getElementById('categoria-in-uso').textContent = "Tutti";

    // Carica le task appropriate in base alla pagina corrente
    if (window.location.pathname.endsWith('completate.html')) {
      caricaTasksCompletate();
    } else {
      caricaTasks();
    }

    const modal = bootstrap.Modal.getInstance(document.getElementById('scegliCategoriaModal'));
    modal.hide();
    return;
  }
  if (CategoriaID) {
    categoriaSelezionata = CategoriaID;
    document.getElementById('categoria-in-uso').textContent = nomeCategoria;

    // Carica le task appropriate in base alla pagina corrente
    if (window.location.pathname.endsWith('completate.html')) {
      caricaTasksCompletatePerCategoria(CategoriaID);
    } else {
      caricaTasksPerCategoria(CategoriaID);
    }

    const modal = bootstrap.Modal.getInstance(document.getElementById('scegliCategoriaModal'));
    modal.hide();
  } else {
    alert('Seleziona una categoria!');
  }
});

// Event listener per form categoria
document.getElementById('formAggiungiCategoria').addEventListener('submit', aggiungiCategoria);

// Event listener per form utente
document.getElementById('formAggiungiUtente').addEventListener('submit', aggiungiUtente);

// Event listener per conferma eliminazione categoria
document.getElementById('confermaEliminaCategoriaBtn').addEventListener('click', () => {
  const categoriaId = document.getElementById('eliminaCategoriaDropdown').value;
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

// Event listener per conferma eliminazione utente
document.getElementById('confermaEliminaUtenteBtn').addEventListener('click', () => {
  const utenteId = document.getElementById('eliminaUtenteDropdown').value;
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