# Tests du moteur de devis

## Contenu
- PoseTests.cs: vérifie les calculs d'imposition (laize/avance).
- DevisCalculatorTests.cs: vérifie un cas de calibration golden (AESU, 67,60/1000 CHF).
- cas_reference.json: paramètres de référence du cas golden.

## Exécution
Depuis le dossier `tests`:

```powershell
dotnet test
```

Ou depuis la racine:

```powershell
dotnet test .\tests\DevisHp.Tests.csproj
```
