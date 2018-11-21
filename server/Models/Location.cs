using System;
using System.Collections.Generic;

namespace DocumentAnnotation.Models
{
    public class Location : IComparable<Location>, IComparable
    {
        public Location(int bookNumber, int sectionNumber, int groupNumber, int wordNumber)
        {
            BookNumber = bookNumber;
            SectionNumber = sectionNumber;
            GroupNumber = groupNumber;
            WordNumber = wordNumber;
            Exact = true;
        }

        public Location(int bookNumber, int sectionNumber)
        {
            BookNumber = bookNumber;
            SectionNumber = sectionNumber;
            Exact = false;
        }

        public Location()
        {
        }

        public int CompareTo(Location other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var bookNumberComparison = BookNumber.CompareTo(other.BookNumber);
            if (bookNumberComparison != 0) return bookNumberComparison;
            var sectionNumberComparison = SectionNumber.CompareTo(other.SectionNumber);
            if (sectionNumberComparison != 0) return sectionNumberComparison;

            // Non-exact locations sort after exact ones
            if (!other.Exact) return 1;
            if (!Exact) return -1;

            var groupNumberComparison = GroupNumber.CompareTo(other.GroupNumber);
            if (groupNumberComparison != 0) return groupNumberComparison;
            return WordNumber.CompareTo(other.WordNumber);
        }

        public override string ToString()
        {
            if (Exact)
                return $"Book: {BookNumber}, Section: {SectionNumber}, Group: {GroupNumber}, Word: {WordNumber}";
            return $"Book: {BookNumber}, Section: {SectionNumber}";
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            if (ReferenceEquals(this, obj)) return 0;
            if (!(obj is Location)) throw new ArgumentException($"Object must be of type {nameof(Location)}");
            return CompareTo((Location) obj);
        }

        public static bool operator <(Location left, Location right)
        {
            return Comparer<Location>.Default.Compare(left, right) < 0;
        }

        public static bool operator >(Location left, Location right)
        {
            return Comparer<Location>.Default.Compare(left, right) > 0;
        }

        public static bool operator <=(Location left, Location right)
        {
            return Comparer<Location>.Default.Compare(left, right) <= 0;
        }

        public static bool operator >=(Location left, Location right)
        {
            return Comparer<Location>.Default.Compare(left, right) >= 0;
        }

        public int BookNumber { get; set; }
        public int SectionNumber { get; set;}
        public int GroupNumber { get;set; }
        public int WordNumber { get;set; }

        public bool Exact { get; set;}
    }
}