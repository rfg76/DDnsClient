namespace DDnsClient
{
    public class HttpResponse
    {
        public int StatusCode { get; set; }
        public string StatusDescr { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return $"{StatusCode} - {StatusDescr}";
        }
    }
}
