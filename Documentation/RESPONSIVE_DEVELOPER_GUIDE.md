# 📱 Guide Développeur - Responsivité Obeli_K

## 🎯 Vue d'Ensemble

L'application Obeli_K utilise une approche **mobile-first** avec un système de **dual views** pour garantir une expérience optimale sur tous les appareils.

## 🏗️ Architecture Responsive

### **Breakpoints Standards**
```css
/* Mobile First */
@media (max-width: 576px)     /* Mobile */
@media (min-width: 577px) and (max-width: 768px)  /* Tablette */
@media (min-width: 769px)     /* Desktop */
@media (min-width: 1200px)    /* Large Desktop */
```

### **Système Dual View**
Toutes les listes utilisent le pattern suivant :
```html
<!-- Vue Desktop - Tableau -->
<div class="d-none d-lg-block">
    <div class="table-responsive">
        <table class="table table-hover">
            <!-- Contenu tableau -->
        </table>
    </div>
</div>

<!-- Vue Mobile - Cartes -->
<div class="d-lg-none">
    <div class="row g-3">
        @foreach (var item in items)
        {
            <div class="col-12">
                <div class="card border-0 shadow-sm">
                    <!-- Contenu carte avec dropdown actions -->
                </div>
            </div>
        }
    </div>
</div>
```

## 🎨 Classes Utilitaires

### **Affichage Conditionnel**
```css
.show-mobile     /* Visible uniquement sur mobile */
.show-tablet     /* Visible uniquement sur tablette */
.show-desktop    /* Visible uniquement sur desktop */
.hide-mobile     /* Masqué sur mobile */
.hide-tablet     /* Masqué sur tablette */
.hide-desktop    /* Masqué sur desktop */
```

### **Espacement Responsive**
```css
.gap-responsive  /* Gap adaptatif selon l'écran */
.p-responsive    /* Padding adaptatif */
.m-responsive    /* Margin adaptatif */
```

### **Utilitaires Spéciaux**
```css
.text-truncate-mobile  /* Tronque le texte sur mobile */
.stack-mobile          /* Empile les éléments sur mobile */
.btn-mobile-full       /* Bouton pleine largeur sur mobile */
```

## 🔧 Composants Responsive

### **Headers de Page**
```html
<div class="d-flex flex-column flex-sm-row justify-content-between align-items-start align-items-sm-center gap-3">
    <div>
        <h2 class="text-primary mb-1">
            <i class="fas fa-icon me-2"></i>
            <span class="d-none d-sm-inline">Titre Complet</span>
            <span class="d-sm-none">Titre Court</span>
        </h2>
        <p class="text-muted mb-0">Description</p>
    </div>
    <a href="#" class="btn btn-primary w-100 w-sm-auto">
        <i class="fas fa-plus me-2"></i>Action
    </a>
</div>
```

### **Actions Dropdown Mobile**
```html
<div class="dropdown">
    <button class="btn btn-outline-secondary btn-sm" type="button" data-bs-toggle="dropdown">
        <i class="fas fa-ellipsis-v"></i>
    </button>
    <ul class="dropdown-menu dropdown-menu-end">
        <li><a class="dropdown-item" href="#"><i class="fas fa-eye me-2"></i>Détails</a></li>
        <li><a class="dropdown-item" href="#"><i class="fas fa-edit me-2"></i>Modifier</a></li>
        <li><hr class="dropdown-divider"></li>
        <li><button class="dropdown-item text-danger"><i class="fas fa-trash me-2"></i>Supprimer</button></li>
    </ul>
</div>
```

### **Formulaires Responsive**
```html
<div class="row g-3">
    <div class="col-12 col-md-6">
        <label for="field1" class="form-label">Champ 1</label>
        <input type="text" class="form-control" id="field1">
    </div>
    <div class="col-12 col-md-6">
        <label for="field2" class="form-label">Champ 2</label>
        <select class="form-select" id="field2">
            <option>Option</option>
        </select>
    </div>
</div>
```

## 📋 Checklist Nouvelle Page

### **1. Header Responsive**
- [ ] Titre adaptatif (complet/court)
- [ ] Actions empilées sur mobile
- [ ] Boutons pleine largeur sur mobile

### **2. Contenu Principal**
- [ ] Système dual view (tableau/cartes)
- [ ] Actions dropdown sur mobile
- [ ] Badges et statuts visibles

### **3. Formulaires**
- [ ] Champs empilés sur mobile
- [ ] Labels visibles
- [ ] Boutons tactiles (min-height: 44px)

### **4. Navigation**
- [ ] Breadcrumbs responsive
- [ ] Pagination centrée sur mobile
- [ ] Liens tactiles

### **5. Tests**
- [ ] 320px (iPhone SE)
- [ ] 375px (iPhone standard)
- [ ] 768px (iPad)
- [ ] 1200px (Desktop)

## 🎯 Bonnes Pratiques

### **Mobile First**
```css
/* Base (Mobile) */
.component {
    padding: 1rem;
    font-size: 1rem;
}

/* Tablette */
@media (min-width: 577px) {
    .component {
        padding: 1.5rem;
    }
}

/* Desktop */
@media (min-width: 769px) {
    .component {
        padding: 2rem;
        font-size: 1.1rem;
    }
}
```

### **Accessibilité**
- Cibles tactiles ≥ 44px
- Contraste suffisant
- Focus visible
- Navigation clavier

### **Performance**
- Images responsive
- CSS optimisé
- Chargement conditionnel

## 🚀 Déploiement

### **Fichiers Modifiés**
- `wwwroot/css/responsive.css` - Framework CSS
- `Views/Shared/_Layout.cshtml` - Navigation
- `Views/*/List.cshtml` - Listes avec dual view
- `Views/*/Create.cshtml` - Formulaires

### **Tests Requis**
1. Navigation mobile
2. Tableaux → cartes
3. Formulaires tactiles
4. Actions dropdown
5. Modals responsive

## 📞 Support

Pour toute question sur l'implémentation responsive :
1. Consulter ce guide
2. Vérifier `responsive.css`
3. Tester sur appareils réels
4. Valider l'accessibilité

---
*Guide mis à jour le : $(Get-Date -Format "dd/MM/yyyy")*
*Version : 1.0 - 100% Responsive*