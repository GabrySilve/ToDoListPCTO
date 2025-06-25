# 1. Esportare (scaricare) un database da SQL Server
Hai due opzioni: **backup `.bak`** oppure **script SQL**. Si consiglia il **backup `.bak`** perché è più completo.
## Metodo consigliato: Backup (.bak)
### Passaggi:
1. Apri **SQL Server Management Studio (SSMS)** e collegati al tuo server.
2. Trova il database da esportare:
   - Nella finestra **Object Explorer**, espandi **Databases**.
   - Clicca con il tasto destro sul database che vuoi esportare.
3. Vai su:  
   `Tasks → Back Up...`
4. Nella finestra **Back Up Database**:
   - Assicurati che il tipo sia **Full**.
   - In **Destination**, rimuovi eventuali voci presenti e premi **Add**.
   - Scegli un percorso (es: `C:\Backup\miodatabase.bak`), assegna un nome e clicca **OK**.
5. Premi **OK** per avviare il backup.
Dopo pochi secondi avrai un file `.bak` da copiare su una chiavetta o su un altro PC.
# 2. Importare (caricare) un database da file .bak su un altro PC
### Passaggi:
1. Apri **SSMS** sul nuovo PC e collegati al server.
2. Crea una cartella, ad esempio `C:\Backup`, e copia lì il file `.bak`.
3. In **SSMS**, clicca con il tasto destro su **Databases** → **Restore Database...**
4. Nella finestra:
   - Seleziona **Device**
   - Clicca su `...` → **Add** → scegli il file `.bak` → **OK**
   - Nella sezione **"Select the backup sets to restore"**, spunta il backup da ripristinare.
   - *(Opzionale)* Vai nella scheda **Files** e controlla dove verrà salvato fisicamente il database sul disco.
5. Premi **OK** per avviare il ripristino.
# Note importanti
- Il percorso in cui si trova il file `.bak` deve avere i **permessi di lettura** per SQL Server.
- Se ricevi errori, copia il file in una cartella come:  
  `C:\Program Files\Microsoft SQL Server\MSSQLXX.MSSQLSERVER\MSSQL\Backup`
- Verifica che l’**istanza SQL Server** abbia i permessi di accesso al file: puoi configurarli tramite i **permessi di sicurezza di Windows**.
