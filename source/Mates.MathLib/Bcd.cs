namespace Mates.MathLib;

using System.Collections;

public enum BcdOperationFlags { None, NoNewResult, ReturnCarries }

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
public sealed class Bcd
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
        _data = new();
        SetDigits(digits);
    }

    /// <summary>
    /// Constructor de la clase <c>Bcd</c> que inicializa los datos binarios decimales (BCD)
    /// a partir de una secuencia de bytes opcionalmente acompañada por el número total de dígitos.
    /// </summary>
    /// <param name="data">
    /// Colección de bytes que representan los valores BCD.
    /// Cada byte contiene dos dígitos codificados en nibbles (4 bits cada uno).
    /// </param>
    /// <param name="digits">
    /// Número total de dígitos decimales que debe manejar la instancia.  
    /// Si se proporciona, se ajusta internamente mediante <see cref="SetDigits(int)"/>;  
    /// si es <c>null</c>, el número de dígitos se deriva del tamaño de los datos.
    /// </param>
    public Bcd(IEnumerable<byte> data, int? digits = null)
    {
        // Crea una nueva lista a partir de la secuencia de bytes recibida.
        // Esto copia el contenido, evitando dependencias con el enumerable original.
        _data = new(data);

        // Si se especificó la cantidad de dígitos, ajusta la estructura interna
        // para asegurar que la longitud en bytes coincide con el número de dígitos esperado.
        if (digits.HasValue)
            SetDigits(digits.Value);
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
    /// Ajusta el número total de dígitos que debe manejar la estructura interna BCD.
    /// Si el nuevo número de dígitos requiere más espacio (más bytes),
    /// amplía la lista interna <see cref="_data"/> rellenándola con ceros.
    /// </summary>
    /// <param name="digits">
    /// Número total de dígitos que debe representar la instancia BCD.  
    /// Cada byte almacena dos dígitos en formato BCD (uno en el nibble bajo y otro en el alto).
    /// </param>
    /// <remarks>
    /// Este método garantiza que la lista interna de bytes tenga la longitud necesaria
    /// para almacenar el número de dígitos indicado.
    /// <para>
    /// Ejemplo:  
    /// - 1 o 2 dígitos → 1 byte  
    /// - 3 o 4 dígitos → 2 bytes  
    /// - 5 o 6 dígitos → 3 bytes, etc.
    /// </para>
    /// </remarks>
    public void SetDigits(int digits)
    {
        // Divide el número de dígitos entre 2, obteniendo:
        // 'div' → cantidad de bytes completos
        // 'rem' → resto (1 si hay un dígito impar)
        var (div, rem) = Math.DivRem(digits, 2);

        // Calcula el número total de bytes requeridos.
        int lenghtNew = div + rem;

        // Calcula cuántos bytes faltan respecto a la longitud actual.
        int lengthNew2 = lenghtNew - _data.Count;

        // Si el nuevo número de dígitos es mayor que el actual, actualiza el campo interno.
        if (_digits < digits)
            _digits = digits;

        // Si faltan bytes en la lista, se agregan y se inicializan a cero.
        if (lengthNew2 > 0)
            _data.AddRange(Enumerable.Repeat((byte)0, lengthNew2).ToList());
    }

    /// <summary>
    /// Calcula el índice absoluto de un dígito BCD a partir de su posición
    /// dentro del byte de datos y el resto (mitad baja o alta del byte).
    /// </summary>
    /// <param name="idxData">Índice del byte en el array de datos BCD.</param>
    /// <param name="idx4Bit">Posición dentro del byte: 0 = nibble bajo, 1 = nibble alto.</param>
    /// <returns>Índice del dígito dentro del número completo.</returns>
    public static int GetIndex(int idxData, int idx4Bit) => idxData * 2 + idx4Bit;

    /// <summary>
    /// Calcula la cantidad total de dígitos representados hasta una posición determinada
    /// (incluyendo el dígito actual).
    /// </summary>
    /// <param name="idxData">Índice del byte en el array de datos BCD.</param>
    /// <param name="idx4Bit">Posición dentro del byte: 0 = nibble bajo, 1 = nibble alto.</param>
    /// <returns>Total de dígitos contados hasta esa posición (1-based).</returns>
    public static int CalcDigits(int idxData, int idx4Bit) => GetIndex(idxData, idx4Bit) + 1;

    /// <summary>
    /// Intenta calcular los índices internos correspondientes a un dígito BCD,
    /// sin lanzar excepciones en caso de error.
    /// </summary>
    /// <param name="idx">
    /// Índice del dígito a acceder. Puede ser negativo para contar desde el final.
    /// </param>
    /// <returns>
    /// Tupla con la información de índice:  
    /// <c>(ok, idxData, idx4Bit)</c>, donde <c>ok</c> indica si el índice era válido.
    /// </returns>
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
    public (bool ok, int idxData, int idx4Bit) TryGetIdx(int idx) => TryGetIdx(idx, false);

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
        var (_, idxData, idx4Bit) = TryGetIdx(idx, true);

        return (idxData, idx4Bit);
    }

    /// <summary>
    /// Establece un dígito decimal en la posición indicada dentro del número BCD.
    /// </summary>
    /// <param name="idx">
    /// Índice del dígito a modificar (0 para el primer dígito, 1 para el segundo, etc.).  
    /// Si el índice es negativo, se cuenta desde el final (por ejemplo, -1 corresponde al último dígito).
    /// </param>
    /// <param name="digit">
    /// Valor decimal (0–9) del dígito que se quiere establecer.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Se lanza si <paramref name="digit"/> está fuera del rango válido (0–9).
    /// </exception>
    /// <remarks>
    /// Este método modifica el dígito BCD en la posición indicada, ajustando internamente la lista de bytes.
    /// Si el índice apunta más allá del tamaño actual de la lista, ésta se amplía automáticamente con ceros.  
    ///
    /// Cada byte contiene dos dígitos codificados en BCD:
    /// <list type="bullet">
    /// <item>El nibble bajo (bits 0–3) almacena el dígito par.</item>
    /// <item>El nibble alto (bits 4–7) almacena el dígito impar.</item>
    /// </list>
    ///
    /// Ejemplo de estructura:
    /// <code>
    /// _data[0] = 0x12 → dígitos [1,2]
    /// _data[1] = 0x34 → dígitos [3,4]
    /// </code>
    /// </remarks>
    public void SetDigit(int idx, int digit)
    {
        // Verifica que el valor del dígito esté dentro del rango 0–9.
        if (digit < 0 || digit > 9)
            throw new ArgumentOutOfRangeException(nameof(digit));

        // Calcula los índices internos:
        //  idxData = índice del byte en la lista _data
        //  idx4Bit = 0 si es nibble bajo, 1 si es nibble alto
        var (_, idxData, idx4Bit) = TryGetIdx(idx);

        // Obtiene el valor actual del byte donde se almacenará el dígito.
        // Si el índice de byte aún no existe, se asume 0.
        var digitData = idxData < _data.Count ? _data[idxData] : 0;

        // Desplaza el valor del dígito a su posición dentro del byte (nibble bajo o alto).
        var digitDataNew = digit << (idx4Bit * 4);

        // Crea una máscara para eliminar el nibble que se va a reemplazar.
        // ⚠️ Nota: debería usarse 0x0F, no 7, para limpiar 4 bits completos.
        var msk = (byte)(7 << (idx4Bit * 4));

        if (idxData < _data.Count)
        {
            // Reemplaza sólo el nibble correspondiente, dejando intacto el otro.
            _data[idxData] = (byte)((digitData & ~msk) | digitDataNew);

            // Si estamos en el último byte y hemos modificado el nibble alto (idx4Bit == 1),
            // actualizamos el número total de dígitos (_digits).
            if (idxData == _data.Count - 1 && idx4Bit != 0)
                _digits = CalcDigits(idxData, idx4Bit);
        }
        else
        {
            // Si el byte aún no existe, se amplía la lista interna para cubrir esa posición.
            SetDigits(CalcDigits(idxData, idx4Bit));

            // Asigna directamente el nuevo valor del dígito en el byte recién añadido.
            _data[idxData] = (byte)digitDataNew;
        }
    }

    /// <summary>
    /// Obtiene el valor decimal de un dígito almacenado en formato BCD (Binary Coded Decimal)
    /// en la posición indicada.
    /// </summary>
    /// <param name="idx">
    /// Índice del dígito a leer.  
    /// Puede ser positivo (desde el inicio) o negativo (desde el final).  
    /// Por ejemplo, <c>-1</c> obtiene el último dígito.
    /// </param>
    /// <returns>
    /// El valor del dígito (0–9) en la posición solicitada.  
    /// Si el índice no es válido o excede la cantidad de datos disponibles, devuelve <c>0</c>.
    /// </returns>
    /// <remarks>
    /// Cada byte de <see cref="_data"/> contiene dos dígitos codificados en BCD:  
    /// - El nibble bajo (bits 0–3) representa el dígito par.  
    /// - El nibble alto (bits 4–7) representa el dígito impar.
    ///
    /// Ejemplo de almacenamiento:
    /// <code>
    ///   _data[0] = 0x12  → dígitos [2, 1]
    ///   _data[1] = 0x34  → dígitos [4, 3]
    /// </code>
    /// </remarks>
    public int GetDigit(int idx)
    {
        // Intenta obtener la posición interna (byte y nibble)
        // correspondiente al índice solicitado.
        var (ok, idxData, idx4Bit) = TryGetIdx(idx);

        // Si el índice no es válido (fuera de rango negativo, etc.),
        // devuelve 0 por defecto.
        if (!ok)
            return 0;

        // Si el byte calculado está fuera del tamaño de la lista de datos,
        // también se devuelve 0 como valor por defecto.
        if (idxData >= _data.Count)
            return 0;

        // Desplaza el byte para colocar el nibble deseado (0 o 1)
        // en los bits menos significativos.
        var digitData = _data[idxData] >> (idx4Bit * 4);

        // Aplica una máscara (0x0F) para aislar los 4 bits del dígito.
        return digitData & 0x0F;
    }

    public (Bcd? result, BitArray? carries) Sum(Bcd n1, Bcd n2, BcdOperationFlags flags = BcdOperationFlags.None)
    {
        Bcd? result = null;
        BitArray? carries = null;
        int count = Math.Min(n1.Digits, n2.Digits);

        /*
         * 123+456789 = 456912 => 219654
         * 3+9 = 12 digit=2 carry = 1 [ 0x0200 ]
         * 2+9 + 1 = 11  digit=1 carry = 1 [ 0x0201 ]
         * 1+7 + 1 = 9 digit=9 carry = 1 [ 0x0201, 0x0900 ]
         * 0+6 = 6 digit=6 carry = 0 [ 0x0201, 0x0906 ]
         * 0+5 = 5 digit=5 carry = 0 [ 0x0201, 0x0906, 0x0500 ]
         * 0+4 = 4 digit=4 carry = 0 [ 0x0201, 0x0906, 0x0504 ]          
         */
        //List<byte>

        return (result, carries);
    }

    /// <summary>
    /// Intenta calcular los índices internos correspondientes a un dígito BCD dentro de la estructura de datos.
    /// </summary>
    /// <param name="idx">
    /// Índice del dígito a acceder.  
    /// Puede ser positivo (desde el inicio) o negativo (desde el final).  
    /// Por ejemplo, -1 se refiere al último dígito.
    /// </param>
    /// <param name="throwsException">
    /// Indica si se debe lanzar una excepción <see cref="ArgumentOutOfRangeException"/> en caso de índice inválido.  
    /// Si es <c>false</c>, devuelve <c>(false, 0, 0)</c> en su lugar.
    /// </param>
    /// <returns>
    /// Una tupla con tres valores:
    /// <list type="bullet">
    /// <item><description><c>ok</c>: Indica si el índice es válido.</description></item>
    /// <item><description><c>idxData</c>: Índice del byte dentro del array de datos BCD.</description></item>
    /// <item><description><c>idx4Bit</c>: Índice del nibble (0 = bajo, 1 = alto) dentro del byte.</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Cada byte almacena dos dígitos BCD:
    /// <code>
    ///    Índices de dígitos:  0     1     2     3     4     5 ...
    ///    Posición interna:   (0,0) (0,1) (1,0) (1,1) (2,0) (2,1) ...
    /// </code>
    /// De modo que:
    /// - El primer valor de la tupla (<c>idxData</c>) indica el byte.  
    /// - El segundo (<c>idx4Bit</c>) indica si se trata del nibble bajo (0) o alto (1).
    /// </remarks>
    private (bool ok, int idxData, int idx4Bit) TryGetIdx(int idx, bool throwsException)
    {
        bool ok;

        /*
         *    0     1     2     3     4     5     6     7     8     9
         *  (0,1) (0,0) (1,1) (1,0) (2,1) (2,0) (3,1) (3,0) (4,1) (4,0)
         *   -10    -9    -8    -7    -6    -5    -4    -3    -2    -1
         *  (0,1) (0,0) (1,1) (1,0) (2,1) (2,0) (3,1) (3,0) (4,1) (4,0)
         */

        // Si el índice es negativo, se cuenta desde el final.
        if (idx < 0)
            idx = _digits + idx;

        // Si después del ajuste sigue siendo negativo, el índice no es válido.
        if (idx < 0 || idx >= _digits)
        {
            if (throwsException)
                throw new ArgumentOutOfRangeException(nameof(idx));

            ok = false;
        }
        else
            ok = true;

        // Divide el índice entre 2: cada byte tiene 2 dígitos (nibbles).
        var (idxData, idx4Bit) = Math.DivRem(Math.Abs(idx), 2);

        return (ok, idxData, 1 - idx4Bit);
    }
}
