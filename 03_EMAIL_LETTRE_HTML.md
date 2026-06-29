# Envoi du devis par email — Power Apps + Power Automate

## 1. Champ EmailClient
Table Devis : `EmailClient` (Text, format email) dans le bloc info client.

## 2. Générer la lettre HTML
L'Azure Function `GenererLettre` (POST /api/GenererLettre) reçoit :
`{ info: InfoDevis, produit: DevisInput, quantites: DevisInput[] }`
et renvoie : `{ html: string, emailClient: string, sujet: string }`

Appel depuis l'écran Résultat (OnVisible ou bouton Aperçu) :
```powerfx
Set(varLettre,
  CalculerDevisConnector.GenererLettre({
    info: { Client: txtClient.Text, Interlocuteur: txtInterlo.Text,
            Adresse: txtAdresse.Text, Ville: txtVille.Text, CodePostal: txtCP.Text,
            EmailClient: txtEmail.Text, Designation: txtDesign.Text,
            NumDevis: txtNumDevis.Text, Date: Today(), Site: drpSite.Selected.Code,
            Commercial: drpCommercial.Selected.Nom,
            CommercialTel: drpCommercial.Selected.Telephone,
            CommercialEmail: drpCommercial.Selected.Email,
            NbReferences: Value(txtNbRef.Text) },
    produit: varProduit,
    quantites: varQuantites
  })
)
```

## 3. Bouton "Envoyer le devis par mail"
```powerfx
If(IsBlank(txtEmail.Text) || !IsMatch(txtEmail.Text, Match.Email),
   Notify("Email client invalide", NotificationType.Error),
   EnvoyerDevisFlow.Run(varLettre.emailClient, varLettre.sujet, varLettre.html);
   Patch(Devis, varDevis, { Statut: 'StatutDevis'.Envoye, LettreHtml: varLettre.html });
   Notify("Devis envoyé à " & varLettre.emailClient, NotificationType.Success)
)
```

## 4. Flow Power Automate "EnvoyerDevisFlow"
- **Déclencheur** : PowerApps (V2) — 3 paramètres texte : `To`, `Sujet`, `CorpsHtml`
- **Action** : Office 365 Outlook → Envoyer un e-mail (V2)
  - À : `To` | Objet : `Sujet` | Corps : `CorpsHtml` | **Is HTML : Oui**
  - (optionnel) Cc : CommercialEmail ; Pièce jointe PDF si disponible

## 5. Structure de la lettre HTML (LettreHtmlBuilder.cs)
- En-tête : client, adresse, N° devis, date
- Descriptif : format H×L, matière (Frontal Adhesif Backing), impression + finitions concaténées
- Tableau 4 quantités : Qté | Prix/1000 HT | Montant HT
- Participation aux frais d'outillage : libellé | qté | prix unitaire
- Validité : date + 1 mois | Règlement : 45 jours fin de mois
- Représentant : nom, tél, email
