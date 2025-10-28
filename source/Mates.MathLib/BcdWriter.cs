namespace Mates.MathLib;

// -----------------------------------------------------------------------------
// Propósito:   Escritor de dígitos BCD empaquetados (4 bits por dígito).
//              Al agregar, cada par de dígitos se empaqueta en un byte con el
//              primer dígito en el nibble alto y el segundo en el nibble bajo.
//              Al materializar (ToBcd / ToBcdReverse), se puede devolver el
//              buffer en el mismo orden de construcción o en orden inverso
//              (derecha→izquierda), junto con el número total de dígitos.
// Dependencias: System, System.Collections.Generic, System.Linq (Reverse<T>),
//               y el tipo externo `Bcd`.
// Concurrencia: No es thread-safe; sincroniza si compartes instancia.
// Errores:     Con número impar de dígitos, el nibble bajo del último byte
//              queda a 0x0 (relleno).
// -----------------------------------------------------------------------------

/// <summary>
/// Construye un valor en BCD empaquetado (4 bits por dígito) a partir de dígitos decimales,
/// almacenando internamente los bytes en el orden de construcción y permitiendo materializarlos
/// tal cual (<see cref="ToBcd"/>) o en orden inverso (<see cref="ToBcdReverse"/>).
/// </summary>
/// <remarks>
/// <para>
/// Política de empaquetado: el primer dígito agregado ocupa el nibble alto (bits 7..4) del byte,
/// y el segundo dígito ocupa el nibble bajo (bits 3..0). Con un número impar de dígitos,
/// el nibble bajo del último byte se rellena con <c>0x0</c>.
/// </para>
/// <para>
/// Seguridad de subprocesos: no es seguro para acceso concurrente. Coordina el acceso si la
/// instancia se usa desde múltiples hilos.
/// </para>
/// <para>
/// Requiere <c>System.Linq</c> por el uso de <see cref="Enumerable.Reverse{TSource}(System.Collections.Generic.IEnumerable{TSource})"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var w = new BcdWriter();
/// w.Add(1); // 0x10
/// w.Add(2); // 0x12
/// w.Add(3); // 0x12, 0x30 (nibble bajo aún pendiente)
/// w.Add(4); // 0x12, 0x34
///
/// // Materializa en el mismo orden:
/// Bcd bcd = w.ToBcd();           // [0x12, 0x34], dígitos: 4
///
/// // O en orden inverso derecha→izquierda:
/// Bcd bcdRev = w.ToBcdReverse(); // [0x34, 0x12], dígitos: 4
/// </code>
/// </example>
public class BcdWriter : IDisposable
{
    /// <summary>
    /// Bytes acumulados en orden de construcción (izquierda→derecha).
    /// </summary>
    private List<byte> _data = new();

    /// <summary>
    /// Índice de nibble dentro del byte en construcción: 1 = alto (el siguiente <see cref="Add"/> inicia byte), 0 = bajo.
    /// </summary>
    private int _idx4Bit = 1;

    /// <summary>
    /// Índice del byte actual dentro de <see cref="_data"/>; -1 cuando aún no hay bytes.
    /// </summary>
    private int _idx = -1;

    /// <summary>
    /// Conteo total de dígitos agregados (0..N).
    /// </summary>
    private int _digits = 0;

    private bool disposedValue;

    /// <summary>
    /// Agrega un dígito decimal (0–9) al BCD en construcción.
    /// </summary>
    /// <param name="digit">Dígito decimal en el rango 0–9.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Se produce si <paramref name="digit"/> está fuera del rango 0–9.
    /// </exception>
    /// <remarks>
    /// El primer dígito de cada byte se coloca en el nibble alto (desplazado 4 bits a la izquierda).
    /// El segundo dígito se inserta en el nibble bajo del mismo byte (OR).
    /// Con número impar de dígitos, el nibble bajo del último byte queda <c>0x0</c>.
    /// </remarks>
    public void Add(int digit)
    {
        if (digit < 0 || digit > 9)
            throw new ArgumentOutOfRangeException(nameof(digit));

        if (_idx4Bit == 1)
        {
            _data.Add((byte)(digit << 4));
            _idx++;
            _idx4Bit = 0;
        }
        else
        {
            _data[_idx] |= (byte)digit;
            _idx4Bit = 1;
        }
        _digits++;
    }

    /// <summary>
    /// Materializa el contenido BCD invirtiendo el orden de los bytes (derecha→izquierda).
    /// </summary>
    /// <returns>Instancia <see cref="Bcd"/> con los bytes ya invertidos y el total de dígitos.</returns>
    /// <remarks>
    /// Para número de dígitos par, se invierten los nibbles en bloque (alto↔bajo) de cada byte al retroceder.
    /// Para impar, se traslada el primer nibble alto del último byte al primer nibble alto del resultado y
    /// se continúa ensamblando nibble a nibble.
    /// </remarks>
    /// <example>
    /// <code>
    /// // "456912" => datos internos [0x21, 0x96, 0x54] → resultado [0x45, 0x69, 0x12]
    /// var bcd = writer.ToBcdReverse();
    /// </code>
    /// </example>
    public Bcd ToBcdReverse()
    {
        var dataNew = new byte[_data.Count];
        var dataRead = _data.AsReadOnly<byte>();

        if ((_digits % 2) == 0)
            Par();
        else
            Impar();

        return new(dataNew, _digits);

        void Par()
        {
            for (int i = 0, j = dataRead.Count - 1; j >= 0; i++, j--)
            {
                var byteRead = dataRead[j];
                // 0x19 => 0x91
                dataNew[i] = (byte)((byteRead & 0x0F) << 4 | (byteRead & 0xF0) >> 4);
            }
        }

        void Impar()
        {
            for (int i = 0, j = dataRead.Count - 1, idx4Bit = 1; j >= 0;)
            {
                var byteRead = dataRead[j];

                if (idx4Bit == 1)
                {
                    dataNew[i] = (byte)(byteRead & 0xF0);
                    idx4Bit = 0;
                    j--;
                }
                else
                {
                    dataNew[i] |= (byte)(byteRead & 0x0F);
                    idx4Bit = 1;
                    i++;
                }
            }
        }
    }

    /// <summary>
    /// Materializa el contenido BCD en el mismo orden en el que se fue construyendo.
    /// </summary>
    /// <returns>Instancia <see cref="Bcd"/> con el buffer tal cual y el total de dígitos.</returns>
    public Bcd ToBcd() => new(_data, _digits);

    /// <summary>
    /// Libera recursos administrados (limpia el buffer interno) y marca la instancia como descartada.
    /// </summary>
    /// <param name="disposing">Si es <see langword="true"/>, se liberan recursos administrados.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _data.Clear();
            }
            _data = null;
            disposedValue = true;
        }
    }

    /// <summary>
    /// Implementación del patrón <see cref="IDisposable"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
