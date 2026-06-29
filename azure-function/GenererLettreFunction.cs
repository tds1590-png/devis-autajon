using System.Net;
using System.Text.Json;
using DevisHp.Core; using DevisHp.Data; using DevisHp.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace DevisHp;

public record LettreRequest {
    public InfoDevis     Info      { get; init; } = new();
    public DevisInput    Produit   { get; init; } = new();
    public List<DevisInput> Quantites { get; init; } = new();
}

public record LettreResponse {
    public string Html        { get; init; } = "";
    public string EmailClient { get; init; } = "";
    public string Sujet       { get; init; } = "";
}

public class GenererLettreFunction {
    private readonly DevisCalculator _calc;
    private static readonly JsonSerializerOptions _json =
        new() { PropertyNameCaseInsensitive = true };

    public GenererLettreFunction(IReferenceRepository repo)
        => _calc = new DevisCalculator(repo);

    /// <summary>
    /// POST /api/GenererLettre
    /// Body : { info: InfoDevis, produit: DevisInput, quantites: DevisInput[] }
    /// Retour : { html, emailClient, sujet }  — html = corps du mail prêt à l'emploi
    /// </summary>
    [Function("GenererLettre")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req) {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var rq   = JsonSerializer.Deserialize<LettreRequest>(body, _json) ?? new();
        var res  = rq.Quantites.Select(q => _calc.Calculer(q)).ToList();
        var html = LettreHtmlBuilder.Generer(rq.Info, rq.Produit, res);
        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteAsJsonAsync(new LettreResponse {
            Html        = html,
            EmailClient = rq.Info.EmailClient,
            Sujet       = $"Devis {rq.Info.NumDevis} - {rq.Info.Client}"
        });
        return resp;
    }
}
