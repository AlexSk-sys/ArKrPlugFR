using System;

namespace ArKrPlugFR.Core.Joins
{
    /// <summary>
    /// A join between two elements, identified by their element ids.
    /// <para>
    /// The pair is normalised so that the smaller id comes first. Revit reports
    /// each join from both sides, and an unordered pair would otherwise appear
    /// twice in a report and be processed twice in a write operation.
    /// </para>
    /// </summary>
    public readonly struct JoinPair : IEquatable<JoinPair>, IComparable<JoinPair>
    {
        /// <summary>Creates a normalised pair.</summary>
        /// <param name="first">Id of one element.</param>
        /// <param name="second">Id of the other element.</param>
        public JoinPair(long first, long second)
        {
            if (first == second)
            {
                throw new ArgumentException("A join cannot reference the same element twice.", nameof(second));
            }

            if (first < second)
            {
                First = first;
                Second = second;
            }
            else
            {
                First = second;
                Second = first;
            }
        }

        /// <summary>Gets the smaller element id.</summary>
        public long First { get; }

        /// <summary>Gets the larger element id.</summary>
        public long Second { get; }

        /// <inheritdoc />
        public bool Equals(JoinPair other)
        {
            return First == other.First && Second == other.Second;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is JoinPair other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (First.GetHashCode() * 397) ^ Second.GetHashCode();
            }
        }

        /// <inheritdoc />
        public int CompareTo(JoinPair other)
        {
            int byFirst = First.CompareTo(other.First);
            return byFirst != 0 ? byFirst : Second.CompareTo(other.Second);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return First + " - " + Second;
        }

        /// <summary>Determines whether two pairs are the same join.</summary>
        public static bool operator ==(JoinPair left, JoinPair right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two pairs are different joins.</summary>
        public static bool operator !=(JoinPair left, JoinPair right)
        {
            return !left.Equals(right);
        }
    }
}
