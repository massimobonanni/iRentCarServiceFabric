using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Classe che contiene i metodi di estensione che agiscono su istanze di <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        private const ulong FnvPrime = 1099511628211;
        private const ulong FnvOffsetBasis = 14695981039346656037;

        /// <summary>
        /// Calcola l'hash code per una stringa.
        /// </summary>
        /// <param name="value">String di cui calcolare l'hash.</param>
        /// <returns>System.Int64.</returns>
        public static long GetExtendedHash(this string value)
        {
            return Encoding.UTF32.GetBytes(value).GetExtendedHash();
        }

        /// <summary>
        /// Calcola l'hash code per un array di <see cref="byte"/>.
        /// </summary>
        /// <param name="value">Array di byte di cui calcolare l'hash.</param>
        /// <returns>System.Int64.</returns>
        public static long GetExtendedHash(this byte[] value)
        {
            if (value == null)
                throw new NullReferenceException(nameof(value));

            ulong hash = FnvOffsetBasis;
            for (int i = 0; i < value.Length; ++i)
            {
                hash ^= value[i];
                hash *= FnvPrime;
            }

            return (long)hash;
        }
    }
}
