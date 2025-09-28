// notifications.js - Gestion des notifications temps réel
class NotificationManager {
    constructor() {
        this.connection = null;
        this.notificationCount = 0;
        this.notificationsList = [];
        this.isConnected = false;
        this.retryCount = 0;
        this.maxRetries = 5;
        
        this.init();
    }

    init() {
        this.setupSignalR();
        this.setupEventListeners();
        this.loadInitialNotifications();
    }

    setupSignalR() {
        try {
            // Connexion au hub SignalR
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/notifications")
                .withAutomaticReconnect([0, 2000, 10000, 30000])
                .build();

            // Gestion des événements de connexion
            this.connection.onreconnecting(() => {
                console.log("Tentative de reconnexion aux notifications...");
                this.isConnected = false;
            });

            this.connection.onreconnected(() => {
                console.log("Reconnecté aux notifications");
                this.isConnected = true;
                this.retryCount = 0;
            });

            this.connection.onclose(() => {
                console.log("Déconnecté des notifications");
                this.isConnected = false;
                this.scheduleReconnect();
            });

            // Écoute des notifications (compat: deux noms d'événements)
            const normalize = (payload) => ({
                id: payload.id || payload.Id,
                type: this.normalizeType(payload.type || payload.Type),
                message: payload.message || payload.Message,
                dateEnvoi: payload.dateEnvoi || payload.date || payload.DateEnvoi || new Date().toISOString(),
                lu: payload.lu ?? false
            });

            this.connection.on("notification", (payload) => {
                this.handleNewNotification(normalize(payload));
            });

            this.connection.on("ReceiveNotification", (payload) => {
                this.handleNewNotification(normalize(payload));
            });

            this.connection.on("UpdateNotificationCount", (count) => {
                this.updateNotificationCount(count);
            });

            this.connection.on("MarkAsRead", (notificationId) => {
                this.markNotificationAsRead(notificationId);
            });

            // Démarrer la connexion
            this.startConnection();

        } catch (error) {
            console.error("Erreur lors de l'initialisation de SignalR:", error);
        }
    }

    async startConnection() {
        try {
            await this.connection.start();
            this.isConnected = true;
            console.log("Connecté aux notifications temps réel");
        } catch (error) {
            console.error("Erreur de connexion aux notifications:", error);
            this.scheduleReconnect();
        }
    }

    scheduleReconnect() {
        if (this.retryCount < this.maxRetries) {
            this.retryCount++;
            const delay = Math.min(1000 * Math.pow(2, this.retryCount), 30000);
            console.log(`Tentative de reconnexion dans ${delay}ms (${this.retryCount}/${this.maxRetries})`);
            
            setTimeout(() => {
                this.startConnection();
            }, delay);
        } else {
            console.error("Nombre maximum de tentatives de reconnexion atteint");
        }
    }

    setupEventListeners() {
        // Gestion du clic sur le bouton notifications
        const notificationButton = document.querySelector('a.nav-link.dropdown-toggle i.bi-bell')?.closest('a') || document.querySelector('[data-bs-toggle="dropdown"]');
        if (notificationButton) {
            notificationButton.addEventListener('click', () => {
                this.loadNotifications();
            });
        }

        // Gestion de la fermeture du dropdown
        document.addEventListener('click', (e) => {
            if (!e.target.closest('.dropdown')) {
                this.closeNotificationsDropdown();
            }
        });
    }

    async loadInitialNotifications() {
        try {
            const response = await fetch('/Notifications/GetUnreadCount');
            if (response.ok) {
                const payload = await response.json();
                const count = payload.count ?? payload;
                this.updateNotificationCount(count);
            }
        } catch (error) {
            console.error("Erreur lors du chargement initial des notifications:", error);
        }
    }

    async loadNotifications() {
        try {
            const response = await fetch('/Notifications/GetRecent');
            if (response.ok) {
                const notifications = await response.json();
                const list = Array.isArray(notifications.items) ? notifications.items : notifications;
                this.displayNotifications(list);
            }
        } catch (error) {
            console.error("Erreur lors du chargement des notifications:", error);
        }
    }

    handleNewNotification(notification) {
        // Ajouter la notification à la liste
        this.notificationsList.unshift(notification);
        
        // Mettre à jour le compteur
        this.notificationCount++;
        this.updateNotificationCount(this.notificationCount);
        
        // Afficher une notification toast
        this.showToastNotification(notification);
        
        // Mettre à jour l'affichage si le dropdown est ouvert
        if (this.isNotificationsDropdownOpen()) {
            this.displayNotifications(this.notificationsList);
        }
    }

    displayNotifications(notifications) {
        const container = document.getElementById('notificationsList');
        if (!container) return;

        // Vider le contenu existant
        container.innerHTML = '';
        
        // Ajouter l'en-tête
        const header = document.createElement('li');
        header.innerHTML = '<h6 class="dropdown-header">Notifications</h6>';
        container.appendChild(header);
        
        // Ajouter le séparateur
        const separator = document.createElement('li');
        separator.innerHTML = '<hr class="dropdown-divider">';
        container.appendChild(separator);

        if (notifications && notifications.length > 0) {
            notifications.forEach(notification => {
                const item = this.createNotificationItem(notification);
                container.appendChild(item);
            });
        } else {
            const noNotifications = document.createElement('li');
            noNotifications.innerHTML = '<a class="dropdown-item text-center" href="#" id="noNotifications">Aucune notification</a>';
            container.appendChild(noNotifications);
        }

        // Ajouter le lien "Voir toutes"
        const viewAll = document.createElement('li');
        viewAll.innerHTML = '<hr class="dropdown-divider"><a class="dropdown-item text-center" href="/Notifications">Voir toutes les notifications</a>';
        container.appendChild(viewAll);
    }

