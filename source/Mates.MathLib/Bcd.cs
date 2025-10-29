namespace Mates.MathLib;

using System.Collections;
using System.Text;

public enum BcdOperationFlags { None, NoNewResult, ReturnCarries }

/// <summary>
/// Representa un número codificado en formato BCD (Binary Coded Decimal).
/// Cada byte contiene dos dígitos decimales, uno en los 4 bits altos y otro en los 4 bits bajos.
/// </summary>
/// <remarks>
/// <para>
/// Convención de almacenamiento: por byte, el nibble bajo (bits 0..3) representa el dígito “par”
/// y el nibble alto (bits 4..7) el dígito “impar”.
/// </para>
/// <para>
/// Ejemplo: si se establecen los dígitos 1,2,3,4 mediante <see cref="SetDigit(int, int)"/>,
/// el array interno podría quedar como [0x21, 0x43].
/// </para>
/// <para>
/// Concurrencia: esta clase no es thread-safe.
/// </para>
/// </remarks>
public sealed class Bcd
{
    /// <summary>Almacena los bytes que representan los dígitos en formato BCD.</summary>
    private List<byte> _data;

    /// <summary>Número total de dígitos decimales definidos.</summary>
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
    /// Inicializa una nueva instancia de la clase <see cref="Bcd"/> con un número de dígitos predefinido.
    /// </summary>
    /// <param name="digits">Número total de dígitos decimales que contendrá el valor BCD.</param>
    /// <remarks>Optimiza la asignación si se conoce de antemano el tamaño.</remarks>
    public Bcd(int digits)
    {
        _data = new();
        SetDigits(digits);
    }

    /// <summary>
    /// Inicializa los datos BCD a partir de una secuencia de bytes y, opcionalmente, del número total de dígitos.
    /// </summary>
    /// <param name="data">Secuencia de bytes BCD (dos dígitos por byte).</param>
    /// <param name="digits">
    /// Número de dígitos esperados; si se especifica, se ajusta internamente con <see cref="SetDigits(int)"/>.
    /// Si es <see langword="null"/>, los dígitos se derivan del tamaño en bytes.
    /// </param>
    public Bcd(IEnumerable<byte> data, int? digits = null)
    {
        _data = new(data);

        if (digits.HasValue)
            SetDigits(digits.Value);
    }

    /// <summary>
    /// Obtiene el número total de dígitos decimales definidos para esta instancia.
    /// </summary>
    public int Digits => _digits;

    /// <summary>
    /// Obtiene una copia del contenido interno del número BCD en formato de array de bytes.
    /// </summary>
    /// <remarks>El array devuelto es una copia: modificarlo no afecta al contenido interno.</remarks>
    public byte[] Data => _data.ToArray();

    /// <summary>
    /// Devuelve la representación decimal del valor BCD, concatenando todos los dígitos.
    /// </summary>
    /// <remarks>
    /// Recorre <see cref="AsEnumerable"/> y convierte cada dígito (0–9) a su carácter ASCII
    /// sumando <c>'0'</c>. Complejidad O(n) en el número de dígitos.
    /// </remarks>
    /// <returns>
    /// Cadena con los dígitos en orden de mayor a menor peso (MSD→LSD). Si no hay dígitos,
    /// devuelve <see cref="string.Empty"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// // Con _data = [0x12, 0x34] y _digits = 4  => "1234"
    /// var s = bcd.ToString();
    /// </code>
    /// </example>
    public override string ToString()
    {
        // Micro-optimización: reservar capacidad según _digits.
        var sb = new StringBuilder(_digits);

        foreach (var digit in AsEnumerable())
            sb.Append((char)(digit + '0'));

        return sb.ToString();
    }

    /// <summary>
    /// Enumera los dígitos decimales del valor BCD del más significativo al menos significativo.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Cada byte de <c>_data</c> contiene dos dígitos en BCD empaquetado: nibble alto (bits 7..4)
    /// seguido del nibble bajo (bits 3..0). Este enumerador emite primero el nibble alto y después
    /// el nibble bajo de cada byte, y se detiene al alcanzar <c>_digits</c>, ignorando cualquier
    /// nibble de relleno (p. ej., cuando el total de dígitos es impar).
    /// </para>
    /// <para>
    /// No materializa una colección intermedia: usa <c>yield return</c>, por lo que es eficiente
    /// en memoria. Complejidad O(n) y diferida.
    /// </para>
    /// </remarks>
    /// <returns>
    /// Secuencia de bytes, cada uno en el rango 0–9, representando los dígitos del número.
    /// </returns>
    /// <example>
    /// <code>
    /// // _data = [0x12, 0x34], _digits = 3  => produce: 1, 2, 3 (el 4 es ignorado por límite)
    /// foreach (var d in bcd.AsEnumerable()) Console.Write(d); // "123"
    /// </code>
    /// </example>
    public IEnumerable<byte> AsEnumerable()
    {
        int ndigit = 0;

        foreach (var item in _data)
        {
            // Si ya alcanzamos el total lógico de dígitos, finalizamos la enumeración.
            if (ndigit++ >= _digits)
                yield break;

            // Emite el dígito del nibble alto: (0x12 & 0xF0) >> 4 => 1
            yield return (byte)((item & 0xF0) >> 4);

            // Comprobación temprana para no emitir nibbles de relleno en cuenta impar.
            if (ndigit++ >= _digits)
                yield break;

            // Emite el dígito del nibble bajo: (0x12 & 0x0F) => 2
            yield return (byte)(item & 0x0F);
        }
    }

