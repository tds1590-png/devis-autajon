# 📋 Prochaines étapes - De Power Apps à Production

**Status**: ✅ PCF Component buildé et prêt
**Date**: 29 Juin 2026

---

## 🎯 Objectif maintenant

Déployer le PCF Component dans **Power Apps** et tester l'intégration complète.

---

## 📍 Point de départ

✅ Tu as:
- Formulaire HTML/JS complet (devis-final.html)
- Moteur calcul JavaScript (calcul-devis.js)
- Données réelles JSON (dataverse-data-complet.json)
- **PCF Component buildé** (TypeScript → bundle.js)
- Repo GitHub public

❌ Tu n'as pas encore:
- Installé Power Platform CLI
- Authentifié dans ton Dataverse
- Créé une solution dans Power Apps
- Testé le PCF dans une Canvas App

---

## 🚀 Étape 1: Installer Power Platform CLI

### Windows PowerShell (Admin)
```powershell
npm install -g @microsoft/power-platform-cli
```

### Vérifier l'installation
```powershell
pac --version
```

**Résultat attendu**: `PowerApps CLI 1.xx.x` ou plus récent

---

## 🔐 Étape 2: S'authentifier dans Dataverse

### Créer un profil d'authentification
```powershell
pac auth create --url https://org2f4b1c01.crm4.dynamics.com
```

**Note**: Remplace l'URL par ton URL Dataverse réelle
- Format: `https://[org-code].crm4.dynamics.com`
- Trouver l'URL: Power Apps Admin Center → Environments → Copy URL

### Résultat
- Navigateur s'ouvre → Authentification Microsoft
- PowerShell affiche: "Profile created successfully"

### Lister les profils
```powershell
pac auth list
```

---

## 📦 Étape 3: Créer une Solution dans Dataverse

### Méthode 1: Via Power Apps Admin Center (GUI)
1. Go: https://admin.powerplatform.microsoft.com/
2. Environments → Select your environment
3. New solution:
   - **Name**: Devis AUTAJON PCF
   - **Unique Name**: devis_autajon_pcf
   - **Version**: 1.0.0
4. Click Create

### Méthode 2: Via PowerShell (CLI)
```powershell
pac solution create -sn devis_autajon_pcf -d "PCF Calculator for Devis"
```

**Résultat**: Solution créée et affichée dans Power Apps

---

## 🔧 Étape 4: Importer le PCF dans la Solution

### Approche manuelle (GUI - Recommandé)

1. Go: https://make.powerapps.com
2. **Solutions** → Select "Devis AUTAJON PCF"
3. **New** → **More** → **App** → **Canvas app**
4. Create → Open in Power Apps Studio
5. **Insert** → **Get more components**
6. **Search**: "Devis"
7. **Add**: "Devis HP6000 Calculator"

### Approche automatisée (CLI)

#### a) D'abord, créer un ZIP du PCF
```powershell
cd "c:\Users\petits\Devis VITI\pcf-component"
Compress-Archive -Path out/controls -DestinationPath devis-pcf-1.0.0.zip
```

#### b) Puis importer dans la solution
```powershell
pac solution add --path "c:\Users\petits\Devis VITI\pcf-component\devis-pcf-1.0.0.zip" --solutionName devis_autajon_pcf
```

**Résultat**: PCF Component ajouté à la solution

---

## 🎨 Étape 5: Créer une Canvas App et tester

### 1. Ouvrir Power Apps Studio
Go: https://make.powerapps.com → **Create** → **Canvas app**

### 2. Ajouter le PCF Component
- **Insert** (left panel)
- **Get more components**
- Search: "Devis HP6000 Calculator"
- **Add** to app

### 3. Positionner et redimensionner
```
Width: 100%
Height: 100%
```

### 4. Tester l'intégration
- Ouvre la Canvas App
- Vérifie que le formulaire se charge
- Tente un calcul (ex: W=100, H=150, Q=2000)
- Attends le résultat: €3467.28

### 5. Vérifier la sauvegarde Dataverse
- Ouvre **Power Apps Admin** → **Tables** → Search "hp_devis"
- Vérifie que les données sont sauvegardées

---

