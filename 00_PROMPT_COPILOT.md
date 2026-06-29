# Prompt de démarrage pour GitHub Copilot

Lis `01_SPECIFICATION.md` et `02_SCHEMA_DATAVERSE.md`, puis :

1. Génère les tables Dataverse de `02_SCHEMA_DATAVERSE.md` (solution + jeux de choix).
2. Crée les imports de seed depuis `seed-data/` (CSV).
3. Génère une Azure Function (C# .NET isolé) `CalculerDevis` implémentant le pseudo-code §5,
   exposée via un custom connector Power Platform.
4. Génère une Canvas App à 3 écrans (Saisie / Résultat 4 quantités / Lettre de devis) avec
   listes dépendantes (Site→Commercial/Couleurs/Finitions ; Frontal→Adhesif→Backing).
5. Implémente la validation (combinaison support valide + champs obligatoires) avant calcul.

Contraintes : chiffrage en Azure Function (pas Power Fx). Calcul séquentiel sans circularité.
4 quantités = 4 appels à CalculerDevis.
