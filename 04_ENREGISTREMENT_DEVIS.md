# Enregistrement & historique des devis

## 1. Champs Devis (en plus des champs de saisie)
- `Statut` Choice(StatutDevis) : Brouillon → Envoyé → Accepté/Refusé/Perdu
- `DateCreation`, `ValiditeJusquau` (= +1 mois), `DonneesSaisieJson` (snapshot JSON),
  `LettreHtml`, `DevisPdf` (File). `OwnerId`/`CreatedBy`/`ModifiedOn` = champs système.

## 2. Numérotation automatique
Format : `MMJJ-Site-3letCom-N°`. Option : colonne auto-number Dataverse.

## 3. Bouton "Enregistrer"
```powerfx
Set(varDevis,
  Patch(Devis, If(IsBlank(varDevis), Defaults(Devis), varDevis), {
    NumDevis: txtNumDevis.Text, Designation: txtDesign.Text, Date: Today(),
    Site: drpSite.Selected, Commercial: drpCommercial.Selected,
    Client: txtClient.Text, Interlocuteur: txtInterlo.Text,
    Adresse: txtAdresse.Text, Ville: txtVille.Text, CodePostal: txtCP.Text,
    EmailClient: txtEmail.Text, NbReferences: Value(txtNbRef.Text),
    FormatLaize: Value(txtLaize.Text), FormatAvance: Value(txtAvance.Text),
    Support: drpSupport.Selected, CouleurNum: drpCouleur.Selected,
    Statut: 'StatutDevis'.Brouillon,
    DateCreation: Now(), ValiditeJusquau: DateAdd(Today(),1,Months),
    DonneesSaisieJson: JSON(varProduit, JSONFormat.Compact)
  })
);
ForAll(varResultats As q,
  Patch(DevisQuantite, Defaults(DevisQuantite), {
    Devis: varDevis, Quantite: q.Quantite,
    PrixDevis: q.PrixDevis, PrixClient: q.PrixClient, MargeBrute: q.MargeBrute,
    MargeAppliquee: q.MargeManuelle, CoutTotal: q.CoutTotal, Outillages: q.Outillages,
    MontantHT: q.PrixClient / 1000 * q.Quantite,
    MlTotal: q.MlTotal, M2Total: q.M2Total,
    NbPosesLaize: q.NbPosesLaize, NbPosesAvance: q.NbPosesAvance
  })
);
Notify("Devis " & varDevis.NumDevis & " enregistré", NotificationType.Success)
```

## 4. Écran "Mes devis"
```powerfx
SortByColumns(
  Filter(Devis,
    OwnerId.Email = User().Email &&
    (IsBlank(txtRecherche.Text) || txtRecherche.Text in Client) &&
    (drpStatut.Selected.Value = "Tous" || Statut.Value = drpStatut.Selected.Value)),
  "DateCreation", Descending)
```
Colonnes galerie : NumDevis · Client · Date · Statut (badge couleur) · PrixClient 1ère qté.

## 5. Rouvrir / Dupliquer
- **Rouvrir** : désérialiser `DonneesSaisieJson` → remplir contrôles de saisie → recalculer.
- **Dupliquer** : recharger entrées dans un nouveau Devis (nouveau N°, Statut=Brouillon).

## 6. Sécurité
Rôle commercial : accès "Utilisateur" sur Devis (basé OwnerId).
Rôle manager : accès "Organisation" (voit tous les devis).
