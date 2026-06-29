using System.Globalization;
using System.Text;
using DevisHp.Models;

namespace DevisHp.Core;

/// <summary>
/// Génère la lettre de devis en HTML. Reproduit la feuille Excel "Lettre Devis".
/// Résultat = corps de mail (Is HTML = Oui dans Outlook).
/// </summary>
public static class LettreHtmlBuilder {

    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");
    private static string Eur(decimal v) => v.ToString("#,##0.00", Fr) + " €";

    public static string Generer(InfoDevis d, DevisInput x, IList<DevisResult> res) {
        // Concaténation des finitions (identique à la feuille Lettre Devis E17)
        var fin = new StringBuilder(x.CouleurNum);
        if (!string.IsNullOrEmpty(x.CouleurFinition)) {
            fin.Append(" + " + x.CouleurFinition);
            if (x.CouleurFinition.Contains("Vernis", StringComparison.OrdinalIgnoreCase)
                && x.TypeVernis != null)
                fin.Append($" ({x.TypeVernis})");
        }
        foreach (var dor in x.Dorures)
            fin.Append((x.DorureGalbee ? " + Dorure galbée " : " + Dorure ") + dor);
        if (x.GalbeASec)   fin.Append(" + Galbe à sec");
        if (x.Gaufrage)    fin.Append(" + Gaufrage");
        if (x.Foulage)     fin.Append(" + Foulage");
        if (x.Pelliculage) fin.Append(" + Pelliculage");

        // Lignes du tableau tarifs
        var lignes = new StringBuilder();
        foreach (var r in res.Where(r => r.Quantite > 0)) {
            decimal montant = r.PrixClient / 1000m * r.Quantite;
            lignes.Append(
                $"<tr><td>{r.Quantite.ToString("#,##0", Fr)} ex.</td>" +
                $"<td style='text-align:right'>{Eur(r.PrixClient)}</td>" +
                $"<td style='text-align:right'>{Eur(montant)}</td></tr>");
        }

        return $@"<!DOCTYPE html><html><head><meta charset='utf-8'>
<style>
  body {{ font-family: Calibri, Arial, sans-serif; font-size: 13px; color: #222; }}
  h2   {{ color: #44B3E1; font-size: 14px; }}
  table{{ border-collapse: collapse; width: 100%; margin: 8px 0; }}
  th,td{{ border: 1px solid #ccc; padding: 5px 8px; }}
  th   {{ background: #44B3E1; color: #fff; }}
  .desc td {{ border: none; padding: 2px 6px; }}
</style></head><body>
<p>Le {d.Date.ToString("dd MMMM yyyy", Fr)}</p>
<p><strong>{d.Client}</strong><br>{d.Adresse}<br>{d.CodePostal} {d.Ville}</p>
<p>N° Devis {d.NumDevis}</p>
<p>À l'attention de {d.Interlocuteur},<br>
Nous avons le plaisir de vous remettre ci-dessous notre offre.</p>

<h2>Descriptif</h2>
<table class='desc'>
  <tr><td><strong>Désignation :</strong></td><td>{d.Designation}</td></tr>
  <tr><td><strong>Format (H × L) :</strong></td><td>{x.FormatAvance} × {x.FormatLaize} mm</td></tr>
  <tr><td><strong>Matière :</strong></td><td>{x.Frontal} {x.Adhesif} {x.Backing}</td></tr>
  <tr><td><strong>Impression :</strong></td><td>{fin}</td></tr>
  <tr><td><strong>Nombre de réf. :</strong></td><td>{d.NbReferences}</td></tr>
</table>

<h2>Tarifs</h2>
<table>
  <tr><th>Quantité</th><th>Prix / 1 000 HT</th><th>Montant HT</th></tr>
  {lignes}
</table>

<p>Validité : {d.Date.AddMonths(1):dd/MM/yyyy} — Règlement : 45 jours fin de mois.</p>
<p>Restant à votre entière disposition.</p>
<p>{d.Commercial}<br>{d.CommercialTel}<br>{d.CommercialEmail}</p>
</body></html>";
    }
}
