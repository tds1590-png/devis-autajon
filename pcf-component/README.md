# 🎯 PCF Component - Devis AUTAJON HP6000

**Power App Component Framework** pour intégrer le calculateur Devis dans Power Apps.

## 📁 Structure

```
pcf-component/
├── ControlManifest.Input.xml    ← Configuration du composant
├── index.ts                     ← Logique TypeScript
├── package.json                 ← Dépendances npm
├── tsconfig.json               ← Configuration TypeScript
├── css/
│   └── DevisCalculator.css
├── strings/
│   └── DevisCalculator.1033.resx
└── GUIDE_PCF_DEPLOIEMENT.md    ← Guide d'installation
```

## ⚡ Quick Start

```powershell
# 1. Installer les outils
npm install -g @microsoft/power-platform-cli

# 2. Installer les dépendances
npm install

# 3. Builder
npm run build

# 4. Packer
npm run pack

# 5. Déployer dans Power Apps
# (Voir GUIDE_PCF_DEPLOIEMENT.md)
```

## 🔑 Fonctionnalités

✅ Charge le formulaire Devis depuis GitHub Pages
✅ Communication bidirectionnelle via postMessage
✅ Sauvegarde automatique dans Dataverse
✅ Intégration Web API
✅ Support du responsive design

## 📝 Configuration

### Manifest (ControlManifest.Input.xml)

Définit les propriétés et inputs du composant:

```xml
<control namespace="Autajon" 
         constructor="DevisCalculator" 
         version="1.0.0" 
         display-name-key="Devis HP6000 Calculator">
```

### TypeScript (index.ts)

Logique principale:
- Créer l'iframe
- Charger le formulaire
- Écouter les messages du formulaire
- Sauvegarder dans Dataverse

### Styles (css/DevisCalculator.css)

Styling du conteneur et de l'iframe.

## 🌐 Intégration Power Apps

**Dans Canvas App:**

```javascript
// Insert → Get more components → Devis HP6000 Calculator
// Le composant s'affiche immédiatement
```

**Dans Model-Driven App:**

```javascript
// Customizations → Forms → Add Custom Control
// Sélectionner: Autajon.DevisCalculator
// Publish
```

## 🔌 Communication

### Formulaire → PCF

```javascript
// Quand devis calculé
window.parent.postMessage({
  type: "DEVIS_CALCULATED",
  payload: { /* données */ }
}, "https://tds1590-png.github.io");
```

### PCF → Dataverse (Web API)

```typescript
fetch(`${clientUrl}/api/data/v9.2/hp_devis`, {
  method: "POST",
  body: JSON.stringify(devisEntity)
});
```

## 🛠️ Commandes

```powershell
npm run build      # Build le composant
npm run pack       # Crée la solution (.zip)
npm run lint       # Vérifier la syntaxe
npm run clean      # Nettoyer les fichiers générés
```

## 📦 Build Output

Après `npm run build`:

```
out/controls/Autajon.DevisCalculator/
├── bundle.js          ← Code compilé
├── bundle.css         ← Styles
└── ControlManifest.xml
```

Après `npm run pack`:

```
out/autajon-devis-pcf.1.0.0.zip  ← Prêt pour Power Apps
```

## 🚀 Déploiement

1. **Build & Pack**
   ```powershell
   npm run build && npm run pack
   ```

2. **Créer une Solution Dataverse**
   - Power Apps Admin Center → Solutions → New

3. **Importer le PCF**
   ```powershell
   pac solution add --solution-file out/autajon-devis-pcf.1.0.0.zip
   ```

4. **Publier**
   ```powershell
   pac solution publish
   ```

5. **Utiliser dans Power Apps**
   - Insert → Get more components → Devis AUTAJON

## 📖 Documentation complète

Voir: [GUIDE_PCF_DEPLOIEMENT.md](GUIDE_PCF_DEPLOIEMENT.md)

---

**Status**: 🟢 Prêt pour développement/test

**Version**: 1.0.0
**Namespace**: Autajon
**Component**: DevisCalculator
