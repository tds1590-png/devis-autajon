# 🛠️ COMMANDES PCF - CHEAT SHEET

## 📦 Installation

```powershell
# Installer Power Platform CLI globalement
npm install -g @microsoft/power-platform-cli

# Vérifier
pac --version

# Installer les dépendances du projet
cd pcf-component
npm install
```

---

## 🔨 Build

```powershell
# Build simple
npm run build

# Build avec plus de logs
npm run build -- --verbose

# Build et watch (recompile à chaque changement)
npm run build -- --watch

# Clean avant rebuild
npm run clean && npm run build
```

---

## 📦 Packaging

```powershell
# Créer la solution ZIP
npm run pack

# Résultat: out/autajon-devis-pcf.1.0.0.zip
```

---

## 🌐 Authentication Dataverse

```powershell
# Se connecter à Dataverse
pac auth create --url https://org2f4b1c01.crm4.dynamics.com

# Lister les connexions
pac auth list

# Sélectionner une connexion
pac auth select --index 0

# Se déconnecter
pac auth delete --index 0
```

---

## 📝 Solution Management

```powershell
# Créer une nouvelle solution
pac solution create --publisher-name "Autajon" --publisher-prefix "devis"

# Ajouter le PCF à une solution
pac solution add --solution-file "out/autajon-devis-pcf.1.0.0.zip" \
  --solution-name "devis_autajon_pcf"

# Lister les solutions
pac solution list

# Publier une solution
pac solution publish --solution-name "devis_autajon_pcf"

# Supprimer une solution
pac solution delete --solution-name "devis_autajon_pcf"
```

---

## 🧹 Lint & Format

```powershell
# Vérifier la syntaxe
npm run lint

# Lint avec fix automatique
npm run lint -- --fix
```

---

## 🧪 Development Server

```powershell
# Démarrer le serveur de dev local
npm start

# Accéder via: https://localhost:3000
```

---

## 🔍 Debugging

```powershell
# Dans Visual Studio Code:
# 1. Ouvrir le dossier pcf-component
# 2. Press F5 ou Debug → Start Debugging
# 3. Choisir "Edge" ou "Chrome"

# Ou manuellement:
# 1. npm start
# 2. Ouvrir DevTools (F12) dans le navigateur
# 3. Voir les logs et erreurs
```

---

## 📊 Monitoring

```powershell
# Voir les logs de build
npm run build 2>&1 | Tee-Object -FilePath build.log

# Voir les fichiers générés
ls out/controls/Autajon.DevisCalculator/

# Vérifier la taille du bundle
ls -lh out/controls/Autajon.DevisCalculator/bundle.js
```

---

## 🚀 Deployment Checklist

```powershell
# 1. Vérifier que tout compile
npm run build
# Vérifier: ✓ Build successful

# 2. Vérifier la qualité du code
npm run lint
# Vérifier: ✓ No errors

# 3. Créer la solution
npm run pack
# Vérifier: out/autajon-devis-pcf.1.0.0.zip existe

# 4. Vérifier l'authentification
pac auth list
# Vérifier: Au moins une connexion disponible

# 5. Publier
pac solution publish --solution-name "devis_autajon_pcf"
# Vérifier: ✓ Published successfully
```

---

## 🆘 Troubleshooting Commands

```powershell
# Nettoyer complètement
npm run clean
rm -r node_modules
npm install

# Vérifier les versions
npm --version
node --version
pac --version

# Vérifier TypeScript
npx tsc --version

# Vérifier la connectivité Dataverse
pac org list

# Voir les erreurs détaillées
npm run build -- --verbose 2>&1
```

---

## 📝 Configuration Files Quick Reference

### tsconfig.json
```json
{
  "compilerOptions": {
    "target": "ES5",          // Compatible Power Apps
    "module": "ES6",          // Modern modules
    "lib": ["ES2015", "DOM"]  // APIs disponibles
  }
}
```

### package.json
```json
{
  "scripts": {
    "build": "pcf-scripts build",
    "pack": "pcf-scripts pack --outputDirectory out",
    "clean": "pcf-scripts clean",
    "lint": "eslint ."
  }
}
```

### ControlManifest.Input.xml
```xml
<control namespace="Autajon" 
         constructor="DevisCalculator" 
         version="1.0.0">
```

---

## 🌍 Environment Variables

```powershell
# Si besoin de variables env:
$env:NODE_ENV = "production"

# Ou dans .env file:
# NODE_ENV=production
# API_URL=https://org...
```

---

## 🔄 Common Workflows

### Workflow 1: Développement rapide
```powershell
npm run clean
npm install
npm run build
npm run lint
npm run pack
```

### Workflow 2: Test local
```powershell
npm start
# Ouvrir https://localhost:3000 dans navigateur
# F12 pour debug
```

### Workflow 3: Publish to Dataverse
```powershell
npm run build
npm run pack
pac auth select --index 0
pac solution add --solution-file out/autajon-devis-pcf.1.0.0.zip
pac solution publish --solution-name devis_autajon_pcf
```

---

## 📊 Build Size Optimization

```powershell
# Voir la taille du bundle
ls -lh out/controls/Autajon.DevisCalculator/bundle.*

# Vérifier l'usage des dépendances
npm ls

# Analyser les dépendances
npm install -g webpack-bundle-analyzer
# Puis configurer dans webpack.config.js
```

---

## 🔗 Resources & Docs

- **Microsoft PCF**: https://docs.microsoft.com/en-us/powerapps/developer/component-framework/
- **npm Scripts**: https://docs.npmjs.com/misc/scripts
- **TypeScript**: https://www.typescriptlang.org/docs/
- **Power Platform CLI**: https://docs.microsoft.com/en-us/power-platform/developer/cli/reference/

---

**Last Updated**: 2026-06-29
**PCF Version**: 1.0.0
**Status**: Ready for production ✅
