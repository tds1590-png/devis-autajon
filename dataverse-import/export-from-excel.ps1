param(
  [string]$WorkbookPath = "C:\Users\petits\Devis VITI\Copie de Devis HP final Refacto.xlsm",
  [string]$OutputPath = "C:\Users\petits\Devis VITI\seed-data"
)

$ErrorActionPreference = 'Stop'

function Clean-Num {
  param(
    [string]$Value,
    [switch]$Percent
  )

  if ([string]::IsNullOrWhiteSpace($Value)) { return '' }
  $s = $Value.Replace([char]160, ' ').Trim()
  $s = $s -replace '€', ''
  $s = $s -replace ' ', ''

  if ($Percent) {
    $s = $s -replace '%', ''
  }

  $s = $s.Replace(',', '.')

  $n = 0.0
  if ([double]::TryParse($s, [System.Globalization.NumberStyles]::Any, [System.Globalization.CultureInfo]::InvariantCulture, [ref]$n)) {
    if ($Percent) {
      return ($n / 100.0).ToString([System.Globalization.CultureInfo]::InvariantCulture)
    }
    return $n.ToString([System.Globalization.CultureInfo]::InvariantCulture)
  }

  return $s
}

function Get-Text {
  param($Cell)
  return [string]$Cell.Text
}

if (!(Test-Path $WorkbookPath)) {
  throw "Workbook not found: $WorkbookPath"
}

if (!(Test-Path $OutputPath)) {
  New-Item -ItemType Directory -Path $OutputPath | Out-Null
}

$excel = New-Object -ComObject Excel.Application
$excel.Visible = $false
$excel.DisplayAlerts = $false
$wb = $excel.Workbooks.Open($WorkbookPath)

try {
  # Support.csv
  $ws = $wb.Worksheets.Item('Liste MP')
  $rows = @()
  for ($r = 4; $r -le 65; $r++) {
    $refSupport = Get-Text $ws.Cells.Item($r, 2)
    if ([string]::IsNullOrWhiteSpace($refSupport)) { continue }

    $fourn = Get-Text $ws.Cells.Item($r, 3)
    $frontal = Get-Text $ws.Cells.Item($r, 5)
    $adh = Get-Text $ws.Cells.Item($r, 6)
    $back = Get-Text $ws.Cells.Item($r, 8)
    $prix = Clean-Num (Get-Text $ws.Cells.Item($r, 9))
    $cle = "$frontal / $adh / $back"

    $rows += [pscustomobject]@{
      Frontal = $frontal
      Adhesif = $adh
      Backing = $back
      Cle = $cle
      PrixM2 = $prix
      Fournisseur = $fourn
      RefId = $refSupport
    }
  }
  $rows | Export-Csv -Path (Join-Path $OutputPath 'Support.csv') -NoTypeInformation -Encoding UTF8 -Delimiter ';'

  # Dorure.csv
  $rows = @()
  for ($r = 4; $r -le 24; $r++) {
    $lib = Get-Text $ws.Cells.Item($r, 24) # X
    if ([string]::IsNullOrWhiteSpace($lib)) { continue }

    $fourn = Get-Text $ws.Cells.Item($r, 21) # U
    $prix = Clean-Num (Get-Text $ws.Cells.Item($r, 25)) # Y

    $rows += [pscustomobject]@{
      Libelle = $lib
      Fournisseur = $fourn
      PrixHorsFG = $prix
    }
  }
  $rows | Export-Csv -Path (Join-Path $OutputPath 'Dorure.csv') -NoTypeInformation -Encoding UTF8 -Delimiter ';'

  # ProfilCouleurHP.csv
  $wsCC = $wb.Worksheets.Item('Code Couleur')
  $rows = @()
  for ($r = 2; $r -le 816; $r++) {
    $site = Get-Text $wsCC.Cells.Item($r, 1)
    if ([string]::IsNullOrWhiteSpace($site)) { continue }

    $nom = Get-Text $wsCC.Cells.Item($r, 4)
    if ([string]::IsNullOrWhiteSpace($nom)) { continue }

    $rows += [pscustomobject]@{
      Nom = $nom
      ReductionVitesse = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 5)) -Percent)
      PourcentPasse = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 6)) -Percent)
      PasseMl = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 7)))
      CalageExtraH = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 8)))
      PrixMilleClics = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 9)))
      NbCouleurHP = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 14)))
      Site = $site
      Press = (Get-Text $wsCC.Cells.Item($r, 3))
      Cle = (Get-Text $wsCC.Cells.Item($r, 2))
    }
  }
  $rows | Export-Csv -Path (Join-Path $OutputPath 'ProfilCouleurHP.csv') -NoTypeInformation -Encoding UTF8 -Delimiter ';'

  # ProfilCouleurFinition.csv
  $rows = @()
  for ($r = 817; $r -le 911; $r++) {
    $site = Get-Text $wsCC.Cells.Item($r, 1)
    if ([string]::IsNullOrWhiteSpace($site)) { continue }

    $press = Get-Text $wsCC.Cells.Item($r, 3)
    $code = Get-Text $wsCC.Cells.Item($r, 15)
    if ([string]::IsNullOrWhiteSpace($code)) {
      $code = Get-Text $wsCC.Cells.Item($r, 4)
    }

    $rows += [pscustomobject]@{
      Site = $site
      Press = $press
      CodeCouleur = $code
      ReductionVitesse = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 5)) -Percent)
      PasseMl = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 7)))
      CalageExtraH = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 8)))
      NbCouleurFlexo = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 10)))
      NbCouleurSeri = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 11)))
      NbVernisFlexo = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 12)))
      NbVernisSeri = (Clean-Num (Get-Text $wsCC.Cells.Item($r, 13)))
    }
  }
  $rows | Export-Csv -Path (Join-Path $OutputPath 'ProfilCouleurFinition.csv') -NoTypeInformation -Encoding UTF8 -Delimiter ';'

  # Commercial.csv
  $wsI = $wb.Worksheets.Item('Index')
  $rows = @()
  for ($r = 2; $r -le 1000; $r++) {
    $site = Get-Text $wsI.Cells.Item($r, 10)
    if ([string]::IsNullOrWhiteSpace($site)) { break }

    $rows += [pscustomobject]@{
      Nom = (Get-Text $wsI.Cells.Item($r, 11))
      Site = $site
      Telephone = (Get-Text $wsI.Cells.Item($r, 12))
      Email = (Get-Text $wsI.Cells.Item($r, 13))
    }
  }
  $rows | Export-Csv -Path (Join-Path $OutputPath 'Commercial.csv') -NoTypeInformation -Encoding UTF8 -Delimiter ';'

  Write-Output 'CSV export done.'
}
finally {
  $wb.Close($false)
  $excel.Quit()
  [System.Runtime.Interopservices.Marshal]::ReleaseComObject($wb) | Out-Null
  [System.Runtime.Interopservices.Marshal]::ReleaseComObject($excel) | Out-Null
  [GC]::Collect()
  [GC]::WaitForPendingFinalizers()
}
