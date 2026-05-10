namespace haw.pd20.events
{
    public class DefaultErrorEvent : GameEvent
    {
        public string ErrorMessage;
        
        public DefaultErrorEvent(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}