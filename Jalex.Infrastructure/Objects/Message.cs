namespace Jalex.Infrastructure.Objects
{
    public class Message
    {
        public Severity Severity { get; set; }
        public string Content { get; set; }

        public Message()
        {
            
        }

        public Message(Severity severity, string content)
        {
            Severity = severity;
            Content = content;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Severity, Content);
        }
    }
}