## 🔗 Étape 6: Configurer Web API Dataverse (Optionnel)

### Vérifier les permissions du PCF
Dans le `index.ts`:
```typescript
private _saveDevisToDataverse(devisData: any): Promise<void> {
  // Web API call to Dataverse
  const webApiUrl = `${this._context.webAPI.getClientUrl()}/api/data/v9.2/hp_devis`;
  // POST request with devisData
}
```

### Résultat attendu
- PCF envoie les données via Web API
- Dataverse enregistre dans la table "hp_devis"
- Confirm: Admin Center → Tables → hp_devis → Data

---

## ✅ Étape 7: Validation complète

### Checklist de test

- [ ] PCF Component se charge dans Canvas App
- [ ] Formulaire HTML s'affiche correctement
- [ ] Toutes les dropdowns sont remplies (46 commerciaux)
- [ ] Calcul fonctionne: W=100, H=150, Q=2000 → €3467.28
- [ ] Résultat correct: M²=82.50, ML=250
- [ ] localStorage persiste les données
- [ ] Bouton "Générer lettre" marche
- [ ] Bouton "Sauvegarder" persiste dans localStorage
- [ ] Web API save écrit dans Dataverse
- [ ] Table hp_devis contient les données

---

## 🐛 Dépannage

### PCF ne s'affiche pas
**Cause**: URL GitHub Pages incorrect
**Fix**: Vérifier dans `index.ts`:
```typescript
private _githubUrl: string = "https://tds1590-png.github.io/devis-autajon/devis-final.html";
```

### Formulaire se charge mais calculs ne marchent pas
**Cause**: dataverse-data-complet.json non accessible
**Fix**: Vérifier GitHub Pages URL:
```
https://raw.githubusercontent.com/tds1590-png/devis-autajon/main/dataverse-data-complet.json
```

### Web API save échoue
**Cause**: Permissions manquantes ou URL Dataverse incorrecte
**Fix**: Vérifier dans `index.ts`:
```typescript
const webApiUrl = `${this._context.webAPI.getClientUrl()}/api/data/v9.2/hp_devis`;
```

### Dropdowns vides
**Cause**: JSON ne charge pas
**Fix**: 
- Ouvre Console (F12)
- Regarde "Erreurs de réseau"
- Vérifie GitHub Pages status

---

## 🎯 Prochaines optimisations (Phase 2+)

### Court terme
- [ ] Ajouter Power Automate Flow (email envoi devis)
- [ ] Dashboard Power BI (analytics devis)
- [ ] Multi-language support

### Moyen terme
- [ ] Authentification Entra ID
- [ ] Historique devis (table Dataverse)
- [ ] Approbation workflow

### Long terme
- [ ] Mobile app
- [ ] API REST
- [ ] Advanced pricing engine
- [ ] Machine learning

---

## 📞 Support & Documentation

### Si tu as besoin d'aide

| Sujet | Document |
|-------|----------|
| Utiliser le formulaire | GUIDE_UTILISATION_FORMULAIRE_V4.md |
| Comprendre le calcul | MOTEUR_CALCUL_JS.md |
| Architecture système | pcf-component/ARCHITECTURE.md |
| Déploiement | DEPLOYMENT_GUIDE_V1.md |
| Commandes | pcf-component/COMMANDES.md |
| Résumé projet | PROJECT_SUMMARY.md |

### Links utiles
- Power Apps Admin: https://admin.powerplatform.microsoft.com/
- Make Power Apps: https://make.powerapps.com
- GitHub Repo: https://github.com/tds1590-png/devis-autajon
- Formulaire: https://tds1590-png.github.io/devis-autajon/devis-final.html

---

## 🎉 Conclusion

**Tu es à 95% du chemin!**

Reste juste à:
1. ✅ Installer Power Platform CLI (5 min)
2. ✅ S'authentifier Dataverse (2 min)
3. ✅ Créer solution + Canvas App (10 min)
4. ✅ Ajouter PCF et tester (5 min)

**Total: ~30 minutes pour la production! 🚀**

---

*Document créé: 29 Juin 2026*
*PCF Component v1.0.0*
*Devis AUTAJON - Ready for Power Apps*
