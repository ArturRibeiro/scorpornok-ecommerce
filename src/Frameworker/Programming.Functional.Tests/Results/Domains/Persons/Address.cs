namespace Programming.Functional.Tests.Results.Domains.Persons
{
    public class Address
    {
        public string P1 { get; private set; }

        public string P2 { get; private set; }

        public string P3 { get; private set; }
        
        public static class Factory
        {
            public static Address Create(string p1, string p2, string p3)
                => new Address()
                {
                    P1 = p1,
                    P2 = p2,
                    P3 = p3
                };
        }


    }
}