namespace DAC.ObjectModels
{
    public class TMiceApplyUpdatesRequest
    {
        public string Token;
        public string KeyField;
        public TMiceExecutionContext ExecutionContext { get; set; }
        public TMiceApllyContent ApplyContext { get; set; }
        public TMiceApplyUpdatesRequest()
        {
            ApplyContext = new TMiceApllyContent();
            ExecutionContext = new TMiceExecutionContext();
        }

    }
}