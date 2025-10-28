using System;

namespace System.Collections.Generic;

public static class ListExtensions
{
    /// <summary>
    /// Copia los elementos de <paramref name="source"/> en la lista <paramref name="destination"/>
    /// reutilizando la memoria interna siempre que sea posible.
    /// </summary>
    public static void CopyFrom<T>(this List<T> destination, IReadOnlyList<T> source)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        int count = source.Count;

        // Aumentar capacidad si es necesario (una sola vez)
        if (destination.Capacity < count)
            destination.Capacity = count;

        // Reescribir los elementos existentes
        int minCount = Math.Min(destination.Count, count);
        for (int i = 0; i < minCount; i++)
            destination[i] = source[i];

        // Si hay más elementos en el source, añadimos los nuevos
        if (count > destination.Count)
        {
            for (int i = destination.Count; i < count; i++)
                destination.Add(source[i]);
        }
        // Si hay menos, recortamos
        else if (count < destination.Count)
        {
            destination.RemoveRange(count, destination.Count - count);
        }
    }
}
