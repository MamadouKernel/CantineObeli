# Configuration de la Base de Données

L'application Obeli supporte deux types de bases de données :
- **SQL Server** (par défaut)
- **PostgreSQL**

## Changer de Base de Données

### 1. Configuration dans appsettings.json

Modifiez le paramètre `DatabaseProvider` dans `appsettings.json` ou `appsettings.Development.json` :

```json
{
  "DatabaseProvider": "SqlServer",  // ou "PostgreSQL"
  "ConnectionStrings": {
    "SqlServerConnection": "Server=VOTRE_SERVEUR;Database=VOTRE_BASE;User Id=UTILISATEUR;Password=MOT_DE_PASSE;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;",
    "PostgreSqlConnection": "Host=VOTRE_SERVEUR;Port=5432;Database=VOTRE_BASE;Username=UTILISATEUR;Password=MOT_DE_PASSE;Pooling=true;"
  }
}
```

### 2. Valeurs Acceptées pour DatabaseProvider

- `SqlServer` ou `mssql` - Pour utiliser SQL Server
- `PostgreSQL` ou `postgres` - Pour utiliser PostgreSQL

### 3. Format des Chaînes de Connexion

#### SQL Server
```
Server=NOM_SERVEUR;Database=NOM_BASE;User Id=UTILISATEUR;Password=MOT_DE_PASSE;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;
```

Ou avec authentification Windows :
```
Server=NOM_SERVEUR;Database=NOM_BASE;Trusted_Connection=True;Encrypt=False;
```

#### PostgreSQL
```
Host=NOM_SERVEUR;Port=5432;Database=NOM_BASE;Username=UTILISATEUR;Password=MOT_DE_PASSE;Pooling=true;
```

### 4. Installation des Packages

Les packages nécessaires sont déjà inclus dans le projet :
- `Microsoft.EntityFrameworkCore.SqlServer` - Pour SQL Server
- `Npgsql.EntityFrameworkCore.PostgreSQL` - Pour PostgreSQL

Si vous devez les réinstaller :
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.8
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.2
```

### 5. Migrations

Après avoir changé de base de données, vous devez créer et appliquer les migrations :

#### Pour SQL Server
```bash
# Définir le provider
$env:DatabaseProvider="SqlServer"

# Créer une migration
dotnet ef migrations add InitialCreate --context ObeliDbContext

# Appliquer les migrations
dotnet ef database update --context ObeliDbContext
```

#### Pour PostgreSQL
```bash
# Définir le provider
$env:DatabaseProvider="PostgreSQL"

# Créer une migration
dotnet ef migrations add InitialCreate --context ObeliDbContext

# Appliquer les migrations
dotnet ef database update --context ObeliDbContext
```

### 6. Différences entre SQL Server et PostgreSQL

#### Noms de Tables et Colonnes
- **SQL Server** : Insensible à la casse par défaut
- **PostgreSQL** : Sensible à la casse (utilise des minuscules par défaut)

#### Types de Données
Certains types peuvent différer :
- `datetime` (SQL Server) vs `timestamp` (PostgreSQL)
- `nvarchar(max)` (SQL Server) vs `text` (PostgreSQL)

#### Fonctions SQL
Certaines fonctions SQL natives peuvent différer entre les deux bases.

### 7. Exemple Complet

#### Configuration pour SQL Server (Production)
```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "SqlServerConnection": "Server=10.88.179.104;Database=Kobeli_db;User Id=sa;Password=Termin@l2024!;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true;Connection Timeout=30;",
    "PostgreSqlConnection": "Host=localhost;Port=5432;Database=Kobeli_db;Username=postgres;Password=YourPassword;Pooling=true;"
  }
}
```

#### Configuration pour PostgreSQL (Développement)
```json
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=Kobeli_db;Trusted_Connection=True;Encrypt=False;",
    "PostgreSqlConnection": "Host=localhost;Port=5432;Database=kobeli_dev;Username=postgres;Password=dev123;Pooling=true;"
  }
}
```

### 8. Vérification

Au démarrage de l'application, vous verrez dans les logs :
```
🔧 Provider de base de données: SqlServer
✅ Configuration SQL Server activée
```

ou

```
🔧 Provider de base de données: PostgreSQL
✅ Configuration PostgreSQL activée
```

### 9. Dépannage

#### Erreur : "La chaîne de connexion est manquante"
Vérifiez que vous avez bien défini la chaîne de connexion correspondant au provider choisi.

#### Erreur de connexion PostgreSQL
- Vérifiez que PostgreSQL est démarré
- Vérifiez le port (5432 par défaut)
- Vérifiez que l'utilisateur a les droits nécessaires

#### Erreur de connexion SQL Server
- Vérifiez que SQL Server est démarré
- Vérifiez l'authentification (Windows ou SQL Server)
- Vérifiez les paramètres de pare-feu

### 10. Recommandations

- **Production** : Utilisez SQL Server pour la stabilité et les performances
- **Développement** : PostgreSQL peut être utilisé pour réduire les coûts de licence
- **Tests** : Les deux bases peuvent être utilisées selon vos besoins

## Support

Pour toute question ou problème, consultez la documentation officielle :
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [SQL Server](https://docs.microsoft.com/sql/)
- [PostgreSQL](https://www.postgresql.org/docs/)
