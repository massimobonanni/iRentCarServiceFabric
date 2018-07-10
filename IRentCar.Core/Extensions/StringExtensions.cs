﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public static bool IsValidEmail(this string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

            // Return true if strIn is in valid email format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                return null;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}
