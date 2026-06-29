# Schéma Dataverse — Calculateur devis HP6000
Préfixe éditeur : `hp_`

## Jeux de choix
- Devise : EUR, CHF
- TypeMachine : HP6000, Finition, Rembobinage
- OperationFinition : Dorure, Galbe, Gaufrage, Foulage, Pelliculage, Decoupe
- TypeConsommable : EncreFlexo, EncreSeri, VernisFlexo, VernisSeri
- TypeVernis : Mat, Satine, Brillant, Relief
- StatutDevis : Brouillon, Envoye, Accepte, Refuse, Perdu

## Tables principales
**Site** : Code(alt key) | Nom | Devise | CoeffChange | LaizeMaxiDefaut
**Commercial** : Nom(key) | Site(Lookup) | Telephone | Email
**ProfilCouleurHP** : Nom(key) | ReductionVitesse | PourcentPasse | PasseMl | CalageExtraH | PrixMilleClics | NbCouleurHP
**DisponibiliteProfilSite** (N:N) : Site | ProfilCouleurHP
**ProfilCouleurFinition** : Site | Press | CodeCouleur(clé composite) | ReductionVitesse | PasseMl | CalageExtraH | NbCouleurFlexo | NbCouleurSeri | NbVernisFlexo | NbVernisSeri
**Support** : Frontal | Adhesif | Backing | Cle(alt key = "F / A / B") | PrixM2 | Fournisseur | RefId
**FilmPelliculage** : Libelle(key) | PrixM2
**Dorure** : Libelle(key) | Fournisseur | PrixHorsFG
**Machine** : Site | Type(Choice) | LaizeMini | LaizeMaxi | TauxHoraire | VitesseBase | CalagePrimerMin
**OperationFinitionInline** : Site | Press | Operation(Choice) | CalageExtra | Passe | Gache | VitesseMax
**Outillage** : Operation(Choice) | Site | Prix
**Consommable** : Type(Choice) | PrixKg | GrammeM2 | GrammeFixe | TauxEncrage
**ParametreGlobal** : Cle(key) | Valeur | Description
**Devis** : NumDevis | Date | Site | Commercial | Client | Interlocuteur | Adresse | Ville | CodePostal | EmailClient | Designation | NbReferences | FormatLaize | FormatAvance | Support | CouleurNum | (finitions) | Statut(Choice) | DateCreation | ValiditeJusquau | DonneesSaisieJson | LettreHtml | DevisPdf(File)
**DevisQuantite** : Devis | Quantite | PrixDevis | PrixClient | MargeBrute | MargeAppliquee | CoutTotal | Outillages | MontantHT | MlTotal | M2Total | NbPosesLaize | NbPosesAvance
