using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapGet("/dni/{numero}", async (string numero) =>
{
    if (string.IsNullOrWhiteSpace(numero) || numero.Length != 8)
        return Results.BadRequest(new { error = "Número de DNI inválido" });

    var token = Environment.GetEnvironmentVariable("RENIEC_TOKEN");

    if (string.IsNullOrEmpty(token))
        return Results.Problem("Token en Render");

    var url = $"https://api.decolecta.com/v1/reniec/dni?numero={numero}";

    try
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return Results.Json(new { error = "Error en la consulta", detalle = content }, statusCode: (int)response.StatusCode);

        return Results.Content(content, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = "Error interno", detalle = ex.Message }, statusCode: 500);
    }
});

// Render usa el puerto que define la variable de entorno PORT
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
