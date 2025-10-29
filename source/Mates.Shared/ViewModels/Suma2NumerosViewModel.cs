//using Microsoft.Asp;

using System.ComponentModel.DataAnnotations;

namespace Mates.Shared.ViewModels;

public class Suma2NumerosViewModel
{
    [Required] public string? Numero1 { get; set; }
    [Required] public string? Numero2 { get; set; }
}
