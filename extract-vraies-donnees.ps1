$seedPath = "c:\Users\petits\Devis VITI\seed-data"

Write-Host "EXTRACTION VRAIES DONNEES DEPUIS CSV..." -ForegroundColor Cyan

# 1. FRONTAL, ADHESIF, BACKING depuis Support.csv
Write-Host ""
Write-Host "Lecture Support.csv..." -ForegroundColor Yellow
$supportCsv = Import-Csv "$seedPath\Support.csv" -Delimiter ";" -Encoding UTF8
$frontals = $supportCsv | Select-Object -ExpandProperty Frontal -Unique | Where-Object {$_} | Sort-Object
$adhesifs = $supportCsv | Select-Object -ExpandProperty Adhesif -Unique | Where-Object {$_} | Sort-Object
$backings = $supportCsv | Select-Object -ExpandProperty Backing -Unique | Where-Object {$_} | Sort-Object

Write-Host "  Frontaux: $($frontals.Count)"
Write-Host "  Adhesifs: $($adhesifs.Count)"
Write-Host "  Backings: $($backings.Count)"

# 2. COULEURS depuis ProfilCouleurHP.csv
Write-Host ""
Write-Host "Lecture ProfilCouleurHP.csv..." -ForegroundColor Yellow
$profCsv = Import-Csv "$seedPath\ProfilCouleurHP.csv" -Delimiter ";" -Encoding UTF8
$colors = $profCsv | Select-Object -ExpandProperty Nom -Unique | Sort-Object
Write-Host "  Couleurs: $($colors.Count)"

# 3. PELLICULAGE
Write-Host ""
Write-Host "Lecture FilmPelliculage.csv..." -ForegroundColor Yellow
$pellCsv = Import-Csv "$seedPath\FilmPelliculage.csv" -Delimiter ";" -Encoding UTF8
$pellicles = $pellCsv | Select-Object -ExpandProperty Libelle -Unique | Sort-Object
Write-Host "  Pelliculages: $($pellicles.Count)"

# 3.5 COMMERCIAUX
Write-Host ""
Write-Host "Lecture Commercial.csv..." -ForegroundColor Yellow
$commCsv = Import-Csv "$seedPath\Commercial.csv" -Delimiter ";" -Encoding UTF8
$commercials = $commCsv | Select-Object -ExpandProperty Nom -Unique | Sort-Object
Write-Host "  Commerciaux: $($commercials.Count)"

# 4. DORURE
Write-Host ""
Write-Host "Lecture Dorure.csv..." -ForegroundColor Yellow
$dorCsv = Import-Csv "$seedPath\Dorure.csv" -Delimiter ";" -Encoding UTF8
$dorures_raw = $dorCsv | Select-Object -ExpandProperty Libelle -Unique | Sort-Object
$dorures = $dorures_raw | Where-Object {$_ -and $_ -ne "3"}
if ($dorures.Count -eq 0) {
    Write-Host "  (CSV vide, valeurs par defaut)"
    $dorures = @("Dorure Or", "Dorure Argent", "Dorure Hologramme")
}
Write-Host "  Dorures: $($dorures.Count)"

# 5. VERNIS
$vernis = @("Brillant", "Mat", "Satiné", "Relief")
Write-Host ""
Write-Host "Vernis: $($vernis.Count)"

# Creer le JSON
$data = @{
    frontals = @($frontals)
    adhesifs = @($adhesifs)
    backings = @($backings)
    colors = @($colors)
    commercials = @($commercials)
    pellicles = @($pellicles)
    dorures = @($dorures)
    vernis = @($vernis)
}

$json = $data | ConvertTo-Json
$json | Out-File "c:\Users\petits\Devis VITI\dataverse-data-complet.json" -Encoding UTF8 -Force

Write-Host ""
Write-Host "OK - Donnees extraites!" -ForegroundColor Green
Write-Host "Fichier: dataverse-data-complet.json" -ForegroundColor Cyan
