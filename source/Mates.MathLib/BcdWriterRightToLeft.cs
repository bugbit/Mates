namespace Mates.MathLib;

// -----------------------------------------------------------------------------
// Propósito:   Escritor de dígitos BCD empaquetados (4 bits por dígito).
//              Al agregar, cada par de dígitos se empaqueta en un byte con el
//              primer dígito en el nibble alto y el segundo en el nibble bajo.
//              Al materializar (ToBcd), los bytes se devuelven en orden inverso
//              (derecha→izquierda) junto con el número total de dígitos.
// Dependencias: System, System.Collections.Generic, System.Linq (Reverse<T>),
//               y el tipo externo `Bcd` (no incluido aquí).
// Riesgos:     - No es thread-safe: instancias compartidas requieren sincronización externa.
//              - Con conteo impar de dígitos, el nibble bajo del último byte se rellena con 0x0.
//              - La semántica exacta de `Bcd` se asume; valide su contrato.
// -----------------------------------------------------------------------------

/// <summary>
/// Construye un valor en BCD empaquetado (4 bits por dígito) a partir de dígitos decimales,
/// almacenando internamente los bytes en el orden de construcción y materializándolos
/// en orden inverso (derecha→izquierda).
/// </summary>
/// <remarks>
/// <para>
/// Política de empaquetado: el primer dígito agregado ocupa el nibble alto (bits 7..4) del byte,
/// y el segundo dígito ocupa el nibble bajo (bits 3..0). Con un número impar de dígitos,
/// el nibble bajo del último byte se rellena con <c>0x0</c>.
/// </para>
/// <para>
/// Seguridad de subprocesos: no es seguro para acceso concurrente. Coordine el acceso si la
/// instancia se usa desde múltiples hilos.
/// </para>
/// <para>
/// Requiere <c>System.Linq</c> debido al uso de <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var w = new BcdWriterRightToLeft();
/// w.Add(1); // 0x10
/// w.Add(2); // 0x12
/// w.Add(3); // 0x12, 0x30 (nibble bajo pendiente)
/// w.Add(4); // 0x12, 0x34
///
/// // ToBcd invierte los bytes (derecha→izquierda) según la convención:
/// Bcd bcd = w.ToBcd(); // bytes devueltos: [0x34, 0x12], dígitos: 4
/// </code>
/// </example>
public class BcdWriterRightToLeft
{
    // Bytes acumulados en orden de construcción (izquierda→derecha).
    private List<byte> _data = new();

    // Índice de nibble dentro del byte en construcción: 1 = alto (siguiente Add inicia byte), 0 = bajo.
    private int _idx4Bit = 1;

    // Índice del byte actual dentro de _data; -1 cuando aún no hay bytes.
    private int _idx = -1;

    // Conteo total de dígitos agregados (0..N).
    private int _digits = 0;

    /// <summary>
    /// Agrega un dígito decimal (0–9) al BCD en construcción.
    /// </summary>
    /// <param name="digit">Dígito decimal en el rango 0–9.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Se produce si <paramref name="digit"/> está fuera del rango 0–9.
    /// </exception>
    /// <remarks>
    /// El primer dígito de cada byte se coloca en el nibble alto (desplazado 4 bits a la izquierda).
    /// El segundo dígito se OR-ea en el nibble bajo del mismo byte.
    /// Con número impar de dígitos, el nibble bajo del último byte queda rellenado con <c>0x0</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var w = new BcdWriterRightToLeft();
    /// w.Add(9); // _data = [0x90]
    /// w.Add(5); // _data = [0x95]
    /// w.Add(3); // _data = [0x95, 0x30] (nibble bajo sin completar aún)
    /// </code>
    /// </example>
    public void Add(int digit)
    {
        // Verifica que el valor del dígito esté dentro del rango 0–9.
        if (digit < 0 || digit > 9)
            throw new ArgumentOutOfRangeException(nameof(digit));

        if (_idx4Bit == 1)
        {
            // Coloca el dígito en el nibble alto de un nuevo byte.
            _data.Add((byte)(digit << 4));
            _idx++;
            _idx4Bit = 0;
        }
        else
        {
            // Completa el nibble bajo del byte actual.
            _data[_idx] |= (byte)digit;
            _idx4Bit = 1;
        }
        _digits++;
    }

    /// <summary>
    /// Materializa el contenido actual como una instancia de <c>Bcd</c>,
    /// invirtiendo el orden de los bytes (derecha→izquierda) y preservando
    /// el conteo total de dígitos.
    /// </summary>
    /// <returns>Una nueva instancia de <c>Bcd</c> con los bytes invertidos y el número de dígitos.</returns>
    /// <remarks>
    /// Si el número de dígitos es impar, el último byte contendrá el nibble bajo con <c>0x0</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var w = new BcdWriterRightToLeft();
    /// w.Add(1);
    /// w.Add(2);
    /// w.Add(3);
    /// w.Add(4);
    /// Bcd bcd = w.ToBcd(); // bytes: [0x34, 0x12], dígitos: 4
    /// </code>
    /// </example>
    public Bcd ToBcd() => new(_data.Reverse<byte>(), _digits);
}
