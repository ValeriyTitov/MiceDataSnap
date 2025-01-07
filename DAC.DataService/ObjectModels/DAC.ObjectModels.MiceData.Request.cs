namespace DAC.ObjectModels
{

    public class TMiceDataRequest
    {

        public TMiceExecutionContext ExecutionContext { get; set; }
        public TMiceDataRequest()
        {
            ExecutionContext = new TMiceExecutionContext();
        }

    }
}