using System.Net;
using System.Text.Json;
using DevisHp.Core; using DevisHp.Data; using DevisHp.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace DevisHp;

public class CalculerDevisFunction {
    private readonly DevisCalculator _calc;
    private static readonly JsonSerializerOptions _json =
        new() { PropertyNameCaseInsensitive = true };

    public CalculerDevisFunction(IReferenceRepository repo)
        => _calc = new DevisCalculator(repo);

    /// <summary>
    /// POST /api/CalculerDevis
    /// Body : DevisInput[]  (1 à 4 éléments, un par quantité)
    /// Retour : DevisResult[]
    /// </summary>
    [Function("CalculerDevis")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req) {
        var body   = await new StreamReader(req.Body).ReadToEndAsync();
        var inputs = JsonSerializer.Deserialize<List<DevisInput>>(body, _json) ?? new();
        var results = inputs.Select(i => _calc.Calculer(i)).ToList();
        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteAsJsonAsync(results);
        return resp;
    }
}
