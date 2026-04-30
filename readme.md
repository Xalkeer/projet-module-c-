# ProjetTaskFlow

Une API RESTful pour la gestion de tâches et de projets, développée en .NET 9 avec authentification JWT et base de données SQLite.

## Description

ProjetTaskFlow est une application de gestion de tâches qui permet aux utilisateurs de créer et gérer des projets, d'assigner des tâches, et de suivre leur progression. L'API fournit une interface sécurisée pour l'inscription, la connexion, et la manipulation des données via des endpoints REST.

## Fonctionnalités

- **Authentification JWT** : Inscription et connexion des utilisateurs avec tokens sécurisés
- **Gestion des utilisateurs** : Rôles (User/Admin), profils utilisateur
- **Gestion des projets** : Création, lecture, mise à jour et suppression de projets
- **Gestion des tâches** : Assignation à des projets, statuts (ToDo/Ongoing/Finished), commentaires
- **Interface Swagger** : Documentation interactive des API en mode développement
- **Base de données SQLite** : Persistance légère et automatique

## Architecture

Le projet est structuré en 3 couches :

- **TaskFlowAPI** : API ASP.NET Core avec contrôleurs, middleware et sécurité
- **DAL** : Couche d'accès aux données avec Entity Framework Core
- **ProjetTaskFlow** : Application console (actuellement basique, pour extension future)

## Technologies utilisées

- **.NET 9** : Framework principal
- **ASP.NET Core** : Pour l'API web
- **Entity Framework Core** : ORM pour la base de données
- **SQLite** : Base de données
- **JWT Bearer** : Authentification
- **Swagger/OpenAPI** : Documentation API
- **Identity** : Hachage des mots de passe

## Modèle de données

### User
- `Id` (int) : Identifiant unique
- `Name` (string) : Nom de l'utilisateur
- `Email` (string) : Email unique
- `PasswordHash` (string) : Mot de passe haché
- `Role` (enum) : User ou Admin
- `Projects` : Liste des projets associés

### Project
- `Id` (int) : Identifiant unique
- `Name` (string) : Nom du projet
- `Description` (string) : Description optionnelle
- `CreationDate` (DateTime) : Date de création
- `User` : Propriétaire du projet
- `Tasks` : Liste des tâches

### Task
- `Id` (int) : Identifiant unique
- `Title` (string) : Titre de la tâche
- `Status` (enum) : ToDo, Ongoing, Finished
- `Project` : Projet parent
- `DueDate` (DateTime?) : Date d'échéance optionnelle
- `Commentaires` : Liste de commentaires (strings)

## Prérequis

- **.NET 9 SDK** : Téléchargeable sur [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/9.0)
- **IDE** : Visual Studio 2022, Rider, ou VS Code avec extension C#

## Installation

1. **Cloner le dépôt** (si applicable) ou ouvrir le projet dans votre IDE
2. **Restaurer les packages** :
   ```bash
   dotnet restore
   ```

## Configuration

### appsettings.json
Le fichier [`TaskFlowAPI/appsettings.json`](TaskFlowAPI/appsettings.json) contient :
- **ConnectionStrings** : Chaîne de connexion SQLite (`Data Source=TaskFlowDb.db`)
- **Jwt** : Configuration JWT (Issuer, Audience, Key, ExpiryMinutes)
- **Logging** : Niveaux de logs

## Lancement

### Via IDE
1. Ouvrez `ProjetTaskFlow.sln`
2. Définissez **TaskFlowAPI** comme projet de démarrage
3. Lancez avec F5 ou le bouton "Run"

### Via ligne de commande
```bash
cd TaskFlowAPI
dotnet run
```

L'API sera accessible sur `http://localhost:5072` (ou https://localhost:7208).

### Interface Swagger
Accédez à `http://localhost:5072/swagger` pour tester les endpoints.

## Utilisation

### Authentification
Tous les endpoints sauf l'inscription et la connexion nécessitent un token JWT dans l'en-tête `Authorization: Bearer <token>`.

#### Inscription
```http
POST /api/users
Content-Type: application/json

{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "password123",
  "role": "User"
}
```

#### Connexion
```http
POST /api/users/login
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "password123"
}
```

Réponse : Token JWT et infos utilisateur.

### Endpoints principaux

#### Utilisateurs
- `POST /api/users` : Inscription d'un nouvel utilisateur
- `POST /api/users/login` : Connexion d'un utilisateur
- `GET /api/users` : Liste des utilisateurs (authentifié)
- `GET /api/users/{id}` : Détails d'un utilisateur (authentifié)

#### Projets (authentifié)
- `GET /api/projects` : Liste des projets
- `GET /api/projects/{id}` : Détails d'un projet
- `POST /api/projects` : Créer un projet
- `PUT /api/projects/{id}` : Modifier un projet
- `DELETE /api/projects/{id}` : Supprimer un projet

#### Tâches (authentifié)
- `GET /api/tasks` : Liste des tâches
- `GET /api/tasks/{id}` : Détails d'une tâche
- `POST /api/tasks` : Créer une tâche
- `PUT /api/tasks/{id}` : Modifier une tâche
- `DELETE /api/tasks/{id}` : Supprimer une tâche

Consultez Swagger pour les schémas complets des requêtes/réponses.

## Base de données

- **Type** : SQLite (`TaskFlowDb.db`)
- **Initialisation** : Automatique au démarrage (migration via `EnsureCreated()`)
- **Emplacement** : Dans le dossier `TaskFlowAPI/`

## Tests

Le fichier [`TaskFlowAPI/TaskFlowAPI.http`](file) contient des exemples de requêtes pour tester tous les endpoints.

**Pour utiliser les tests** :
1. Lancez l'API : `dotnet run --project TaskFlowAPI`
2. Ouvrez `TaskFlowAPI.http` dans Rider
3. Exécutez toutes les requêtes