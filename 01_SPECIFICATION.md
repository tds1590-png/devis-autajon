# Calculateur de devis — Étiquettes numériques HP6000
## Spécification de portage Power Apps + Dataverse

> Reproduit un classeur Excel de chiffrage (impression numérique d'étiquettes adhésives,
> presse HP Indigo 6000, multi-sites groupe Autajon : AEME AEEP AEAT AEBO AELO AELI AESU).

---

## 1. Objectif fonctionnel
1. Calculer un coût de revient complet (matière, machine, finitions, transport).
2. Prix de vente = coût ÷ 0,80 (marge 20 % intégrée).
3. Coefficient de change par site : EUR = 1 ; AESU (CHF) = 1,06607.
4. Marge commerciale manuelle supplémentaire → prix client.
5. Chiffrer 4 quantités simultanément (paliers dégressifs).
6. Générer la lettre de devis client (HTML → mail + PDF).
7. Enregistrer chaque devis dans Dataverse (historique, statuts, sécurité).

---

## 2. Architecture
```
Canvas/Model-driven App
  ├─ Écran 1 : Saisie
  ├─ Écran 2 : Résultat (4 quantités)
  ├─ Écran 3 : Lettre de devis
  └─ Écran 4 : Mes devis (historique)
        │
        ▼
Tables Dataverse (référentiels + table Devis)
        │
        ▼
Azure Function (C# .NET 8 isolated) via custom connector
  ├─ POST /api/CalculerDevis  → DevisResult[]
  └─ POST /api/GenererLettre  → { html, emailClient, sujet }
```
⚠️ Le moteur de chiffrage (~50 étapes chaînées) vit dans l'Azure Function,
PAS en Power Fx. Calcul séquentiel, sans circularité.

---

## 3. Listes déroulantes dépendantes
- **Site** → filtre : Commerciaux, Profils couleur HP, Finitions, Machines.
- **Frontal → Adhesif → Backing** : cascade validée (combinaison dans table Support).
- **Type Vernis** : visible seulement si finition contient « Vernis ».
- Validation bloquante avant calcul : champs obligatoires + combinaison support valide.

---

## 4. Champs de saisie

