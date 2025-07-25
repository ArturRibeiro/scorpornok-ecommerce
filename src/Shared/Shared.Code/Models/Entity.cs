﻿namespace Shared.Code.Models
{
    public abstract class Entity<T>
    {
        private int? _requestedHashCode;
        private readonly IList<string> _errors = new List<string>();

        public virtual T Id
        {
            get;
            protected set;
        }

        public bool IsTransient => this.Id == null || this.Id.Equals(default(T));
        public IReadOnlyCollection<string> Errors => _errors.ToList().AsReadOnly();

        public virtual bool IsValid()
            => true;

        protected internal void SetValidation(ValidationResult validationResult)
        {
            foreach (var error in validationResult.Errors)
                _errors.Add(error.ErrorMessage);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Entity<T>))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            Entity<T> item = (Entity<T>)obj;
            
            if (item.IsTransient || this.IsTransient)
                return false;
            else
                return item.Id.Equals(this.Id);
        }

        public override int GetHashCode()
        {
            if (!IsTransient)
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

                return _requestedHashCode.Value;
            }
            else
                return base.GetHashCode();

        }
    }
}