    createNotificationItem(notification) {
        const item = document.createElement('li');
        
        const icon = this.getNotificationIcon(notification.type);
        const timeAgo = this.getTimeAgo(notification.dateEnvoi);
        
        item.innerHTML = `
            <a class="dropdown-item" href="#" onclick="notificationManager.markAsRead('${notification.id}')">
                <div class="d-flex align-items-start">
                    <div class="flex-shrink-0">
                        ${icon}
                    </div>
                    <div class="flex-grow-1 ms-2">
                        <p class="mb-1 small">${notification.message}</p>
                        <small class="text-muted">${timeAgo}</small>
                    </div>
                    ${!notification.lu ? '<span class="badge bg-primary rounded-pill ms-2">Nouveau</span>' : ''}
                </div>
            </a>
        `;
        
        return item;
    }

    getNotificationIcon(type) {
        const iconMap = {
            0: '<i class="bi bi-info-circle text-info"></i>',
            1: '<i class="bi bi-pencil text-warning"></i>',
            2: '<i class="bi bi-x-circle text-danger"></i>',
            3: '<i class="bi bi-exclamation-triangle text-warning"></i>'
        };
        
        return iconMap[type] || '<i class="bi bi-bell text-secondary"></i>';
    }

    normalizeType(type) {
        if (typeof type === 'number') return type;
        const map = { 'Info': 0, 'Modification': 1, 'Annulation': 2, 'Rupture': 3 };
        return map[type] ?? 0;
    }

    getTimeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);

        if (diffMins < 1) return "À l'instant";
        if (diffMins < 60) return `Il y a ${diffMins} min`;
        if (diffHours < 24) return `Il y a ${diffHours}h`;
        if (diffDays < 7) return `Il y a ${diffDays}j`;
        
        return date.toLocaleDateString('fr-FR');
    }

    updateNotificationCount(count) {
        this.notificationCount = count;
        const badge = document.getElementById('notificationCount');
        if (badge) {
            badge.textContent = count;
            badge.style.display = count > 0 ? 'inline' : 'none';
        }
    }

    async markAsRead(notificationId) {
        try {
            const response = await fetch('/Notifications/MarkAsRead', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ id: notificationId })
            });

            if (response.ok) {
                // Mettre à jour l'affichage local
                const notification = this.notificationsList.find(n => n.id === notificationId);
                if (notification) {
                    notification.lu = true;
                }
                
                // Mettre à jour le compteur
                if (this.notificationCount > 0) {
                    this.updateNotificationCount(this.notificationCount - 1);
                }
                
                // Mettre à jour l'affichage
                this.displayNotifications(this.notificationsList);
            }
        } catch (error) {
            console.error("Erreur lors du marquage comme lu:", error);
        }
    }

    showToastNotification(notification) {
        // Créer une notification toast Bootstrap
        const toastContainer = this.getOrCreateToastContainer();
        
        const toast = document.createElement('div');
        toast.className = 'toast align-items-center text-white bg-primary border-0';
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');
        
        const icon = this.getNotificationIcon(notification.type);
        
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${icon} ${notification.message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;
        
        toastContainer.appendChild(toast);
        
        // Afficher le toast
        const bsToast = new bootstrap.Toast(toast);
        bsToast.show();
        
        // Supprimer le toast après affichage
        toast.addEventListener('hidden.bs.toast', () => {
            toast.remove();
        });
    }

    getOrCreateToastContainer() {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }
        return container;
    }

    isNotificationsDropdownOpen() {
        const dropdown = document.querySelector('.dropdown-menu.show');
        return dropdown && dropdown.id === 'notificationsList';
    }

    closeNotificationsDropdown() {
        const dropdown = document.querySelector('.dropdown-menu.show');
        if (dropdown) {
            const bsDropdown = bootstrap.Dropdown.getInstance(dropdown);
            if (bsDropdown) {
                bsDropdown.hide();
            }
        }
    }

    // Méthode pour envoyer une notification de test (développement)
    sendTestNotification() {
        if (this.connection && this.isConnected) {
            this.connection.invoke("SendTestNotification", "Test de notification");
        }
    }

    // Méthode pour nettoyer les ressources
    dispose() {
        if (this.connection) {
            this.connection.stop();
        }
    }
}

// Initialisation du gestionnaire de notifications
let notificationManager;

document.addEventListener('DOMContentLoaded', function() {
    // Vérifier si SignalR est disponible
    if (typeof signalR !== 'undefined') {
        notificationManager = new NotificationManager();
    } else {
        console.warn("SignalR n'est pas disponible, les notifications temps réel sont désactivées");
    }
});

// Gestion de la fermeture de la page
window.addEventListener('beforeunload', function() {
    if (notificationManager) {
        notificationManager.dispose();
    }
});
