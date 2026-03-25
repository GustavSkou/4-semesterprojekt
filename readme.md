##  Run the C# Project (ProductionSystem)

```bash
dotnet build ProductionSystem/PluginHost/PluginHost.csproj
dotnet run --project ProductionSystem/PluginHost/PluginHost.csproj
```

###  Step-by-step (C#)
1. (Kun hvis du har dubletter i `ProductionSystem/Plugins`) ryd mappen:
```bash
rm -rf ProductionSystem/Plugins
```
2. Byg så plugins bliver kopieret korrekt:
```bash
dotnet build ProductionSystem/PluginHost/PluginHost.csproj
```
3. Start serveren:
```bash
dotnet run --project ProductionSystem/PluginHost/PluginHost.csproj
```
4. Tjek at den svarer:
```bash
curl http://localhost:5027/ProductionSystem/TEST
```

---

##  Run the Laravel App (ConfigurePcLaravel)
1. Gå ind i appen:
```bash
cd ConfigurePcLaravel/configurePc-App
```
2. Installer dependencies:
```bash
composer install
npm install
```
3. Lav `.env` og generer key:
```bash
cp .env.example .env
php artisan key:generate
```
4. Byg frontend assets (nødvendig for `/` route):
```bash
npm run build
```
5. Migrér og seed DB:
```bash
php artisan migrate:fresh --seed
```
6. Start server:
```bash
php artisan serve
```

---

##  Tests
###  Laravel integration tests
```bash
cd ConfigurePcLaravel/configurePc-App
php artisan test
```

###  ProductionSystem unit tests
```bash
dotnet test ProductionSystem/ProductionSystem.UnitTests/ProductionSystem.UnitTests.csproj
```

---

##  Troubleshooting (C#)
- Hvis du får fejl om dublet-plugins eller "SendCommand does not have implementation", så er `ProductionSystem/Plugins` fyldt med dubletter. Ryd mappen og byg igen:
```bash
rm -rf ProductionSystem/Plugins
dotnet build ProductionSystem/PluginHost/PluginHost.csproj
```

---

##  Login (Operator)

**Email:**

```text
operator@example.com

```

**Password:**

```text
1234
```