| Section | Champ | Type |
|---|---|---|
| Client | Client, Interlocuteur, Adresse, Ville, Code postal | Text |
| Client | **EmailClient** | Text (email) — destinataire du devis |
| Devis | Désignation, N° devis | Text — N° auto : `MMJJ-Site-3letCom-N°` |
| Devis | Date | Date (auto = aujourd'hui) |
| Devis | Site | Lookup (AEME/AEEP/AEAT/AEBO/AELO/AELI/AESU) |
| Devis | Commercial | Lookup filtré par site |
| Produit | Quantité 1 à 4 | Whole (paliers) |
| Produit | Nb de références | Whole |
| Produit | Format laize (mm) | Decimal — largeur étiquette |
| Produit | Format avance (mm) | Decimal — hauteur / développé |
| Support | Frontal / Adhesif / Backing | Lookup cascade (validé vs table Support) |
| Support | Couleur Numérique | Lookup (profils HP filtrés par site) |
| Support | Numérotation | Choice (Oui/Non) |
| Finition | Couleur/Vernis Finition | Lookup (2e machine, par site) |
| Finition | Type Vernis | Choice (Mat/Satiné/Brillant/Relief) |
| Finition | Dorure 1, Dorure 2 | Lookup (table Dorure) |
| Finition | Dorure galbée, Galbe à sec, Gaufrage, Foulage, Pelliculage | Choice/Nombre |
| Rembobinage | Rembobinage, Diamètre mandrin (mm), Enroulement | Lookup/Decimal |
| Marge | Marge appliquée | Decimal % (par quantité, sur écran Résultat) |

---

## 5. Moteur de calcul — pseudo-code (Azure Function)
Appeler `calculerDevis(saisie, Q)` une fois par quantité (4 appels).

```text
function calculerDevis(saisie, Q):
  site    = Site(saisie.site)
  hp      = Machine(site, "HP6000")
  fin     = Machine(site, "Finition")
  prof    = ProfilCouleurHP(saisie.couleurNum)
  profFin = ProfilCouleurFinition(site, presse, saisie.couleurFinition)  // nullable
  sup     = Support("Frontal / Adhesif / Backing")
  P       = ParametreGlobal   // toutes les constantes
  fg      = 1 + P.FraisGeneraux   // 1.20

  // 1. IMPOSITION (voir §9 pour l'algorithme exact)
  laizeUtile = min(site.LaizeMaxiDefaut, hp.LaizeMaxi)         // ~330 mm
  nbPoses    = NbPosesLaize(saisie.laize, 4, 18, laizeUtile, dorureActive)
  mlUtile    = Q × (saisie.avance / 1000) / nbPoses

  // 2. MATIÈRE (mètres linéaires)
  ops        = opérations de finition actives (table OperationFinitionInline)
  mlCalageHP = prof.PasseMl + prof.PasseMl × max(0, nbRef-1)
  mlCalageFin= Σ op.Passe
  mlGache    = Σ op.Gache × mlUtile
  mlTotal    = mlCalageHP + mlCalageFin + mlUtile + mlGache
  M2(ml)     = ml × laizeUtile / 1000

  // 3. COÛTS (coûts matière × fg)
  coutSupport  = sup.PrixM2 × M2(mlTotal) × fg
  coutDorure   = Σ dorure.PrixHorsFG × mlTotal × 270/1000 × fg
  coutHP       = prof.CalageExtraH × hp.TauxHoraire
               + (mlTotal / (hp.VitesseBase × (1 - prof.ReductionVitesse))) × hp.TauxHoraire
  coutFinition = 0
  si ops non vide :
    coutFinition = Σ op.CalageExtra × fin.TauxHoraire
                 + (mlUtile+mlGache) / min(op.VitesseMax) × fin.TauxHoraire
  coutEncres  = CoutEncres(profFin, M2(mlUtile+mlGache))
                  // calage = Σ nb_couleur × gFixe/1000 × prix/kg
                  // roulage = Σ nb_couleur × gM2/1000 × prix/kg × taux × m²
  coutClics   = (mlTotal / DeveloppeRepeat(avance, 4, hp.DeveloppeMaxi)) × prof.NbCouleurHP
                / 1000 × prof.PrixMilleClics
  coutCliches = f(profFin, mlRoulage, prixCliche, nbRef, fg)
  emballage   = max(P.TauxConditionnement × coutSupport, P.ConditionnementMin)  // min 14,81
  transport   = P.TauxTransport × coutSupport
  fraisFixe   = P.FraisFixe   // 50
  outillages  = Σ Outillage(operation, site).Prix   // facturé séparément

  coutTotal = coutSupport + coutDorure + coutHP + coutFinition
            + coutEncres + coutClics + coutCliches + emballage + transport + fraisFixe

  // 4. PRIX
  prixDevis  = (coutTotal / P.MargeDevis) × site.CoeffChange   // ÷0,80 puis × coeff
  prixClient = prixDevis × (1 + saisie.margeManuelle)
  coutMB     = coutSupport + coutDorure + coutEncres + coutClics   // matière+conso
  margeBrute = (prixDevis - coutMB) / prixDevis

  return { prixDevis, prixClient, margeBrute, coutTotal, outillages,
           mlTotal, m2Total: M2(mlTotal), nbPosesLaize: nbPoses }
```

---

## 6. Sorties

**Écran Résultat** (par quantité) : Prix devis, MB %, Marge appliquée, Prix client ;
métrage (laize théo/réelle, ml & m² calage/roulage/total) ; imposition (nb poses laize/avance) ;
outillages (prix unitaire base & client).

**Lettre de devis** (HTML) : en-tête client + N° auto, descriptif (format H×L, matière,
impression + finitions concaténées, nb réf), tableau 4 quantités (Prix/1000 HT + Montant HT),
participation outillage, validité (date+1 mois), règlement 45 j fin de mois, contact représentant.

---

## 7. Export des seeds depuis Excel

| Table Dataverse | Source Excel |
|---|---|
| ProfilCouleurHP | Feuille `Code Couleur` rangs 1–816 |
| ProfilCouleurFinition | Feuille `Code Couleur` rangs 817–911 |
| Support | `Liste MP` Tableau5 (B3:I65) |
| Dorure | `Liste MP` Tableau7 (S3:Z24) |
| FilmPelliculage | `Liste MP` Tableau6 |
| OperationFinitionInline | `Paramètres Techniques` rangs 23–40 |
| Commercial | `Index` Tableau9 (J:M) |
| Autres tables | Feuille `ExportApp` du classeur |

---

## 8. Email client

- Champ **EmailClient** dans l'info client.
- `GenererLettre` renvoie `{ html, emailClient, sujet }`.
- Bouton « Envoyer » → flow Power Automate → Office 365 Outlook (Is HTML = Oui).
- Détails Power Fx + flow : `03_EMAIL_LETTRE_HTML.md`.

---

## 9. Imposition — algorithme exact (feuille Pose)

Constantes : Interpose = 4 mm ; Rives = 18 mm.

```text
// Nb étiquettes dans la laize :
// première pose = laize + 18 ; chaque suivante ajoute (laize + 4)
NbPosesLaize(laize, interpose=4, rives=18, laizeMaxi, dorure):
  n=0 ; w = laize + rives
  while w <= laizeMaxi : n++ ; w += laize + interpose
  if dorure : n -= 2
  return max(1, n)

// Nb poses dans le sens avance (finition) :
NbPosesAvance(avance, interpose=4, developpeMaxi):
  return max(1, floor(developpeMaxi / (avance + interpose)))

// Développé-repeat HP (pour comptage des clics) :
DeveloppeRepeat(avance, interpose=4, developpeMaxi):
  return (avance + interpose) × NbPosesAvance(avance, interpose, developpeMaxi)
```

DeveloppeMaxi par machine (table Pose.csv) :
- HP6000 : 980 mm (AESU : 981 mm)
- Finition AEME/Cartes : 305 · AEEP/Cartes : 356 · AEAT/Galaxie : 330
- AEBO/Cartes : 320 · AELO/Galaxie : 320 · AESU/Cartes : 320 · AELI/Cartes : 305

**Cas golden vérifié dans Excel** (AESU, laize=100, avance=150) :
NbPosesLaize = 3 · NbPosesAvance finition = 2 · PrixDevis = **67,60 /1000 CHF**

---

## 10. Enregistrement et historique

Table `Devis` : Statut (Choice StatutDevis : Brouillon/Envoyé/Accepté/Refusé/Perdu),
DateCreation, ValiditeJusquau, DonneesSaisieJson (snapshot entrées), LettreHtml, DevisPdf.
Numéro auto : `MMJJ-Site-3letCom-N°`. Sécurité : OwnerId (commercial voit ses devis).
Détails Power Fx (Patch, ForAll, Gallery filtrée) : `04_ENREGISTREMENT_DEVIS.md`.

---

## 11. Export PDF et tableau de bord

- **PDF** : flow `ExporterDevisPdf` (HTML → OneDrive → Convert → PDF) ou Word template.
- **Dashboard canvas** : CountRows/GroupBy/Sum par statut/commercial/mois.
- **Power BI** : connecteur Dataverse + mesures DAX (`Taux Acceptation`, `CA Accepté`).
- Détails : `05_PDF_ET_TABLEAU_DE_BORD.md`.
