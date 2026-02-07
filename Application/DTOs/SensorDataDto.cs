using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record SensorDataDto(
    [Required(ErrorMessage = "O ID do talhão é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "O ID do talhão deve ser maior que zero")]
    int FieldId,

    [Required(ErrorMessage = "O tipo do sensor é obrigatório")]
    [MaxLength(100, ErrorMessage = "O tipo do sensor não pode exceder 100 caracteres")]
    string SensorType,

    [Required(ErrorMessage = "O valor é obrigatório")]
    double Value,

    [Required(ErrorMessage = "O timestamp é obrigatório")]
    DateTime Timestamp
)
{
    public void Validate()
    {
        if (Timestamp > DateTime.UtcNow.AddMinutes(5))
        {
            throw new ValidationException("O timestamp não pode estar no futuro");
        }
    }
}
