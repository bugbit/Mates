namespace Mates.Math;

using Math = System.Math;

/// <summary>
/// Representa un número codificado en formato BCD (Binary Coded Decimal).
/// Cada byte contiene dos dígitos decimales, uno en los 4 bits altos y otro en los 4 bits bajos.
/// </summary>
/// <remarks>
/// Ejemplo:
/// <para>
/// Si se agregan los dígitos 1, 2, 3, 4 mediante <see cref="SetDigit"/>,
/// internamente el array <see cref="Data"/> contendrá los bytes [0x21, 0x43],
/// donde cada nibble representa un dígito decimal.
/// </para>
/// </remarks>
public class Bcd
{
    /// <summary>
    /// Almacena los bytes que representan los dígitos en formato BCD.
    /// Cada byte puede contener hasta dos dígitos.
    /// </summary>
    private List<byte> _data;
    private int _digits;

    /// <summary>
    /// Inicializa una nueva instancia vacía de la clase <see cref="Bcd"/>.
    /// </summary>
    public Bcd()
    {
        _data = new();
        _digits = 0;
    }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="Bcd"/> con un tamaño predefinido.
    /// </summary>
    /// <param name="digits">
    /// Número total de dígitos decimales que contendrá el valor BCD.
    /// Cada byte puede almacenar dos dígitos, por lo que internamente se reserva <c>digits / 2</c> bytes.
    /// </param>
    /// <remarks>
    /// Este constructor permite optimizar la asignación de memoria cuando se conoce
    /// de antemano la cantidad total de dígitos que se van a utilizar.
    /// </remarks>
    public Bcd(int digits)
    {
        _data = new List<byte>((digits / 2) + 1);
        _digits = digits;
    }

    /// <summary>
    /// Obtiene el número total de dígitos decimales definidos para esta instancia de <see cref="Bcd"/>.
    /// </summary>
    /// <remarks>
    /// Este valor se establece en el constructor y puede servir para validar
    /// operaciones que dependan del tamaño del número BCD.
    /// </remarks>
    public int Digits => _digits;

    /// <summary>
    /// Obtiene una copia del contenido interno del número BCD en formato de array de bytes.
    /// </summary>
    /// <remarks>
    /// El array devuelto es una copia: modificarlo no afecta al contenido interno del objeto.
    /// </remarks>
    public byte[] Data => _data.ToArray();

    /// <summary>
    /// Calcula la posición del dígito dentro del almacenamiento interno BCD,
    /// devolviendo tanto el índice de byte como el índice del nibble (4 bits).
    /// </summary>
    /// <param name="idx">
    /// Índice lógico del dígito a obtener.  
    /// Puede ser positivo (desde el inicio) o negativo (desde el final).
    /// </param>
    /// <returns>
    /// Una tupla con:
    /// <list type="bullet">
    /// <item><description><c>idxData</c>: índice del byte que contiene el dígito.</description></item>
    /// <item><description><c>idx4Bit</c>: indica si el dígito está en el nibble bajo (0) o alto (1).</description></item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Se lanza si el índice resultante es menor que cero tras el ajuste de índices negativos.
    /// </exception>
    /// <remarks>
    /// Este método permite convertir un índice de dígito (por ejemplo, el 5.º dígito) en su
    /// posición real dentro de la lista de bytes que componen el número BCD.
    ///
    /// Cada byte almacena dos dígitos:
    /// <code>
    ///  índice:   0     1     2     3     4     5     6     7     8     9
    ///  posición: (0,0) (0,1) (1,0) (1,1) (2,0) (2,1) (3,0) (3,1) (4,0) (4,1)
    /// </code>
    /// Donde la primera coordenada es el byte (<c>idxData</c>) y la segunda el nibble (<c>idx4Bit</c>).
    ///
    /// Los índices negativos se interpretan desde el final del número:
    /// <code>
    ///  índice:   -10   -9    -8    -7    -6    -5    -4    -3    -2    -1
    ///  posición: (0,0) (0,1) (1,0) (1,1) (2,0) (2,1) (3,0) (3,1) (4,0) (4,1)
    /// </code>
    /// </remarks>
    public (int idxData, int idx4Bit) GetIdx(int idx)
    {
        /*
         *    0     1     2     3     4     5     6     7     8     9
         *  (0,0) (0,1) (1,0) (1,1) (2,0) (2,1) (3,0) (3,1) (4,0) (4,1)
         *   -10    -9    -8    -7    -6    -5    -4    -3    -2    -1
         *  (0,0) (0,1) (1,0) (1,1) (2,0) (2,1) (3,0) (3,1) (4,0) (4,1)
         */

        if (idx < 0)
            idx = _data.Count + idx;

        if (idx < 0)
            throw new ArgumentOutOfRangeException(nameof(idx));

        var (idxData, idxRem) = Math.DivRem(Math.Abs(idx), 2);

        return (idxData, idxRem);
    }

    /// <summary>
    /// Establece un dígito decimal en la posición indicada dentro del número BCD.
    /// </summary>
    /// <param name="idx">
    /// Índice del dígito a modificar (0 para el primer dígito, 1 para el segundo, etc.).
    /// Si es negativo, se cuenta desde el final.
    /// </param>
    /// <param name="digit">
    /// Valor decimal (0–9) del dígito a establecer.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Se produce si <paramref name="digit"/> es menor que 0 o mayor que 9.
    /// </exception>
    /// <remarks>
    /// Si la posición indicada está fuera del tamaño actual, se amplía automáticamente la lista interna.
    /// Cada byte almacena dos dígitos (nibbles): el inferior ocupa los bits 0–3, el superior los bits 4–7.
    /// </remarks>
    public void SetDigit(int idx, int digit)
    {
        if (digit < 0 || digit > 9)
            throw new ArgumentOutOfRangeException(nameof(digit));

        var (idxData, idxRem) = Math.DivRem(idx, 2);

        var digitData = idxData < _data.Count ? _data[idxData] : 0;
        var digitDataNew = digit << (idxRem * 4);
        var msk = (byte)(7 << (idxRem * 4));

        if (idxData < _data.Count)
            _data[idxData] = (byte)((digitData & ~msk) | digitDataNew);
        else
            _data.Add((byte)digitDataNew);
    }
}
