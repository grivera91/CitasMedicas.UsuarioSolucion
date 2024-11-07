using CitasMedicas.UsuarioApi.DTO;
using System.Text;
using System.Text.Json;

namespace CitasMedicas.UsuarioApi.Services
{
    public class PacienteService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PacienteService> _logger;

        public PacienteService(IHttpClientFactory clientFactory, ILogger<PacienteService> logger)
        {
            _httpClient = clientFactory.CreateClient("PacienteApi");
            _logger = logger;
        }

        public async Task<PacienteCreateResponseDto?> RegistrarPacienteAsync(PacienteCreateRequestDto pacienteCreateRequestDto)
        {
            try
            {
                StringContent? content = new StringContent(
                    JsonSerializer.Serialize(pacienteCreateRequestDto),
                    Encoding.UTF8,
                    "application/json");

                HttpResponseMessage? response = await _httpClient.PostAsync("", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<PacienteCreateResponseDto>(responseContent);
                }
                else
                {
                    _logger.LogError("Error al registrar paciente: {0}", response.ReasonPhrase);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Excepción al registrar paciente: {0}", ex.Message);
                return null;
            }
        }
    }
}
