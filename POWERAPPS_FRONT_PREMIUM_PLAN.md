# Plan Front Premium - Devis HP (Power Apps)

## Etat actuel
- Les donnees seed et la structure back (Azure Function + docs) sont en place.
- La construction visuelle Power Apps n'est pas encore lancee dans l'environnement.

## Objectif UX
- Interface premium orientee clients haut de gamme.
- Parcours rapide, lisible, rassurant, avec forte qualite de presentation.
- Sortie devis claire, elegante, imprimable et envoyable.

## Architecture recommandee (premium)
1. Model-driven App pour la robustesse data (Dataverse, navigation, securite, historisation).
2. Custom Pages pour les ecrans experience (Saisie, Resultat, Lettre).
3. Composants PCF pour les zones critiques:
   - Selecteurs dependants (Support/Finition)
   - Cartes tarifaires premium (4 paliers)
   - Badge statut et timeline devis
4. Theme entreprise (charte premium):
   - Typo sobre
   - Palette neutre + accent de marque
   - Espacements constants + systeme de composants

## Ecrans cibles
### 1. Saisie Devis (Custom Page)
- Grille 2 colonnes desktop, 1 colonne mobile.
- Blocs: Client, Produit, Support, Finition, Conditions commerciales.
- Validation progressive (inline) et resume lateral.

### 2. Resultat 4 quantites (Custom Page)
- Cartes comparatives premium (quantite, prix/1000, montant, marge).
- Bloc details techniques repliable (ml, m2, poses).
- Actions hautes: Recalculer, Enregistrer, Envoyer, Export PDF.

### 3. Lettre de devis (Custom Page)
- Apercu WYSIWYG du HTML.
- Validation contact + bouton Envoi.
- Historique des versions et statut.

### 4. Cockpit Commercial (Model-driven + dashboard)
- Pipeline statuts, taux d'acceptation, CA accepte, filtres site/commercial.

## Standards visuels premium
- Rayon de bordure discret, ombres legeres, contraste eleve.
- Titres courts, sous-titres explicites, densite d'information maitrisee.
- Micro-interactions utiles uniquement (chargement, confirmation, erreurs).

## Qualite et gouvernance
- Security roles: Commercial (user), Manager (org).
- Journalisation: statut + email + PDF.
- Performance: calcul externalise Azure Function (deja aligne).

## Plan de build (ordre)
1. Creer la Model-driven app squelette (navigation + tables).
2. Creer les 3 Custom Pages premium.
3. Connecter actions Calculer/GenererLettre.
4. Brancher Enregistrer/Envoyer/Exporter PDF.
5. Ajuster theme, composants et responsive.
6. Revue UX premium + recette metier.

## Definition of Done (front)
- Parcours complet saisie -> resultat -> envoi fluide.
- Design coherent premium desktop et tablette.
- Aucun champ technique expose inutilement au client.
- Demo metier validee sur le cas golden AESU.
