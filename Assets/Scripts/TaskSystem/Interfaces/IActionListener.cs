namespace haw.pd20.tasksystem
{
    public interface IActionListener<T>
    {
        public void OnActionCompleted(T action);

        public void OnError(T action);

        public void OnHelpActivated(T action);
        
        public void OnHelpDeactivated(T action);
    }
}