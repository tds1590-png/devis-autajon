param(
    [string]$SeedPath = "..\seed-data"
)

$files = @(
    "Site.csv",
    "ParametreGlobal.csv",
    "Consommable.csv",
    "Outillage.csv",
    "Machine.csv",
    "Pose.csv",
    "OperationFinitionInline.csv",
    "FilmPelliculage.csv",
    "Support.csv",
    "Dorure.csv",
    "Commercial.csv",
    "ProfilCouleurHP.csv",
    "ProfilCouleurFinition.csv"
)

Write-Host "Ordre d'import Dataverse:" -ForegroundColor Cyan
$idx = 1
foreach ($f in $files) {
    $path = Join-Path $SeedPath $f
    $exists = Test-Path $path
    $status = if ($exists) { "OK" } else { "MISSING" }
    Write-Host ("{0}. {1} [{2}]" -f $idx, $f, $status)
    $idx++
}

Write-Host "\nEnsuite importer chaque CSV depuis make.powerapps.com > Tables > Import." -ForegroundColor Yellow
