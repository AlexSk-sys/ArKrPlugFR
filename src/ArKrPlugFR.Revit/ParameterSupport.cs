using System;
using System.Collections.Generic;
using ArKrPlugFR.Core.Parameters;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Revit
{
    /// <summary>
    /// Reads and writes element parameters the way the transfer tools need it.
    /// <para>
    /// Every lookup follows the same order: the instance parameter first, the parameter of
    /// the element's type second. Only a text parameter receives a value, because the
    /// transfer carries text; a value of any other kind is read as the text the interface
    /// shows for it.
    /// </para>
    /// </summary>
    public static class ParameterSupport
    {
        /// <summary>Returns the type of an element, or <c>null</c> when it has none.</summary>
        /// <param name="element">The element to read.</param>
        public static ElementType? TypeOf(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return element.Document.GetElement(element.GetTypeId()) as ElementType;
        }

        /// <summary>Finds a parameter by name: the instance one first, the type one second.</summary>
        /// <param name="element">The element to read.</param>
        /// <param name="name">Name of the parameter.</param>
        public static Parameter? Find(Element element, string name)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (!ParameterNameRules.IsUsable(name))
            {
                return null;
            }

            string trimmed = name.Trim();
            Parameter? parameter = element.LookupParameter(trimmed);

            if (parameter != null)
            {
                return parameter;
            }

            return TypeOf(element)?.LookupParameter(trimmed);
        }

        /// <summary>Reads the value of a parameter as text, or <c>null</c> when there is none.</summary>
        /// <param name="element">The element to read.</param>
        /// <param name="name">Name of the parameter.</param>
        public static string? Read(Element element, string name)
        {
            Parameter? parameter = Find(element, name);

            if (parameter == null || !parameter.HasValue)
            {
                return null;
            }

            string? text = parameter.AsString();

            if (text == null)
            {
                text = parameter.AsValueString();
            }

            return TransferValue.Normalize(text);
        }

        /// <summary>
        /// Writes a text value into a parameter; the instance parameter is written first,
        /// the type parameter second, so a read-only instance parameter does not block a
        /// writable type parameter of the same name.
        /// </summary>
        /// <param name="element">The element to write to.</param>
        /// <param name="name">Name of the parameter.</param>
        /// <param name="value">Value to write.</param>
        /// <returns><c>true</c> when the value was written.</returns>
        public static bool TryWrite(Element element, string name, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            Parameter? target = FindWriteTarget(element, name);

            return target != null && target.Set(value);
        }

        /// <summary>
        /// Determines whether a text value could be written into a parameter: the instance
        /// parameter first, the type parameter second.
        /// </summary>
        /// <param name="element">The element to write to.</param>
        /// <param name="name">Name of the parameter.</param>
        public static bool CanWrite(Element element, string name)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return FindWriteTarget(element, name) != null;
        }

        private static Parameter? FindWriteTarget(Element element, string name)
        {
            if (!ParameterNameRules.IsUsable(name))
            {
                return null;
            }

            string trimmed = name.Trim();
            Parameter? instance = element.LookupParameter(trimmed);

            if (IsWritableText(instance))
            {
                return instance;
            }

            Parameter? type = TypeOf(element)?.LookupParameter(trimmed);

            return IsWritableText(type) ? type : null;
        }

        private static bool IsWritableText(Parameter? parameter)
        {
            return parameter != null && !parameter.IsReadOnly && parameter.StorageType == StorageType.String;
        }

        /// <summary>Lists the names of every parameter of an element, instance first, without repeats.</summary>
        /// <param name="element">The element to read.</param>
        public static IReadOnlyList<string> ReadableNames(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var names = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Collect(element, names, seen, writableOnly: false);
            Collect(TypeOf(element), names, seen, writableOnly: false);

            return names;
        }

        /// <summary>Reads the value of every parameter of an element, keyed by name.</summary>
        /// <param name="element">The element to read.</param>
        public static IReadOnlyDictionary<string, string?> ReadValues(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (string name in ReadableNames(element))
            {
                values[name] = Read(element, name);
            }

            return values;
        }

        /// <summary>
        /// Lists the names of every text parameter of an element that can be written,
        /// instance first, without repeats.
        /// </summary>
        /// <param name="element">The element to read.</param>
        public static IReadOnlyList<string> WritableTextNames(Element element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var names = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Collect(element, names, seen, writableOnly: true);
            Collect(TypeOf(element), names, seen, writableOnly: true);

            return names;
        }

        private static void Collect(Element? element, List<string> names, HashSet<string> seen, bool writableOnly)
        {
            if (element == null)
            {
                return;
            }

            foreach (Parameter parameter in element.Parameters)
            {
                if (parameter.Definition == null)
                {
                    continue;
                }

                if (writableOnly && (parameter.IsReadOnly || parameter.StorageType != StorageType.String))
                {
                    continue;
                }

                string name = parameter.Definition.Name;

                if (!string.IsNullOrWhiteSpace(name) && seen.Add(name))
                {
                    names.Add(name);
                }
            }
        }
    }
}
