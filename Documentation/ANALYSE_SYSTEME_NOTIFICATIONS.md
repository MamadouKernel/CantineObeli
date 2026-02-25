# Analyse de la Fonctionnalité : Système de Notifications en Temps Réel

## 📋 Cahier des Charges

### Fonctionnalités Attendues

**"Notification"**

Mise en place d'un système de notification en temps réel permettant d'informer l'équipe du prestataire cantine dans les cas suivants :

1. **Modification de commande** : Toute modification de commande effectuée par un utilisateur dans le délai imparti.

2. **Annulation de commande** : Toute annulation de commande, qu'elle soit initiée par l'utilisateur dans le délai ou par l'équipe prestataire au moment du service.

3. **Réactivité immédiate** : Ce système devra permettre une réactivité immédiate pour une meilleure gestion des flux de commande et des éventuelles ruptures de stock.

## ✅ État d'Implémentation

### 1. Infrastructure SignalR ✅ IMPLÉMENTÉ

#### Configuration - `Program.cs`

**Ligne 56** :
```csharp
// 4) SignalR
builder.Services.AddSignalR();
```

**Ligne 66** :
```csharp
app.MapHub<NotificationsHub>("/hubs/notifications");
```

**✅ CONFORME** : SignalR est configuré et le hub est mappé.

---

### 2. Hub de Notifications - `Hubs/NotificationsHub.cs`

#### Fonctionnalités du Hub

**Méthodes disponibles** :
```csharp
public class NotificationsHub : Hub
{
    // Rejoindre un groupe
    public async Task JoinGroup(string groupName)
    
    // Quitter un groupe
    public async Task LeaveGroup(string groupName)
    
    // Envoyer à un utilisateur spécifique
    public async Task SendNotification(string userId, string message, TypeNotification type)
    
    // Envoyer à un groupe
    public async Task SendNotificationToGroup(string groupName, string message, TypeNotification type)
    
    // Envoyer à un rôle
    public async Task SendNotificationToRole(RoleType role, string message, TypeNotification type)
    
    // Envoyer à tous
    public async Task SendNotificationToAll(string message, TypeNotification type)
}
```

**Gestion automatique des groupes** :
```csharp
public override async Task OnConnectedAsync()
{
    // Ajouter l'utilisateur au groupe de son rôle
    var user = Context.User;
    if (user?.Identity?.IsAuthenticated == true)
    {
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);
        
        foreach (var role in roles)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }
        
        // Ajouter spécifiquement au groupe des prestataires si applicable
        if (roles.Contains("PrestataireCantine"))
        {
  