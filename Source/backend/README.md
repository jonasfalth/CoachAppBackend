# Coach App Backend

## Säkerhetskonfiguration

### JWT-nyckel
För att generera en säker JWT-nyckel, kör följande kommando i PowerShell:

```powershell
$bytes = New-Object Byte[] 32
$rand = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rand.GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

Kopiera den genererade strängen och använd den som `JwtSettings:SecretKey` i produktionsmiljön.

### Produktionsmiljö
1. Skapa en miljövariabel `JWT_SECRET_KEY` med den genererade nyckeln
2. Uppdatera `AllowedOrigins` i `appsettings.Production.json` med dina faktiska domäner
3. Säkerställ att databasmapparna har korrekta behörigheter:
   ```bash
   chmod 700 /app/data
   chmod 700 /app/logs
   ```

### Loggning
- Loggfiler roteras dagligen
- Maximalt 31 dagars loggar sparas
- Loggar sparas i `/app/logs` i produktion
- Känslig information loggas aldrig

### Databashantering
- Databaser sparas i `/app/data` i produktion
- Säkerställ regelbunden backup av databaserna
- Använd transaktioner för kritiska operationer

## Utvecklingsmiljö
1. Kopiera `appsettings.json` till `appsettings.Development.json`
2. Uppdatera inställningarna för utveckling
3. Använd SQLite för lokal utveckling

## Säkerhetskontroller
- [ ] JWT-nyckel är minst 32 bytes
- [ ] CORS är korrekt konfigurerad för produktion
- [ ] Loggfiler roteras och rensas
- [ ] Databaser har säkra behörigheter
- [ ] Känslig information är skyddad
- [ ] Alla endpoints är skyddade med autentisering
- [ ] HTTPS är aktiverat i produktion 