﻿namespace Shared.Code.Models
{
    /// <summary>
    /// Referencia: https://docs.microsoft.com/pt-br/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
    /// </summary>
    public abstract class ValueObject
    {
        protected static bool EqualOperator(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
                return false;
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right)
            => !(EqualOperator(left, right));

        protected abstract IEnumerable<object> GetValues();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;

            ValueObject other = (ValueObject)obj;
            IEnumerator<object> thisValues = GetValues().GetEnumerator();
            IEnumerator<object> otherValues = other.GetValues().GetEnumerator();
            while (thisValues.MoveNext() && otherValues.MoveNext())
            {
                if (ReferenceEquals(thisValues.Current, null) ^ ReferenceEquals(otherValues.Current, null))
                    return false;
                if (thisValues.Current != null && !thisValues.Current.Equals(otherValues.Current))
                    return false;
            }
            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode()
            => GetValues()
             .Select(x => x != null ? x.GetHashCode() : 0)
             .Aggregate((x, y) => x ^ y);


        public ValueObject GetCopy()
            => this.MemberwiseClone() as ValueObject;
    }
}
