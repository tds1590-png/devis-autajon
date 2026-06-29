# ⚡ Commandes Rapides - Devis AUTAJON

*Les 10 commandes essentielles pour lancer/tester/déployer*

---

## 🏃 Démarrage rapide (3 commandes)

### 1. Vérifier le formulaire localement
```bash
# Ouvre simplement le fichier
double-clic: devis-final.html

# Ou via terminal
cd "c:\Users\petits\Devis VITI"
start devis-final.html
```
**Résultat**: Formulaire s'ouvre avec 46 commerciaux, 25 frontaux, 121 couleurs

---

### 2. Accéder via GitHub Pages
```
https://tds1590-png.github.io/devis-autajon/devis-final.html
```
**Résultat**: Formulaire accessible partout (pas besoin d'installation)

---

### 3. Générer/mettre à jour les données CSV → JSON
```powershell
cd "c:\Users\petits\Devis VITI"
.\extract-vraies-donnees.ps1
```
**Résultat**: Génère dataverse-data-complet.json avec les 46 commerciaux

---

## 🔨 Build & Compilation (PCF)

### 4. Construire le PCF Component
```bash
cd "c:\Users\petits\Devis VITI\pcf-component"
npm install --legacy-peer-deps   # Une seule fois
npm run build                     # À chaque modification
```
**Résultat**: Génère bundle.js (10.9 KB) dans out/

---

### 5. Vérifier le build
```bash
ls -la "c:\Users\petits\Devis VITI\pcf-component\out\controls\bundle.js"
```
**Résultat**: Voir la taille du bundle compilé

---

## 🚀 Power Platform Deployment

### 6. Installer Power Platform CLI
```powershell
npm install -g @microsoft/power-platform-cli
pac --version  # Vérifier
```
**Résultat**: Power Platform CLI prêt

---

### 7. S'authentifier à Dataverse
```powershell
pac auth create --url https://org2f4b1c01.crm4.dynamics.com
```
**Résultat**: Authentification via navigateur, profil créé

---

### 8. Créer une solution
```powershell
pac solution create -sn devis_autajon_pcf -d "PCF Calculator"
```
**Résultat**: Solution "Devis AUTAJON PCF" créée dans Dataverse

---

## 📤 Git & GitHub

### 9. Pusher les modifications vers GitHub
```bash
cd "c:\Users\petits\Devis VITI"
git status                  # Voir les changements
git add .                   # Ajouter tous les fichiers
git commit -m "Message"     # Créer un commit
git push                    # Pousser vers GitHub
```
**Résultat**: Changements visibles sur https://github.com/tds1590-png/devis-autajon

---

### 10. Cloner le repo (partager avec l'équipe)
```bash
git clone https://github.com/tds1590-png/devis-autajon.git
cd devis-autajon
# Ouvre le projet
```
**Résultat**: Copie complète du projet sur ton ordi

---

## 📊 Diagnostic & Dépannage

### Vérifier si toutes les données sont chargées
```powershell
# Ouvre la console du navigateur (F12)
# Tape dans la console:
console.table(DATA)  # Affiche toutes les 8 arrays
```

### Vérifier les erreurs de calcul
```javascript
// Dans la console du navigateur:
calculerDevis({formatLargeur: 100, formatHauteur: 150, quantite: 2000})
// Résultat: {prixDevis: 3467.28, m2Total: 82.5, ...}
```

### Vérifier la persistance localStorage
```javascript
// Console du navigateur:
console.log(localStorage.getItem("devis-form"))
// Résultat: JSON avec les données du formulaire
```

### Vérifier GitHub Pages
```bash
curl https://raw.githubusercontent.com/tds1590-png/devis-autajon/main/dataverse-data-complet.json | head -20
```
**Résultat**: Voir les premières 20 lignes du JSON

---

## 🔄 Workflow complet (du CSV au Power Apps)

```
1. Modifier seed-data/*.csv
   ↓ (si besoin)
2. .\extract-vraies-donnees.ps1
   ↓
3. git add . && git push
   ↓ (le JSON se met à jour automatiquement)
4. npm run build  (si changement PCF)
   ↓
5. Tester: https://...github.io/devis-autajon/devis-final.html
   ↓
6. Importer PCF dans Power Apps
   ↓
7. Ajouter à Canvas App et tester
   ↓
8. Sauvegarder dans Dataverse (Web API)
```

---

## 📁 Structure des fichiers (à connaître)

```
📦 Devis VITI/
├─ 📄 devis-final.html              ← Formulaire (EDIT THIS)
├─ 📄 calcul-devis.js               ← Moteur calcul (EDIT THIS)
├─ 📄 dataverse-data-complet.json    ← Données (AUTO-GENERATED)
├─ 📄 extract-vraies-donnees.ps1     ← Extraction CSV (RUN THIS)
├─ 📁 pcf-component/
│  ├─ 📄 index.ts                   ← PCF logic (EDIT THIS)
│  ├─ 📄 package.json
│  ├─ 📄 tsconfig.json
│  ├─ 📁 out/
│  │  └─ 📄 bundle.js               ← Compiled (AUTO-GENERATED)
│  └─ 📁 css/
├─ 📁 seed-data/
│  ├─ 📄 Commercial.csv
│  ├─ 📄 Support.csv
│  ├─ 📄 ProfilCouleurHP.csv
│  └─ ... (autres CSVs)
└─ 📄 PROJECT_SUMMARY.md            ← Vue d'ensemble
```

---

## 🎯 Fichiers à éditer selon besoin

### Pour modifier le formulaire
→ Edit: **devis-final.html**

### Pour modifier le calcul
→ Edit: **calcul-devis.js**

### Pour modifier les données
→ Edit: **seed-data/\*.csv** + Run: **extract-vraies-donnees.ps1**

### Pour modifier le PCF
→ Edit: **pcf-component/index.ts** + Run: **npm run build**

---

## 💡 Tips & Tricks

### Ajouter un commercial
1. Edit: seed-data/Commercial.csv
2. Add: "Nouveau Commercial,contact@email.com,0123456789"
3. Run: .\extract-vraies-donnees.ps1
4. Reload: devis-final.html

### Ajouter une couleur
1. Edit: seed-data/ProfilCouleurHP.csv
2. Add: "NOUVELLE_COULEUR,#CCCCCC,1.50"
3. Run: .\extract-vraies-donnees.ps1
4. Reload: devis-final.html

### Modifier la formule de prix
1. Edit: calcul-devis.js
2. Search: "AEAT = cSupport"
3. Modify: calcul logique
4. Test: Reload form, test avec W=100, H=150, Q=2000

### Déboguer le PCF
1. Edit: pcf-component/index.ts
2. Add: console.log() statements
3. Run: npm run build
4. Reload Power Apps canvas
5. Check: F12 Console

---

## ✅ Checklist d'une livraison

- [ ] `npm run build` lance sans erreur
- [ ] dataverse-data-complet.json a 46 commerciaux
- [ ] devis-final.html s'ouvre et charge les données
- [ ] Test: W=100, H=150, Q=2000 = €3467.28
- [ ] localStorage persiste les données
- [ ] `git push` réussit
- [ ] https://github.com/tds1590-png/devis-autajon voit les changements
- [ ] PCF s'ajoute à Canvas App sans erreur

---

## 🎓 Variables d'environnement utiles

```powershell
# Si npm ne trouve pas les packages
$env:npm_config_legacy_peer_deps="true"

# Si tu veux voir les logs détaillés
$DebugPreference = "Continue"

# Pour Power Platform CLI
pac auth list  # Voir les profils existants
pac auth delete --name [profile-name]  # Supprimer un profil
```

---

## 📞 Rapide ref d'URLs

| Ressource | URL |
|-----------|-----|
| **Repo GitHub** | https://github.com/tds1590-png/devis-autajon |
| **Formulaire Live** | https://tds1590-png.github.io/devis-autajon/devis-final.html |
| **Données JSON** | https://raw.githubusercontent.com/tds1590-png/devis-autajon/main/dataverse-data-complet.json |
| **Power Apps Studio** | https://make.powerapps.com |
| **Power Apps Admin** | https://admin.powerplatform.microsoft.com |
| **Dataverse URL** | https://org2f4b1c01.crm4.dynamics.com |

---

*Créé: 29 Juin 2026*
*Quick reference pour le projet Devis AUTAJON*
*v1.0.0 - Production Ready*
