# Calculateur de devis HP6000 → Power Apps + Dataverse

Dossier complet pour reproduire, avec l'agent GitHub (VS Code), le calculateur de devis Excel
(étiquettes adhésives, impression numérique HP Indigo 6000, multi-sites Autajon) en application
Power Apps adossée à Dataverse, avec un moteur de chiffrage en Azure Function.

## Par où commencer
1. Donner à l'agent : « Lis `00_PROMPT_COPILOT.md` puis construis par phases. »
2. Suivre l'ordre de construction ci-dessous, en validant chaque phase.

## Index des documents
| # | Fichier | Contenu |
|---|---|---|
| 00 | `00_PROMPT_COPILOT.md` | Prompt de démarrage pour l'agent |
| 01 | `01_SPECIFICATION.md` | Spéc fonctionnelle complète : saisie, **moteur de calcul** (pseudo-code), imposition (§9), email (§8), enregistrement (§10), PDF/dashboard (§11) |
| 02 | `02_SCHEMA_DATAVERSE.md` | 15 tables Dataverse, types, clés, relations, 6 jeux de choix |
| 03 | `03_EMAIL_LETTRE_HTML.md` | Email client : lettre HTML + bouton Power Fx + flow Outlook |
| 04 | `04_ENREGISTREMENT_DEVIS.md` | Enregistrer/retrouver/rouvrir/dupliquer + statuts + sécurité |
| 05 | `05_PDF_ET_TABLEAU_DE_BORD.md` | Export PDF (flow OneDrive) + dashboard canvas & Power BI |

## Arborescence
```
devis-hp-powerapps/
├─ README.md                          ← ce fichier
├─ 00_PROMPT_COPILOT.md
├─ 01_SPECIFICATION.md
├─ 02_SCHEMA_DATAVERSE.md
├─ 03_EMAIL_LETTRE_HTML.md
├─ 04_ENREGISTREMENT_DEVIS.md
├─ 05_PDF_ET_TABLEAU_DE_BORD.md
├─ azure-function/
│   ├─ CalculerDevisFunction.cs       ← endpoint POST /api/CalculerDevis
│   ├─ GenererLettreFunction.cs       ← endpoint POST /api/GenererLettre
│   ├─ README.md
│   └─ src/
│       ├─ Models.cs                  ← DTO + entités référentielles
│       ├─ IReferenceRepository.cs    ← interface accès Dataverse
│       ├─ DevisCalculator.cs         ← moteur de chiffrage (portage Excel)
│       ├─ PoseCalculator.cs          ← imposition (NbPoses, DeveloppeRepeat)
│       └─ LettreHtmlBuilder.cs       ← génération lettre HTML
├─ seed-data/
│   ├─ Site.csv                       ✅ rempli (7 sites, coeff CHF AESU=1.06607)
│   ├─ ParametreGlobal.csv            ✅ rempli (FG 20%, ÷0.80, conditionnement…)
│   ├─ Outillage.csv                  ✅ rempli (découpe 240, dorure 140, galbé 530…)
│   ├─ Consommable.csv                ✅ rempli (encres/vernis, g/m², taux encrage)
│   ├─ Machine.csv                    ✅ rempli (HP6000 taux 210€/h, finition 107€/h)
│   ├─ Pose.csv                       ✅ rempli (développé maxi HP=980, finition 305-356)
│   ├─ OperationFinitionInline.csv    ✅ rempli (22 opérations : calage, gâche, vitesse)
│   ├─ FilmPelliculage.csv            ✅ rempli (1 film PP TC MATT SOFT TOUCH)
│   ├─ Support.csv                    ← exporter 'Liste MP' Tableau5 (B3:I65)
│   ├─ Dorure.csv                     ← exporter 'Liste MP' Tableau7 (S3:Z24)
│   ├─ ProfilCouleurHP_TEMPLATE.csv   ← exporter 'Code Couleur' rangs 1-816
│   ├─ ProfilCouleurFinition_TEMPLATE.csv ← exporter 'Code Couleur' rangs 817-911
│   └─ Commercial_TEMPLATE.csv        ← exporter 'Index' colonnes J:M
└─ tests/
    ├─ PoseTests.cs                   ← tests imposition (NbPosesLaize, NbPosesAvance)
    ├─ DevisCalculatorTests.cs        ← test moteur + FakeRepository
    ├─ cas_reference.json             ← cas GOLDEN Excel (AESU, 67,60/1000 CHF)
    └─ README_TESTS.md
```

## Ordre de construction (7 phases)
1. **Dataverse** — créer les tables + jeux de choix (doc 02), importer les CSV de `seed-data/`
2. **Compléter les 4 gabarits** depuis le classeur Excel source (voir `seed-data/` ci-dessus)
3. **Azure Function** — implémenter `IReferenceRepository` sur Dataverse (ServiceClient),
   brancher `DeveloppeParPoseAvance` via `Pose.csv` + `PoseCalculator`, déployer
4. **Custom connector** — exposer `CalculerDevis` + `GenererLettre`
5. **Canvas App** — 3 écrans (Saisie / Résultat / Lettre) + listes dépendantes
6. **Enregistrement** (doc 04) + **Email** (doc 03) + **PDF + dashboard** (doc 05)
7. **Validation** — rejouer `tests/cas_reference.json` : viser **PrixDevis = 67,60/1000 CHF**
   au centime (site AESU, laize=100, avance=150, support Frozen Orion Diamond/Lavable/Glassine,
   couleur Noir, finition Ni couleur ni vernis). Tant que l'écart n'est pas nul, calibrer le moteur.

## Données de référence figées
| Constante | Valeur |
|---|---|
| Frais généraux (FG) | 20 % (× 1,20 sur coûts matière) |
| Marge intégrée | prixDevis = coûtTotal ÷ **0,80** |
| Coeff change AESU (CHF) | **1,06607** (tous autres sites EUR = 1) |
| Interpose | 4 mm entre étiquettes |
| Rives | 18 mm (9+9 marges latérales) |
| Laize découpe max | 330 mm |
| Conditionnement min | 14,81 € |
| Taux transport | 6,3 % du coût support |
| Frais fixe | 50 € par devis |
| HP6000 taux horaire | 210 €/h |
| Finition taux horaire | 107 €/h |
| HP6000 vitesse base | 3 600 f/h |
| HP6000 développé max | 980 mm (AESU : 981 mm) |