    /// <summary>
    /// Ajusta el número total de dígitos que debe manejar la estructura interna BCD,
    /// ampliando <see cref="_data"/> con ceros si es necesario.
    /// </summary>
    /// <param name="digits">Número total de dígitos a representar.</param>
    /// <remarks>
    /// 1–2 dígitos → 1 byte; 3–4 → 2 bytes; 5–6 → 3 bytes; etc.
    /// </remarks>
    public void SetDigits(int digits)
    {
        var (div, rem) = Math.DivRem(digits, 2);
        int lengthNew = div + rem;
        int missing = lengthNew - _data.Count;

        if (_digits < digits)
            _digits = digits;

        if (missing > 0)
            _data.AddRange(Enumerable.Repeat((byte)0, missing).ToList());
    }

    /// <summary>
    /// Calcula el índice absoluto de un dígito BCD a partir de su posición de byte y nibble.
    /// </summary>
    /// <param name="idxData">Índice del byte en el array interno.</param>
    /// <param name="idx4Bit">Posición dentro del byte: 0 = nibble bajo, 1 = nibble alto.</param>
    /// <returns>Índice de dígito (base 0).</returns>
    public static int GetIndex(int idxData, int idx4Bit) => idxData * 2 + (1 - idx4Bit);

    /// <summary>
    /// Calcula la cantidad total de dígitos representados hasta una posición determinada (incluido el actual).
    /// </summary>
    public static int CalcDigits(int idxData, int idx4Bit) => GetIndex(idxData, idx4Bit) + 1;

    /// <summary>
    /// Intenta calcular los índices internos correspondientes a un dígito BCD,
    /// sin lanzar excepciones en caso de error.
    /// </summary>
    /// <param name="idx">Índice del dígito (admite negativos para contar desde el final).</param>
    /// <returns>Tupla <c>(ok, idxData, idx4Bit)</c>.</returns>
    public (bool ok, int idxData, int idx4Bit) TryGetIdx(int idx) => TryGetIdx(idx, false);

    /// <summary>
    /// Calcula la posición (byte y nibble) de un dígito en el almacenamiento interno BCD.
    /// </summary>
    /// <param name="idx">Índice lógico del dígito (admite negativos).</param>
    /// <returns>(idxData, idx4Bit)</returns>
    /// <exception cref="ArgumentOutOfRangeException">Si el índice resultante no es válido.</exception>
    public (int idxData, int idx4Bit) GetIdx(int idx)
    {
        var (_, idxData, idx4Bit) = TryGetIdx(idx, true);
        return (idxData, idx4Bit);
    }

    /// <summary>
    /// Establece un dígito decimal en la posición indicada dentro del número BCD.
    /// Amplía el almacenamiento si es necesario.
    /// </summary>
    /// <param name="idx">Índice del dígito (0-based; negativos desde el final).</param>
    /// <param name="digit">Dígito decimal (0–9).</param>
    /// <exception cref="ArgumentOutOfRangeException">Si <paramref name="digit"/> está fuera de 0–9.</exception>
    public void SetDigit(int idx, int digit)
    {
        if (digit < 0 || digit > 9)
            throw new ArgumentOutOfRangeException(nameof(digit));

        var (_, idxData, idx4Bit) = TryGetIdx(idx);
        var digitData = idxData < _data.Count ? _data[idxData] : 0;
        var digitDataNew = digit << (idx4Bit * 4);

        var msk = (byte)(0xF << (idx4Bit * 4));

        if (idxData < _data.Count)
        {
            _data[idxData] = (byte)((digitData & ~msk) | digitDataNew);

            if (idxData == _data.Count - 1 && idx4Bit != 0)
                _digits = CalcDigits(idxData, idx4Bit);
        }
        else
        {
            SetDigits(CalcDigits(idxData, idx4Bit));
            _data[idxData] = (byte)digitDataNew;
        }
    }

    /// <summary>
    /// Obtiene el valor decimal (0–9) de un dígito almacenado en la posición indicada.
    /// </summary>
    /// <param name="idx">Índice del dígito (admite negativos).</param>
    /// <returns>El dígito 0–9; devuelve 0 si el índice no es válido o excede los datos disponibles.</returns>
    public int GetDigit(int idx)
    {
        var (ok, idxData, idx4Bit) = TryGetIdx(idx);
        if (!ok || idxData >= _data.Count)
            return 0;

        var digitData = _data[idxData] >> (idx4Bit * 4);
        return digitData & 0x0F;
    }

