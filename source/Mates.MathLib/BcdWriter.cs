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
public class BcdWriter : IDisposable
{
    // Bytes acumulados en orden de construcción (izquierda→derecha).
    private List<byte> _data = new();

    // Índice de nibble dentro del byte en construcción: 1 = alto (siguiente Add inicia byte), 0 = bajo.
    private int _idx4Bit = 1;

    // Índice del byte actual dentro de _data; -1 cuando aún no hay bytes.
    private int _idx = -1;

    // Conteo total de dígitos agregados (0..N).
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
    /// El segundo dígito se OR-ea en el nibble bajo del mismo byte.
    /// Con número impar de dígitos, el nibble bajo del último byte queda rellenado con <c>0x0</c>.
    /// 123+456789 = 456912 => 219654
    /// 3+9 = 12 digit=2 carry = 1 [0x0200]
    /// 2+9 + 1 = 11  digit=1 carry = 1 [0x0201]
    /// 1+7 + 1 = 9 digit=9 carry = 1 [0x0201, 0x0900]
    /// 0+6 = 6 digit=6 carry = 0 [0x0201, 0x0906]
    /// 0+5 = 5 digit=5 carry = 0 [0x0201, 0x0906, 0x0500]
    /// 0+4 = 4 digit=4 carry = 0 [0x0201, 0x0906, 0x0504]
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

    public Bcd ToBcdReverse()
    {
        /*
            Ejemplo:
                . "456912" si par [0x21, 0x96, 0x54]. Bcd : [ 0x45, 0x69, 0x12 ]
                . "45691" si impar [0x19, 0x65, 0x40]. Bcd : [ 0x45, 0x69, 0x10 ]
         */

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
            /*
             * "45691"
                dataRead = [0x19, 0x65, 0x40]
                dataNew=[ 0x4 ]
                dataNew=[ 0x45 ]
                dataNew=[ 0x45, 0x6 ]
                dataNew=[ 0x45, 0x69 ]
                dataNew=[ 0x45, 0x69, 0x10 ]
             */
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

    public Bcd ToBcd() => new(_data, _digits);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: eliminar el estado administrado (objetos administrados)
                _data.Clear();
            }

            // TODO: liberar los recursos no administrados (objetos no administrados) y reemplazar el finalizador
            // TODO: establecer los campos grandes como NULL
            _data = null;

            disposedValue = true;
        }
    }

    // // TODO: reemplazar el finalizador solo si "Dispose(bool disposing)" tiene código para liberar los recursos no administrados
    // ~BcdWriterRightToLeft()
    // {
    //     // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // No cambie este código. Coloque el código de limpieza en el método "Dispose(bool disposing)".
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
