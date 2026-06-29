# Exports restants depuis Excel (feuille ExportApp)

## Sources a exporter
- Support.csv (60 lignes): feuille Liste MP, Tableau5, plage B3:I65.
- Dorure.csv (20 lignes): feuille Liste MP, Tableau7, plage S3:Z24.
- ProfilCouleurHP.csv (800+ lignes): feuille Code Couleur, rangs 1-816.
- ProfilCouleurFinition.csv: feuille Code Couleur, rangs 817-911.
- Commercial.csv: feuille Index, colonnes J:M.

## Procedure recommandee
1. Ouvrir le classeur source.
2. Copier la plage exacte.
3. Coller dans un nouveau classeur vide.
4. Enregistrer en CSV UTF-8 (delimiteur point-virgule).
5. Deposer les fichiers dans ce dossier seed-data.

## Conseils Dataverse
- Verifier les en-tetes de colonnes avant import.
- Importer d abord les tables de reference (Site, Machine, etc.) puis les tables dependantes.
- Pour les decimales, conserver la virgule francaise si l environnement Dataverse est en locale FR.
