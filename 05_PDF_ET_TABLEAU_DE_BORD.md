# Export PDF & Tableau de bord

## PARTIE A — Export PDF

### Option 1 (recommandée) — Flow "ExporterDevisPdf"
1. Déclencheur PowerApps (V2) : `NumDevis`, `LettreHtml`
2. OneDrive → Créer fichier `/Devis/{NumDevis}.html`
3. OneDrive → Convertir en **PDF**
4. Au choix : Dataverse Mettre à jour ligne (champ `DevisPdf`) | mail avec pièce jointe | répondre base64
5. (Nettoyage) Supprimer le .html temporaire

```powerfx
// Bouton "Télécharger PDF"
Set(varPdf, ExporterDevisPdf.Run(varDevis.NumDevis, varDevis.LettreHtml));
Patch(Devis, varDevis, { Statut: 'StatutDevis'.Envoye })
```

### Option 2 — Modèle Word
Word Online → Remplir un modèle → Convertir en PDF. Meilleur rendu charte graphique.

### Option 3 — Fonction PDF() canvas
`Set(varPdf, PDF(ScreenDevis, {Size: PaperSize.A4}))` — capture d'écran rapide, sans flow.

> Archiver le PDF au passage en statut Envoyé pour trace immuable.

## PARTIE B — Tableau de bord

### Canvas (intégré à l'app)
```powerfx
// Devis du mois
CountRows(Filter(Devis, Year(DateCreation)=Year(Now()) && Month(DateCreation)=Month(Now())))

// Pipeline par statut
ForAll(['StatutDevis'.Brouillon,'StatutDevis'.Envoye,'StatutDevis'.Accepte,
        'StatutDevis'.Refuse,'StatutDevis'.Perdu] As s,
  { Statut: s, Nb: CountRows(Filter(Devis, Statut = s)) })

// Taux d'acceptation par commercial
AddColumns(
  GroupBy(Filter(Devis, Statut in ['StatutDevis'.Envoye,'StatutDevis'.Accepte,
                                    'StatutDevis'.Refuse,'StatutDevis'.Perdu]),
          "Commercial","lignes"),
  "Traites",   CountRows(lignes),
  "Acceptes",  CountRows(Filter(lignes, Statut='StatutDevis'.Accepte)),
  "TauxAccept",CountRows(Filter(lignes,Statut='StatutDevis'.Accepte))/CountRows(lignes)
)
```
> Remplir `MontantHT` à l'enregistrement (PrixClient/1000 × Quantite) pour des Sum() délégables.

### Power BI (recommandé pour l'analyse riche)
Connecteur Dataverse → tables Devis + DevisQuantite + Commercial + Site.
```dax
Nb Devis       = DISTINCTCOUNT(Devis[NumDevis])
Taux Acceptation = DIVIDE(
    CALCULATE([Nb Devis], Devis[Statut]="Accepte"),
    CALCULATE([Nb Devis], Devis[Statut] IN {"Envoye","Accepte","Refuse","Perdu"}))
CA Accepte     = CALCULATE(SUM(DevisQuantite[MontantHT]), Devis[Statut]="Accepte")
```
Intégrer dans l'app via le composant **Power BI tile**.
