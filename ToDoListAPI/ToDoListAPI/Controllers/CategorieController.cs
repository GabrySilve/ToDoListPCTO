using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Data;
using ToDoListAPI.Models;

namespace ToDoListAPI.Controllers  //LA CARTELLA CONTROLLERS SI OCCUPA SI GESTIRE, CONTROLLARE...UTENTI,TASK E CATEGORIA ANDANDO A CREARE, RECUPERARE O ELIMINARE SECONDO I LORO ID
{
    [ApiController]
    [Route("api/[controller]")]   //Controller API per la gestione delle categorie.
    public class CategorieController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategorieController(ApplicationDbContext context) // Espone le seguenti operazioni HTTP:
        {
            _context = context; //Utilizza ApplicationDbContext per l'accesso al database tramite Entity Framework Core.
        }

        [HttpGet]  //Recupera tutte le categorie.
        public IActionResult GetAll()
        {
            var categorie = _context.Categorie.ToList();
            return Ok(categorie);
        }

        [HttpGet("{id}")] //Recupera una categoria specifica tramite ID.
        public IActionResult GetById(int id)
        {
            var categoria = _context.Categorie.Find(id);
            if (categoria == null)
                return NotFound();

            return Ok(categoria);
        }

        [HttpPost]
        public IActionResult Create([FromBody] CategorieDto dto) //Crea una nuova categoria(con dati nel body).
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoria = new Data.Categorie
            {
                Descrizione = dto.Descrizione,
            };

            _context.Categorie.Add(categoria);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetAll), new { id = categoria.ID }, categoria);
        }

        [HttpPut("{id}")]   

        public IActionResult Update(int id, [FromBody] CategorieDto updated)   // Aggiorna una categoria esistente tramite ID.
        {
            var categoria = _context.Categorie.Find(id);
            if (categoria == null)
                return NotFound();

            categoria.Descrizione = updated.Descrizione;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var categoria = _context.Categorie.Find(id);
            if (categoria == null)
                return NotFound();

            // Trova le task che usano questa categoria
            var tasksConQuestaCategoria = _context.Task
                .Where(t => t.CategoriaID == id)
                .ToList();

            // Imposta CategoriaID a null per ciascuna task
            foreach (var task in tasksConQuestaCategoria)
            {
                task.CategoriaID = null;
            }

            // Ora puoi rimuovere la categoria
            _context.Categorie.Remove(categoria);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
/*

 * 


 
 
 * 
 * Utilizza ApplicationDbContext per l'accesso al database tramite Entity Framework Core.
 * Implementa controlli di validità dei dati e risposte HTTP appropriate (200 OK, 201 Created, 204 NoContent, 400 BadRequest, 404 NotFound).
 */