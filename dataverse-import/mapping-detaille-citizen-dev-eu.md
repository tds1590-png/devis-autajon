# Mapping detaille CSV -> Dataverse (Citizen Dev EU)

## Regles generales
- Prefixe logique: hp_.
- Import en UTF-8, separateur ;
- Decimales: verifier la locale FR (virgule) pendant l import.
- Pour les colonnes Choice: mapper les libelles (pas les valeurs numeriques).
- Pour les colonnes Lookup: mapper via cle metier (Code, Cle, Nom) quand disponible.

## Site.csv -> hp_site
- Code -> hp_code (Texte, cle alternative)
- Nom -> hp_nom (Texte)
- Devise -> hp_devise (Choice Devise: EUR/CHF)
- CoeffChange -> hp_coeffchange (Decimal)
- LaizeMaxiDefaut -> hp_laizemaxidefaut (Decimal)

## ParametreGlobal.csv -> hp_parametreglobal
- Cle -> hp_cle (Texte, cle)
- Valeur -> hp_valeur (Decimal)
- Description -> hp_description (Texte)

## Consommable.csv -> hp_consommable
- Type -> hp_type (Choice TypeConsommable)
- PrixKg -> hp_prixkg (Decimal)
- GrammeM2 -> hp_grammem2 (Decimal)
- GrammeFixe -> hp_grammefixe (Decimal)
- TauxEncrage -> hp_tauxencrage (Decimal)

## Outillage.csv -> hp_outillage
- Operation -> hp_operation (Choice OperationFinition)
- Prix -> hp_prix (Decimal)
- Note -> hp_note (Texte)
- Site: si absent du CSV, importer par defaut global ou enrichir le CSV avec colonne Site avant import final.

## Machine.csv -> hp_machine
- Site -> hp_site (Lookup -> hp_site via hp_code)
- Type -> hp_type (Choice TypeMachine)
- LaizeMini -> hp_laizemini (Decimal)
- LaizeMaxi -> hp_laizemaxi (Decimal)
- TauxHoraire -> hp_tauxhoraire (Decimal)
- VitesseBase -> hp_vitessebase (Decimal)
- CalagePrimerMin -> hp_calageprimermin (Decimal)

## Pose.csv -> hp_pose
- Site -> hp_site (Lookup -> hp_site via hp_code)
- Type -> hp_type (Choice TypeMachine)
- Presse -> hp_presse (Texte)
- DeveloppeMaxi -> hp_developpemaxi (Decimal)

## OperationFinitionInline.csv -> hp_operationfinitioninline
- Site -> hp_site (Lookup -> hp_site via hp_code)
- Press -> hp_press (Texte)
- Operation -> hp_operation (Choice OperationFinition)
- CalageExtra -> hp_calageextra (Decimal)
- Passe -> hp_passe (Decimal)
- Gache -> hp_gache (Decimal)
- VitesseMax -> hp_vitessemax (Decimal)

## FilmPelliculage.csv -> hp_filmpelliculage
- Libelle -> hp_libelle (Texte, cle)
- PrixM2 -> hp_prixm2 (Decimal)

## Support.csv -> hp_support
- Frontal -> hp_frontal (Texte)
- Adhesif -> hp_adhesif (Texte)
- Backing -> hp_backing (Texte)
- Cle -> hp_cle (Texte, cle alternative)
- PrixM2 -> hp_prixm2 (Decimal)
- Fournisseur -> hp_fournisseur (Texte)
- RefId -> hp_refid (Texte)

## Dorure.csv -> hp_dorure
- Libelle -> hp_libelle (Texte, cle)
- Fournisseur -> hp_fournisseur (Texte)
- PrixHorsFG -> hp_prixhorsfg (Decimal)

## Commercial.csv -> hp_commercial
- Nom -> hp_nom (Texte)
- Site -> hp_site (Lookup -> hp_site via hp_code)
- Telephone -> hp_telephone (Texte)
- Email -> hp_email (Texte)

## ProfilCouleurHP.csv -> hp_profilcouleurhp
- Nom -> hp_nom (Texte, cle)
- ReductionVitesse -> hp_reductionvitesse (Decimal)
- PourcentPasse -> hp_pourcentpasse (Decimal)
- PasseMl -> hp_passeml (Decimal)
- CalageExtraH -> hp_calageextrah (Decimal)
- PrixMilleClics -> hp_prixmilleclics (Decimal)
- NbCouleurHP -> hp_nbcouleurhp (Entier)

## ProfilCouleurFinition.csv -> hp_profilcouleurfinition
- Site -> hp_site (Lookup -> hp_site via hp_code)
- Press -> hp_press (Texte)
- CodeCouleur -> hp_codecouleur (Texte)
- ReductionVitesse -> hp_reductionvitesse (Decimal)
- PasseMl -> hp_passeml (Decimal)
- CalageExtraH -> hp_calageextrah (Decimal)
- NbCouleurFlexo -> hp_nbcouleurflexo (Entier)
- NbCouleurSeri -> hp_nbcouleurseri (Entier)
- NbVernisFlexo -> hp_nbvernisflexo (Entier)
- NbVernisSeri -> hp_nbvernisseri (Entier)

## Choices a preparer avant import
- Devise: EUR, CHF
- TypeMachine: HP6000, Finition, Rembobinage
- OperationFinition: Dorure, Galbe, Gaufrage, Foulage, Pelliculage, Decoupe
- TypeConsommable: EncreFlexo, EncreSeri, VernisFlexo, VernisSeri
- TypeVernis: Mat, Satine, Brillant, Relief
- StatutDevis: Brouillon, Envoye, Accepte, Refuse, Perdu

## Ordre d import conseille
1. hp_site
2. hp_parametreglobal
3. hp_consommable
4. hp_outillage
5. hp_machine
6. hp_pose
7. hp_operationfinitioninline
8. hp_filmpelliculage
9. hp_support
10. hp_dorure
11. hp_commercial
12. hp_profilcouleurhp
13. hp_profilcouleurfinition

## Validation post-import
- Controler les doublons sur hp_code, hp_cle, hp_nom selon table.
- Controler les lookups resolves (site/commercial/support).
- Verifier un calcul de reference AESU (laize 100, avance 150).
