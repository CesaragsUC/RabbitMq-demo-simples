namespace Rabbit.Exchanges.Utils
{
    public class ResponseMessage
    {
        public ResponseMessage(string msg = "")
        {
            Errors ??= new List<string>();

            Message = !string.IsNullOrEmpty(msg) ? msg : string.Empty;
        }

        public string Message { get; set; }
        public List<string> Errors { get; set; } 
    }
}
