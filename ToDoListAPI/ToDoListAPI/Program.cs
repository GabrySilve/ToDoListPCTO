using Microsoft.EntityFrameworkCore;
using ToDoListAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔧 CONFIGURA CORS (prima di Build)


// Configurazione di CORS (Cross-Origin Resource Sharing) per permettere richieste da qualsiasi origine,
// con qualsiasi metodo e header — utile durante lo sviluppo per evitare problemi di accesso da client esterni.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // In sviluppo va bene così
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Aggiunta dei servizi fonamentali
builder.Services.AddControllers(); // - Controller per gestire le API REST.
builder.Services.AddEndpointsApiExplorer(); // - EndpointApiExplorer e Swagger per generare e mostrare la documentazione API.
builder.Services.AddSwaggerGen();

// 🔧 REGISTRA IL DbContext
// configurato per usare SQL Server con la stringa di connessione definita nel file di configurazione.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configurazione di Swagger solo in ambiente di sviluppo,
// per visualizzare e testare le API tramite interfaccia web.app.UseCors("AllowAll");

// Swagger solo in sviluppo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Forza l’uso di HTTPS per tutte le richieste.
app.UseCors("AllowAll");
app.UseAuthorization();  // Middleware per l’autorizzazione (anche se non configurata esplicitamente qui).
app.MapControllers();     // Mappa le rotte ai controller definiti.
app.Run();       // Avvia l’applicazione.