    /// <summary>
    /// Analiza una cadena numérica decimal y construye su representación en BCD.
    /// </summary>
    /// <param name="value">Cadena con dígitos decimales.</param>
    /// <returns>Instancia <see cref="Bcd"/> equivalente.</returns>
    /// <exception cref="FormatException">Si <paramref name="value"/> contiene caracteres no numéricos.</exception>
    public static Bcd Parse(string value) => TryParse(value, true).bcd!;

    public static (bool ok, Bcd? bcd) TryParse(string value) => TryParse(value, false);

    /// <summary>
    /// Copia los datos y metadatos (dígitos) de otra instancia <see cref="Bcd"/>.
    /// </summary>
    /// <param name="bcd">Origen a copiar.</param>
    public void SetData(Bcd bcd)
    {
        // Implementación actual basada en método de extensión CopyFrom(List<byte>).
        _data.CopyFrom(bcd._data);
        _digits = bcd._digits;
    }

    /// <summary>
    /// Suma dos valores BCD dígito a dígito (de derecha a izquierda) con acarreo.
    /// </summary>
    /// <param name="n1">Sumando 1.</param>
    /// <param name="n2">Sumando 2.</param>
    /// <param name="flags">
    /// <see cref="BcdOperationFlags.NoNewResult"/> para escribir en <paramref name="n1"/> en lugar de devolver uno nuevo;  
    /// <see cref="BcdOperationFlags.ReturnCarries"/> para solicitar mapa de acarreos.
    /// </param>
    /// <returns>
    /// Tupla con:  
    /// - <c>result</c>: <see cref="Bcd"/> con la suma (o <see langword="null"/> si se usa <c>NoNewResult</c>),  
    /// - <c>carries</c>: <see cref="BitArray"/> con acarreos por posición (o <see langword="null"/>).
    /// </returns>
    /// <remarks>
    /// Recorre ambos operandos desde el último dígito (índices negativos) y acumula en un <see cref="BcdWriter"/>.
    /// Finalmente materializa con <see cref="BcdWriter.ToBcdReverse"/> para restituir el orden correcto.
    /// </remarks>
    public static (Bcd? result, BitArray? carries) Sum(Bcd n1, Bcd n2, BcdOperationFlags flags = BcdOperationFlags.None)
    {
        Bcd? result = null;
        BitArray? carries = null;
        int count = Math.Max(n1.Digits, n2.Digits);

        using var bcdWriter = new BcdWriter();
        int carry = 0;

        for (int i = 1; i <= count || carry > 0; i++)
        {
            var digit1 = n1.GetDigit(-i);
            var digit2 = n2.GetDigit(-i);
            var resultDigit = digit1 + digit2 + carry;

            if (resultDigit > 9)
            {
                carry = 1;
                resultDigit -= 10;
            }
            else
                carry = 0;

            if (carry != 0)
            {
                if (flags.HasFlag(BcdOperationFlags.ReturnCarries))
                {
                    carries ??= new(count);
                    carries.Set(i - 1, true);
                }
            }
            bcdWriter.Add(resultDigit);
        }

        var bcd = bcdWriter.ToBcdReverse();

        if (flags.HasFlag(BcdOperationFlags.NoNewResult))
            n1.SetData(bcd);
        else
            result = bcd;

        return (result, carries);
    }

    /// <summary>
    /// Versión interna de <see cref="TryGetIdx(int)"/> con opción de lanzar excepción.
    /// </summary>
    /// <param name="idx">Índice de dígito (admite negativos).</param>
    /// <param name="throwsException">
    /// <see langword="true"/> para lanzar <see cref="ArgumentOutOfRangeException"/> si no es válido;
    /// en caso contrario devuelve <c>(false, 0, 0)</c>.
    /// </param>
    private (bool ok, int idxData, int idx4Bit) TryGetIdx(int idx, bool throwsException)
    {
        bool ok;

        // Ajuste de índices negativos: se cuentan desde el final.
        if (idx < 0)
            idx = _digits + idx;

        // Validación de rango (nota: permite == _digits; coherencia con consumos del método).
        if (idx < 0 || idx > _digits)
        {
            if (throwsException)
                throw new ArgumentOutOfRangeException(nameof(idx));

            ok = false;
        }
        else
        {
            ok = true;
        }

        // Cada byte contiene dos dígitos.
        var (idxData, idx4Bit) = Math.DivRem(Math.Abs(idx), 2);

        // Convención interna: 0 = nibble bajo, 1 = nibble alto.
        return (ok, idxData, 1 - idx4Bit);
    }

    private static (bool ok, Bcd? bcd) TryParse(string value, bool throwsException)
    {
        using var bcdWriter = new BcdWriter();

        foreach (var c in value)
        {
            if (!char.IsDigit(c))
            {
                if (throwsException)
                    throw new FormatException();

                return (false, null);
            }

            bcdWriter.Add(c - '0');
        }

        return (true, bcdWriter.ToBcd());
    }
}
