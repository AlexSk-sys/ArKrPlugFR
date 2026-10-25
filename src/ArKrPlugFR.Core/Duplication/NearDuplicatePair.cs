using System;

namespace ArKrPlugFR.Core.Duplication
{
    /// <summary>
    /// Two types of one family that differ in only a few parameters.
    /// <para>
    /// These are not duplicates, but they are the most likely place for a copy to
    /// hide, so a scan that found nothing reports them as a diagnostic.
    /// </para>
    /// </summary>
    public sealed class NearDuplicatePair
    {
        /// <summary>Creates a pair.</summary>
        /// <param name="familyName">Name of the family both types belong to.</param>
        /// <param name="firstName">Name of the first type.</param>
        /// <param name="secondName">Name of the second type.</param>
        /// <param name="differenceCount">How many parameters the two types disagree on.</param>
        public NearDuplicatePair(string familyName, string firstName, string secondName, int differenceCount)
        {
            if (differenceCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(differenceCount), differenceCount, "A near duplicate differs in at least one parameter.");
            }

            FamilyName = familyName ?? string.Empty;
            FirstTypeName = firstName ?? string.Empty;
            SecondTypeName = secondName ?? string.Empty;
            DifferenceCount = differenceCount;
        }

        /// <summary>Gets the name of the family.</summary>
        public string FamilyName { get; }

        /// <summary>Gets the name of the first type.</summary>
        public string FirstTypeName { get; }

        /// <summary>Gets the name of the second type.</summary>
        public string SecondTypeName { get; }

        /// <summary>Gets how many parameters the two types disagree on.</summary>
        public int DifferenceCount { get; }
    }
}
