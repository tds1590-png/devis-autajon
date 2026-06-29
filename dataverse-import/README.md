# Package d'import Dataverse

## Ordre d'import recommandé
1. Site.csv
2. ParametreGlobal.csv
3. Consommable.csv
4. Outillage.csv
5. Machine.csv
6. Pose.csv
7. OperationFinitionInline.csv
8. FilmPelliculage.csv
9. Support.csv
10. Dorure.csv
11. Commercial.csv
12. ProfilCouleurHP.csv
13. ProfilCouleurFinition.csv

## Mapping source -> table Dataverse
- Site.csv -> hp_site
- ParametreGlobal.csv -> hp_parametreglobal
- Consommable.csv -> hp_consommable
- Outillage.csv -> hp_outillage
- Machine.csv -> hp_machine
- Pose.csv -> hp_pose
- OperationFinitionInline.csv -> hp_operationfinitioninline
- FilmPelliculage.csv -> hp_filmpelliculage
- Support.csv -> hp_support
- Dorure.csv -> hp_dorure
- Commercial.csv -> hp_commercial
- ProfilCouleurHP.csv -> hp_profilcouleurhp
- ProfilCouleurFinition.csv -> hp_profilcouleurfinition

## Mapping detaille colonnes
Voir le document mapping-detaille-citizen-dev-eu.md pour le mapping colonne par colonne,
les choices a pre-creer, les lookups et les verifications post-import.

## Vérifications post-import
- Contrôler les doublons sur les clés alternatives (Site.Code, Support.Cle, etc.).
- Contrôler les décimales (virgule/point) sur PrixM2, PrixKg, CoeffChange.
- Exécuter un test de calcul avec le cas AESU (100 x 150) pour valider les dépendances.
